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

	// TODO: Create QOL overrides, like LogInfo, LogError, LogSuccess...

	static void LogInternal(
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

	public static void Log()
	{
		// Connect to the LogInternal with some default params.
		// IncludePath def false
	}
	public static void LogSuccess()
	{
		// Connect to the LogInternal with some default params.
		// IncludePath def false
	}
	public static void LogWarning()
	{
		// Connect to the LogInternal with some default params.
		// IncludePath def true
	}
	public static void LogError()
	{
		// Connect to the LogInternal with some default params.
		// IncludePath def true
	}
}
