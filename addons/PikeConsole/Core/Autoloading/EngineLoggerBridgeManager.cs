using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;
using Godot;

namespace FractalPike.PikeConsole.Core.Autoloading;

public partial class EngineLoggerBridgeManager : Node
{
	// Note: We need this cached so that the GC doesn't try to clean it.
	// The GC can't see the reference from accross the interop.
	EngineLoggerBridge _engineLogger = null;

	[Export] CVarBool _injectEngineLogs;

	public override void _EnterTree()
	{
		// We are responsible for initializing since this is not in the dedicated CVar dir.
		// It's safe to accidentally call this more than once (though we shouldn't)
		_injectEngineLogs.Initialize();
		_injectEngineLogs.ValueChanged += OnInteropEnabledChanged;

		ActivateInteropLogger();
	}

	public override void _ExitTree()
	{
		// Note, we need to remove the event listener AFTER running killinterop.
		// This is in case the interoplogger state is not aligned with the CVar and need to run the delegate for removal (edge case).
		KillInteropLogger();
		_injectEngineLogs.ValueChanged -= OnInteropEnabledChanged;
	}

	private void OnInteropEnabledChanged(bool enable)
	{
		if (enable)
			ActivateInteropLogger();
		else
			KillInteropLogger();
	}

	void OnNewInteropValue(bool newValue)
	{
		if (newValue)
			ActivateInteropLogger();
		else
			KillInteropLogger();
	}

	public void ActivateInteropLogger()
	{
		if (_engineLogger != null || _injectEngineLogs.Value == false)
			return;

		_engineLogger = new EngineLoggerBridge();
		OS.AddLogger(_engineLogger);

		PikeLogger.Log(LogTarget.All, $"[PikeConsole] Interop connection established. Engine exceptions and warnings are logged.");
	}

	public void KillInteropLogger()
	{
		if (_injectEngineLogs.Value)
		{
			// IMPORTANT:
			// We want the CVar to be in sync with the current state. But since setting the value triggers this 
			// exact method, we return and let the event delegate manage the removal. This avoids weird double-kill conditions.
			_injectEngineLogs.SetRAM(false);
			PikeLogger.LogWarning(LogTarget.All, $"Force-setting \"{_injectEngineLogs.ResourcePath.GetFile().GetBaseName()}\" was forcefully set to false.");
			return;
		}

		if (_engineLogger == null)
			return;

		OS.RemoveLogger(_engineLogger);
		_engineLogger.Dispose();
		_engineLogger = null;

		PikeLogger.Log(LogTarget.All, $"[PikeConsole] Interop connection severed. Engine exceptions are no longer logged.");
	}
}
