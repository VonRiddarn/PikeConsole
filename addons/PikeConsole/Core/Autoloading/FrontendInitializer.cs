using System;
using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.Utilities;
using Godot;

namespace FractalPike.PikeConsole.Core.Autoloading;

public partial class FrontendInitializer : Node
{
	// Pointer that allows the frontend UI to just go "Parent.FrontendInitializer.LogStartupCache".
	// It's a little hacky, but it gives us a reliable singleton without having to reference backend stuff.
	[Export] public LogStartupCache LogStartupCache { get; private set; }

	public override void _EnterTree()
	{
		if (LogStartupCache == null)
		{
			PikeLogger.LogError(LogTarget.Editor, $"LogStartupCache is not set up through the editor in {Name}!");
			return;
		}
	}

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

			// Always consume no matter what. This also kills the LogStartupCache.
			var logs = LogStartupCache.Consume();

			// Then we can just try pushing the startup cache to the frontend.
			if (uiInstance is IConsoleFrontend frontend)
				frontend.PushStartupLogs(logs);
			else
				PikeLogger.LogWarning(LogTarget.Debug, $"Frontend console does not inherit \"IConsoleFrontend\". Startup logs are lost.");
		}
		catch (Exception err)
		{
			ForceDisableRuntimeLogger($"Frontend UI instantiation failed: {err.Message}");
			return;
		}

		PikeLogger.LogSuccess(LogTarget.Editor, $"[PikeConsole] Frontend UI successfully injected.", forceLog: true);
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
		PikeConsoleConfig.ConsoleLoggerEnabled.SetRAM(false);
		// TODO: When there is an interop logger CVAr, diasable it here.

		// Log warning last so that it doesn't get burried by potential feedbacks
		PikeLogger.LogWarning(LogTarget.Editor, $"{warningMessage} -- PikeConsole is running headless.", forceLog: true);

		LogStartupCache?.Kill();
	}
}
