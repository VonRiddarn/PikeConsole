using System;
using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.Utilities;
using Godot;

namespace FractalPike.PikeConsole.Autoloading;

public partial class FrontendInitializer : Node
{
	public override void _Ready()
	{
		string uiPath = PikeConsoleSettings.FrontendScenePath;

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

			// Always consume no matter what. This also kills the StartupLogBuffer.
			// Doing this in the initializer makes sure that the logbuffer is killed. 
			// Leaving this up to the user would result in memory leaks if they forget to consume!
			var logs = StartupLogBuffer.Consume();

			// Then we can just try pushing the startup cache to the frontend.
			// We recursively go through all children of the ui to see if there is a consumer.
			// This only happens at startup, so it's fine.
			if (TryFindConsumer(uiInstance, out IStartupLogConsumer consumer))
				consumer.ConsumeStartupLogs(logs);
			else
				PikeLogger.LogWarning(LogTarget.Debug, $"Frontend console does not inherit \"IStartupLogConsumer\". Startup logs are lost.");
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
		PikeConsoleStates.RuntimeConsoleEnabled.SetRAM(false);

		// Log warning last so that it doesn't get burried by potential feedbacks
		PikeLogger.LogWarning(LogTarget.Editor, $"{warningMessage} -- PikeConsole is running headless.", forceLog: true);

		// Still call consume to kill off the buffer and prevent memory leaks!
		var _ = StartupLogBuffer.Consume();
	}


	static bool TryFindConsumer(Node root, out IStartupLogConsumer result)
	{
		if (root is IStartupLogConsumer consumer)
		{
			result = consumer;
			return true;
		}

		// If not found, recursively check all children
		foreach (Node child in root.GetChildren())
		{
			if (TryFindConsumer(child, out result))
				return true;
		}

		result = null;
		return false;
	}
}
