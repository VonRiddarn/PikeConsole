// using System;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FractalPike.PikeConsole.Core.Logging;

/*
	This class routes logs to log receivers. Nothing more.
	Logs are not throttled or queued. They are sent to receivers or the in-engine GD.Print immedietly.
	It's then up to each individual subscriber to handle filtering.
	This class does, however, execute logs by target and make sure to no-op logs that are not for the current environment.

	Note: 
	All log calls use [CallerFilePath] and [CallerLineNumber] to allow for easier throttling and debugging by other systems.
	To my understanding this is compiler magic and not reflection, so it should be good. Do not spam logs in the hot path anyway.

	Q: Why are there no method overrides for string literals? 
	A:
		Because a literal can be a silent killer if not used correctly. 
		Using a literal with pure quotations will compile fine and not introduce overhead.
		However, a lot of people use string concatenation in stead of a literal, which cannot be caught
		and be turned into a no-op. 

		Safe, free: "Hello world"
		Unsafe, potentially expensive: "Hello " + GetPlanet()

		Thus, I made the decision of only allowing interpolated strings with a custom string builder.
		This prevents accidental allocation and computation.
*/

public static class PikeLogger
{
	/// <summary>
	/// <para>Universal log emitter event. The in-game console subscribes to this event.</para>
	/// <para>Godot engine logs are NOT routed through this logger. This logger 
	/// only serves to centralize and route developer made logs. Engine logs 
	/// are handled by a separate wrapper.</para>
	/// </summary>
	/// <remarks>
	/// NOTE:<br/>
	/// The struct is sent as a reference. To consume it you must use the <c>in</c> keyword!
	/// <code>
	/// void OnLogEmitted(in LogEvent logEvent)
	/// </code>
	/// </remarks>
	public static event LogEventHandler LogEmitted; // TODO: Consume using thread safe queue!!

	static bool? _isDebugEnvironment = null;

	// TODO: Centralize environment / system information later. Can also be used with the commands
	/// <summary>
	/// Debug environment flag cached after first call. 
	/// Uses lazy initialization so that we only cross the interop bridge once per lifetime.
	/// </summary>
	/// <returns>True for debug environments, false for strictly runtime environments.</returns>
	public static bool IsDebugEnvironment => _isDebugEnvironment ??= Godot.OS.IsDebugBuild();

	/// <summary>
	/// Used by subsystems, like the custom string builder to no-op on invalid environments.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsTargetEnabled(LogTarget target)
	{
		bool debugActive = (target & LogTarget.Debug) != 0 && IsDebugEnvironment;
		bool runtimeActive = PikeConsoleConfig.EnableRuntimeLogging && (target & LogTarget.Runtime) != 0;
		return debugActive || runtimeActive;
	}

	[StackTraceHidden]
	static void LogInternal(
		LogTarget logTarget,
		[InterpolatedStringHandlerArgument("logTarget")] ref LogInterpolatedStringHandler handler,
		LogLevel logLevel,
		bool forceLog = false,
		string domain = "",
		bool includePath = false,
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0,
		[CallerMemberName] string memberName = "")
	{
		// Early return pattern. We are not logging in this environment.
		if (!IsTargetEnabled(logTarget)) return;

		// Build the message from the interpolationhandler.
		string message = handler.ToStringAndClear();

#if TOOLS
		// ----- GODOT EDITOR -----
		if ((logTarget & LogTarget.Debug) != 0 && IsDebugEnvironment)
		{
			// Fix backslashes for windows systems.
			filePath = filePath.Replace('\\', '/');

			// Replace the potential path map with a localized version so that Godot can recognize the string as a file.
			// NOTE: We could implement ReadOnlySpan<char> here to only allocate once, but it would make it less readable and only save a few nanoseconds.
			if (!string.IsNullOrEmpty(PikeConsoleConfig.PATH_MAP_ALIAS) && filePath.StartsWith(PikeConsoleConfig.PATH_MAP_ALIAS))
				filePath = filePath.Replace(PikeConsoleConfig.PATH_MAP_ALIAS, "res:/");
			else // If we have no PathMap alias force-inverse the path into local. Note: This crosses the interop bridge. 
				filePath = Godot.ProjectSettings.LocalizePath(filePath);

			// If we do not have two leading slashes, add the extra so Godot can parse the link correctly.
			if (filePath.StartsWith("res:/") && !filePath.StartsWith("res://"))
				filePath = filePath.Replace("res:/", "res://");

			// Log the message
			switch (logLevel)
			{
				// Code is non-DRY by design. Keeping manual interpolated strings here skips a dive in the callstack.
				// Meaning, we do not allocate a member variable, nor do we interpolate more than once.
				case LogLevel.Info:
					Godot.GD.PrintRich($"[color={PikeConsoleConfig.COLOR_INFO}]{message}[/color]");
					break;
				case LogLevel.Success:
					Godot.GD.PrintRich($"[color={PikeConsoleConfig.COLOR_SUCCESS}]{message}[/color]");
					break;
				case LogLevel.Warning:
					Godot.GD.PrintRich($"[color={PikeConsoleConfig.COLOR_WARNING}][b]WARNING[/b]: [url={filePath}:{lineNumber}]{filePath}:{lineNumber}[/url]:{memberName} - {message}");
					break;
				case LogLevel.Error:
					Godot.GD.PrintRich($"[color={PikeConsoleConfig.COLOR_ERROR}][b]ERROR[/b]: [url={filePath}:{lineNumber}]{filePath}:{lineNumber}[/url]:{memberName} - {message}");
					break;
			}
		}
#endif

		// ----- EVENT LISTENERS (ALL ENVIRONMENTS) -----
		LogEmitted?.Invoke(new LogEvent(
			HashCode.Combine(filePath, lineNumber, memberName),
			logLevel,
			message,
			forceLog,
			domain,
			includePath ? $"{filePath}:{lineNumber}:{memberName}" : string.Empty
		));
	}

	// Public API - outward facing wrapper methods.
	// And here goes documentation hell...

	/// <summary>
	/// Logs information to the console with close to zero allocation when not running on the targeted environment.
	/// </summary>
	/// <remarks>
	/// Important note: All logs going through PikeLogger MUST be interpolated strings. Even if they are literals.
	/// This is an opinionated design choice in place that makes it harder to accidentally allocate memory.
	/// </remarks>
	/// <param name="logTarget">The target environment</param>
	/// <param name="handler">Log message as interpolated string. Only built if this is the target environment.</param>
	/// <param name="forceLog">Flag that listeners can use to bypass log throttle.</param>
	/// <param name="domain">Optional domain parameter used for filtering. (EG: "FractalPike.Entities.AI")</param>
	/// <param name="includePath">If true the caller path is interpolated and added to LogEvent.SourcePath</param>
	/// <param name="filePath">COMPILER MANAGED VARIABLE, DO NOT TOUCH</param>
	/// <param name="lineNumber">COMPILER MANAGED VARIABLE, DO NOT TOUCH</param>
	/// <param name="memberName">COMPILER MANAGED VARIABLE, DO NOT TOUCH</param>
	[StackTraceHidden]
	public static void Log(
		LogTarget logTarget,
		[InterpolatedStringHandlerArgument("logTarget")] ref LogInterpolatedStringHandler handler,
		bool forceLog = false,
		string domain = "",
		bool includePath = false,
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0,
		[CallerMemberName] string memberName = "")
	{
		LogInternal(logTarget, ref handler, LogLevel.Info, forceLog, domain, includePath, filePath, lineNumber, memberName);
	}

	/// <summary>
	/// Logs a success message to the console with close to zero allocation when not running on the targeted environment.
	/// </summary>
	/// <remarks>
	/// Important note: All logs going through PikeLogger MUST be interpolated strings. Even if they are literals.
	/// This is an opinionated design choice in place that makes it harder to accidentally allocate memory.
	/// </remarks>
	/// <param name="logTarget">The target environment</param>
	/// <param name="handler">Log message as interpolated string. Only built if this is the target environment.</param>
	/// <param name="forceLog">Flag that listeners can use to bypass log throttle.</param>
	/// <param name="domain">Optional domain parameter used for filtering. (EG: "FractalPike.Entities.AI")</param>
	/// <param name="includePath">If true the caller path is interpolated and added to LogEvent.SourcePath</param>
	/// <param name="filePath">COMPILER MANAGED VARIABLE, DO NOT TOUCH</param>
	/// <param name="lineNumber">COMPILER MANAGED VARIABLE, DO NOT TOUCH</param>
	/// <param name="memberName">COMPILER MANAGED VARIABLE, DO NOT TOUCH</param>
	[StackTraceHidden]
	public static void LogSuccess(
		LogTarget logTarget,
		[InterpolatedStringHandlerArgument("logTarget")] ref LogInterpolatedStringHandler handler,
		bool forceLog = false,
		string domain = "",
		bool includePath = false,
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0,
		[CallerMemberName] string memberName = "")
	{
		LogInternal(logTarget, ref handler, LogLevel.Success, forceLog, domain, includePath, filePath, lineNumber, memberName);
	}

	/// <summary>
	/// Logs a warning to the console with close to zero allocation when not running on the targeted environment.
	/// </summary>
	/// <remarks>
	/// Important note: All logs going through PikeLogger MUST be interpolated strings. Even if they are literals.
	/// This is an opinionated design choice in place that makes it harder to accidentally allocate memory.
	/// </remarks>
	/// <param name="logTarget">The target environment</param>
	/// <param name="handler">Log message as interpolated string. Only built if this is the target environment.</param>
	/// <param name="forceLog">Flag that listeners can use to bypass log throttle.</param>
	/// <param name="domain">Optional domain parameter used for filtering. (EG: "FractalPike.Entities.AI")</param>
	/// <param name="includePath">If true the caller path is interpolated and added to LogEvent.SourcePath</param>
	/// <param name="filePath">COMPILER MANAGED VARIABLE, DO NOT TOUCH</param>
	/// <param name="lineNumber">COMPILER MANAGED VARIABLE, DO NOT TOUCH</param>
	/// <param name="memberName">COMPILER MANAGED VARIABLE, DO NOT TOUCH</param>
	[StackTraceHidden]
	public static void LogWarning(
		LogTarget logTarget,
		[InterpolatedStringHandlerArgument("logTarget")] ref LogInterpolatedStringHandler handler,
		bool forceLog = false,
		string domain = "",
		bool includePath = true,
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0,
		[CallerMemberName] string memberName = "")
	{
		LogInternal(logTarget, ref handler, LogLevel.Warning, forceLog, domain, includePath, filePath, lineNumber, memberName);
	}

	/// <summary>
	/// Logs an error to the console with close to zero allocation when not running on the targeted environment.
	/// </summary>
	/// <remarks>
	/// Important note: All logs going through PikeLogger MUST be interpolated strings. Even if they are literals.
	/// This is an opinionated design choice in place that makes it harder to accidentally allocate memory.
	/// </remarks>
	/// <param name="logTarget">The target environment</param>
	/// <param name="handler">Log message as interpolated string. Only built if this is the target environment.</param>
	/// <param name="forceLog">Flag that listeners can use to bypass log throttle.</param>
	/// <param name="domain">Optional domain parameter used for filtering. (EG: "FractalPike.Entities.AI")</param>
	/// <param name="includePath">If true the caller path is interpolated and added to LogEvent.SourcePath</param>
	/// <param name="filePath">COMPILER MANAGED VARIABLE, DO NOT TOUCH</param>
	/// <param name="lineNumber">COMPILER MANAGED VARIABLE, DO NOT TOUCH</param>
	/// <param name="memberName">COMPILER MANAGED VARIABLE, DO NOT TOUCH</param>
	[StackTraceHidden]
	public static void LogError(
		LogTarget logTarget,
		[InterpolatedStringHandlerArgument("logTarget")] ref LogInterpolatedStringHandler handler,
		bool forceLog = false,
		string domain = "",
		bool includePath = true,
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0,
		[CallerMemberName] string memberName = "")
	{
		LogInternal(logTarget, ref handler, LogLevel.Error, forceLog, domain, includePath, filePath, lineNumber, memberName);
	}
}
