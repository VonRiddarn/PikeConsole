using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;
using Godot;

namespace FractalPike.PikeConsole.Autoloading;

public partial class EngineLoggerBridgeManager : Node
{
	// Note: We need this cached so that the GC doesn't try to clean it.
	// The GC can't see the reference from accross the interop.
	EngineLoggerBridge _engineLogger = null;

	// Quick access ref to the CVar in PikeConsoleCvars.
	CVarBool _enabled = null;

	public override void _EnterTree()
	{
		_enabled = PikeConsoleStates.RuntimeConsoleEnabled;
		PikeConsoleStates.RuntimeConsoleEnabled.ValueChanged += OnRuntimeActiveChanged;

		ActivateInteropLogger();
	}

	public override void _ExitTree()
	{
		KillInteropLogger();
		PikeConsoleStates.RuntimeConsoleEnabled.ValueChanged -= OnRuntimeActiveChanged;
	}

	void OnRuntimeActiveChanged(bool enable)
	{
		if (enable)
			ActivateInteropLogger();
		else
			KillInteropLogger();
	}

	public void ActivateInteropLogger()
	{
		if (_engineLogger != null || _enabled.Value == false)
			return;

		_engineLogger = new EngineLoggerBridge();
		OS.AddLogger(_engineLogger);

		PikeLogger.Log(LogTarget.Editor, $"[PikeConsole] Interop connection established. Engine exceptions and warnings are logged.");
	}

	public void KillInteropLogger()
	{
		if (_engineLogger == null)
			return;

		OS.RemoveLogger(_engineLogger);
		_engineLogger.Dispose();
		_engineLogger = null;

		PikeLogger.Log(LogTarget.Editor, $"[PikeConsole] Interop connection severed. Engine exceptions are no longer logged.");
	}
}
