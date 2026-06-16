using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FractalPike.PikeConsole.Config;

namespace FractalPike.PikeConsole.Core.Logging;

/*
	This class routes logs to log receivers. Nothing more.
	Logs are not throttled or queued. They are sent to receivers or the in-engine GD.Print immedietly.
	It's then up to each individual subscriber to handle filtering.
	This class does, however, execute logs by target and make sure to no-op logs that are not for the current environment.

	Note: 
	All log calls use [CallerFilePath] and [CallerLineNumber] to allow for easier throttling and debugging by other systems.
	To my understanding this is compiler magic and not reflection, so it should be good. Do not spam logs in the hot path anyway.


	VERY IMPORTANT NOTE!!!
	
		PikeLogger is strictly a logging utility. 
		It should NEVER, EVER, manage or trigger a GD.PushError or GD.PushWarning.
		Since the "EngineLoggerBridge" routes logs through PikeLogger that would cause infinite recursion.
		"EngineLoggerBridge" does not subscribe to messages, so prints are fine.

	IS THIS THREAD SAFE?

		Yes. PikeLogger is safe to call from any thread.
		The default consumer (UI) extracts the logs using a thread safe queue.
		If you want to override the default consumer or add your own you MUST assume LogEmitted is called from any thread.
		It is connected to the Engine which is inherently multi-threaded.
*/

public static class PikeLogger
{

	private static readonly object _syncRoot = new();

	/// <summary>
	/// <para>Universal log emitter event. The in-game console subscribes to this event.</para>
	/// <para>Note: Since this logger routes errors and warnings from the Godot engine there is a chance 
	/// this event is emitted from another thread. The consumer MUST threrefore be thread safe,
	/// either with "Callable.From" or using a thread safe queue that is consumed on the main thread.</para>
	/// </summary>
	/// <remarks>
	/// The struct is sent as a reference. To consume it you must use the <c>in</c> keyword!
	/// <code>
	/// void OnLogEmitted(in LogEvent logEvent)
	/// </code>
	/// </remarks>
	public static event LogEventHandler LogEmitted;

	static bool? _isDebugEnvironment = null;

	// TODO: Centralize environment / system information later. Can also be used with the commands
	/// <summary>
	/// Debug environment flag cached after first call. 
	/// Uses lazy initialization so that we only cross the interop bridge once per lifetime.
	/// </summary>
	/// <returns>True for debug environments, false for strictly runtime environments.</returns>
	public static bool IsDebugEnvironment => _isDebugEnvironment ??= Godot.OS.IsDebugBuild();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static bool IsEditor()
	{
		// This helps us avoid race conditions AND INTEROP OVERHEAD with "Godot.Engine.IsEditorHint()"
		// If we are playtesting in the editor, this is true. When building the game it is stripped.
#if TOOLS
		return true;
#else
    	return false;
#endif
	}

	/// <summary>
	/// Used by subsystems, like the custom string builder to no-op on invalid environments.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsTargetEnabled(LogTarget target)
	{
		bool debugActive = (target & LogTarget.Debug) != 0 && IsDebugEnvironment;
		bool editorActive = (target & LogTarget.Editor) != 0 && IsEditor();
		bool runtimeActive = PikeConsoleConfig.ConsoleLoggerEnabled.Value && (target & LogTarget.Runtime) != 0;
		return debugActive || runtimeActive || editorActive;
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

		// Filter out any empty logs. We do this because if a system blindly logs $"{response.Message}" for something that is null,
		// we just spam empty containers for no reason.
		if (string.IsNullOrWhiteSpace(message)) return;

		// Locks the rest of the execution so that only one thread can run the method at a time.
		// Note: We still need to consume the events in a thread safe manner!
		lock (_syncRoot)
		{



#if TOOLS
			// ----- GODOT EDITOR -----
			if ((logTarget & LogTarget.Editor) != 0 && IsDebugEnvironment)
			{

				// Fix backslashes for windows systems.
				filePath = filePath.Replace('\\', '/');

				// Replace the potential path map with a localized version so that Godot can recognize the string as a file.
				// NOTE: We could implement ReadOnlySpan<char> here to only allocate once, but it would make it less readable and only save a few nanoseconds.
				if (!string.IsNullOrEmpty(PikeConsoleConfig.PathMap) && filePath.StartsWith(PikeConsoleConfig.PathMap))
					filePath = filePath.Replace(PikeConsoleConfig.PathMap, "res:/");
				else // If we have no PathMap alias force-inverse the path into local. Note: This crosses the interop bridge. 
					filePath = Godot.ProjectSettings.LocalizePath(filePath);

				// If we do not have two leading slashes, add the extra so Godot can parse the link correctly.
				if (filePath.StartsWith("res:/") && !filePath.StartsWith("res://"))
					filePath = filePath.Replace("res:/", "res://");

				// Log the message
				switch (logLevel)
				{
					// Code is non-DRY by design. We're keeping allocations and callstack dives low to keep a low profile on the editor playtest-environment.
					// Since the logic is static this is all the repetition we need, and this will all be stripped in compiled builds anyway.
					// It's ugly, but it's also a fair trade to keep a low footprint.
					case LogLevel.Info:
						if (includePath)
							Godot.GD.PrintRich($"[color=#{PikeConsoleConfig.InfoColor.ToHtml(false)}][url={filePath}:{lineNumber}]{filePath}:{lineNumber}[/url]:{memberName} - {message}");
						else
							Godot.GD.PrintRich($"[color=#{PikeConsoleConfig.InfoColor.ToHtml(false)}]{message}[/color]");
						break;
					case LogLevel.Success:
						if (includePath)
							Godot.GD.PrintRich($"[color=#{PikeConsoleConfig.SuccessColor.ToHtml(false)}][url={filePath}:{lineNumber}]{filePath}:{lineNumber}[/url]:{memberName} - {message}");
						else
							Godot.GD.PrintRich($"[color=#{PikeConsoleConfig.SuccessColor.ToHtml(false)}]{message}[/color]");
						break;
					case LogLevel.Warning:
						if (includePath)
							Godot.GD.PrintRich($"[color=#{PikeConsoleConfig.WarningColor.ToHtml(false)}][b]WARNING[/b]: [url={filePath}:{lineNumber}]{filePath}:{lineNumber}[/url]:{memberName} - {message}");
						else
							Godot.GD.PrintRich($"[color=#{PikeConsoleConfig.WarningColor.ToHtml(false)}][b]WARNING[/b]: {message}");
						break;
					case LogLevel.Error:
						if (includePath)
							Godot.GD.PrintRich($"[color=#{PikeConsoleConfig.ErrorColor.ToHtml(false)}][b]ERROR[/b]: [url={filePath}:{lineNumber}]{filePath}:{lineNumber}[/url]:{memberName} - {message}");
						else
							Godot.GD.PrintRich($"[color=#{PikeConsoleConfig.ErrorColor.ToHtml(false)}][b]ERROR[/b]: {message}");
						break;
					case LogLevel.Engine_Warning:
						if (includePath)
							Godot.GD.PrintRich($"[color=#{PikeConsoleConfig.WarningColor.ToHtml(false)}][b][ENGINE] WARNING[/b]: [url={filePath}:{lineNumber}]{filePath}:{lineNumber}[/url]:{memberName} - {message}");
						else
							Godot.GD.PrintRich($"[color=#{PikeConsoleConfig.WarningColor.ToHtml(false)}][b][ENGINE] WARNING[/b]: {message}");
						break;
					case LogLevel.Engine_Error:
						if (includePath)
							Godot.GD.PrintRich($"[color=#{PikeConsoleConfig.ErrorColor.ToHtml(false)}][b][ENGINE] ERROR[/b]: [url={filePath}:{lineNumber}]{filePath}:{lineNumber}[/url]:{memberName} - {message}");
						else
							Godot.GD.PrintRich($"[color=#{PikeConsoleConfig.ErrorColor.ToHtml(false)}][b][ENGINE] ERROR[/b]: {message}");
						break;
				}
			}

			// Still inside the preprocessor directives, we make an early return.
			// The editor simply checks if it was the only target, and early returns if it was.
			// This makes it so that we don't have to leave nasty extra checks in a compiled version.
			if ((logTarget & LogTarget.Runtime) == 0 && (logTarget & LogTarget.Debug) == 0)
				return;
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
	/// <param name="logLevel">The severity of the log. (exposed for edge-case manual overrides)</param>
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
		LogLevel logLevel = LogLevel.Info,
		bool forceLog = false,
		string domain = "",
		bool includePath = false,
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0,
		[CallerMemberName] string memberName = "")
	{
		LogInternal(logTarget, ref handler, logLevel, forceLog, domain, includePath, filePath, lineNumber, memberName);
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
