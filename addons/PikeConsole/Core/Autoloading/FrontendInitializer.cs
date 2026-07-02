using System;
using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.Autoloading;
using FractalPike.PikeConsole.Core.Logging;
using Godot;

public partial class FrontendInitializer : Node
{
	public override void _Ready()
	{
		string uiPath = PikeConsoleConfig.FrontendScenePath;

		if (string.IsNullOrEmpty(uiPath))
		{
			ForceDisableRuntimeLogger("Frontend UI path is empty in Project Settings.");
			return;
		}

		if (!ResourceLoader.Exists(uiPath))
		{
			ForceDisableRuntimeLogger($"Failed to load Frontend UI! No scene found at: {uiPath}");
			return;
		}

		try
		{
			// Kind of tryhard to put in try-catch, 
			// but since this is a public addon I don't want to risk weird user-induced type cast exceptions.
			PackedScene uiScene = ResourceLoader.Load<PackedScene>(uiPath);
			Node uiInstance = uiScene.Instantiate();
			AddChild(uiInstance);
		}
		catch (Exception err)
		{
			ForceDisableRuntimeLogger($"Frontend UI instantiation failed: {err.Message}");
			return;
		}

		PikeLogger.LogSuccess(LogTarget.Editor, $"[PikeConsole] Frontend UI successfully injected.", forceLog: true, domain: "PikeConsole");
	}

	void ForceDisableRuntimeLogger(string warningMessage)
	{
		// Fetch and kill the interop logger.
		var bridgeManager = GetNodeOrNull<EngineLoggerBridgeManager>("../EngineLoggerBridgeManager");
		if (bridgeManager != null)
			bridgeManager.KillInteropLogger();
		else
			PikeLogger.LogError(LogTarget.Editor, $"Could not find the EngineLoggerBridgeManager node to kill interop logger!");

		// Disable dogfed CVars
		PikeConsoleConfig.ConsoleLoggerEnabled.Value = false;
		// TODO: When there is an interop logger CVAr, diasable it here.

		// Log warning last so that it doesn't get burried by potential feedbacks
		PikeLogger.LogWarning(LogTarget.Editor, $"{warningMessage} -- PikeConsole is running headless.", forceLog: true);
	}
}
