// using System;
using System;
using System.Runtime.CompilerServices;
using FractalPike.PikeConsole.Core.Logging.Models;

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
	/// Universal log emitter event. The in-game console subscribes to this event.
	/// </summary>
	/// <remarks>
	/// Godot engine logs are NOT routed through this logger. This logger 
	/// only serves to centralize and route developer made logs. Engine logs 
	/// are handled by a separate wrapper.
	/// </remarks>
	public static event Action<LogEvent> LogEmitted;

	public static bool UseRuntime { get; set; } = true;
	public static bool IsDebugEnvironment => Godot.OS.IsDebugBuild(); // TODO: Centralize environment / system information later. Can also be used with the commands

	/// <summary>
	/// Used by subsystems, like the custom string builder to no-op on invalid environments.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsTargetEnabled(LogTarget target)
	{
		bool debugActive = (target & LogTarget.Debug) != 0 && IsDebugEnvironment;
		bool runtimeActive = UseRuntime && (target & LogTarget.Runtime) != 0;
		return debugActive || runtimeActive;
	}

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

		// Route the message to the sinks.
		// ----- GODOT -----
		if ((logTarget & LogTarget.Debug) != 0 && IsDebugEnvironment)
		{
			switch (logLevel)
			{
				case LogLevel.Info:
				case LogLevel.Success:
					Godot.GD.Print(message);
					break;
				case LogLevel.Warning:
					Godot.GD.PushWarning(message);
					break;
				case LogLevel.Error:
					Godot.GD.PushError(message);
					break;
			}
		}

		// ----- EVENT LISTENERS (RUNTIME) -----
		if ((logTarget & LogTarget.Runtime) != 0 && UseRuntime)
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
