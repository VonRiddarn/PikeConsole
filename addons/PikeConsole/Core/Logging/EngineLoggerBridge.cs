using Godot;
using System;

namespace FractalPike.PikeConsole.Core.Logging;

public partial class EngineLoggerBridge : Logger
{
	// DOCS: https://docs.godotengine.org/en/stable/classes/class_logger.html
	// I'm just hardcoding a const so it's readable
	const int ENGINE_WARNING_TYPE = 1;

	// NOTE: We currently only care about actual errors and warnings, not messages about them.
	// IF we define _LogMessage we WILL receive interop mashalling from Godot, even if we early return.
	// By not defining the method Godot skips marshalling at the engine level!

	public override void _LogError(
		string function,
		string file,
		int line,
		string code,
		string rationale,
		bool editorNotify,
		int errorType,
		Godot.Collections.Array<ScriptBacktrace> scriptBacktraces)
	{

		string message = string.IsNullOrEmpty(rationale) ? code : rationale;

		if (errorType == ENGINE_WARNING_TYPE)
			PikeLogger.Log(LogTarget.All, $"{(string.IsNullOrWhiteSpace(message) ? "Unknown engine warning!" : message)}",
			logLevel: LogLevel.Engine_Warning,
			domain: "Godot.Warnings",
			forceLog: true,
			filePath: file,
			lineNumber: line,
			includePath: true);
		else
			PikeLogger.Log(LogTarget.All, $"{(string.IsNullOrWhiteSpace(message) ? "Unknown engine error!" : message)}",
			logLevel: LogLevel.Engine_Error,
			domain: "Godot.Errors",
			forceLog: true,
			filePath: file,
			lineNumber: line,
			includePath: true);
	}
}
