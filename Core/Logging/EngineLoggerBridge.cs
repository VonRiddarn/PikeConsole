using Godot;
using System;

namespace FractalPike.PikeConsole.Core.Logging;

public partial class EngineLoggerBridge : Logger
{
	// DOCS: https://docs.godotengine.org/en/stable/classes/class_logger.html
	// I'm just hardcoding a const so it's readable
	const int ENGINE_WARNING_TYPE = 1;

	private readonly object _lock = new();

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
		// TODO: Add nullorempty checks for rationale and code. I accidentally used javascript logic...
		if (errorType == ENGINE_WARNING_TYPE)
			PikeLogger.LogWarning(LogTarget.All, $"[GODOT ENGINE] {rationale ?? code ?? "Unknown engine warning!"}");
		else
			PikeLogger.LogError(LogTarget.All, $"[GODOT ENGINE] {rationale ?? code ?? "Unknown engine error!"}");
	}
}
