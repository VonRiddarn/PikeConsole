using FractalPike.PikeConsole.Core.Logging;
using Godot;

namespace FractalPike.PikeConsole.Core.Autoloading;

public partial class EngineLoggerBridgeManager : Node
{
	// Note: We need this cached so that the GC doesn't try to clean it.
	// The GC can't see the reference from accross the interop.
	EngineLoggerBridge _engineLogger = null;

	public override void _EnterTree()
	{
		// CVar.ChangedValue += OnNewInteropValue;
		ActivateInteropLogger();
		GD.Print("Does not trigger log bridge.");
		GD.PrintErr("Will trigger log bridge.");

		// Triggers log bridge!
		GD.PushWarning("What's going on?");
		GD.PushError("OH NO! THE HUMANITY!");
		PikeLogger.Log(LogTarget.All, $"TEst");
	}

	public override void _ExitTree()
	{
		KillInteropLogger();
	}

	void OnNewInteropValue(bool newValue)
	{
		if (newValue)
			ActivateInteropLogger();
		else
			KillInteropLogger();
	}

	// TODO: Create internal CVars that manage this at runtime. 
	// NOTE TO SELF: If we create internal CVars we are also responsible for initializing them. Use [Export] for the CVars.
	public void ActivateInteropLogger()
	{
		if (_engineLogger != null) // || console_inject_engine_logs (CVar) false
			return;

		_engineLogger = new EngineLoggerBridge();
		OS.AddLogger(_engineLogger);

		// TODO: Look into how Godot handles crashes (both engine and .NET environment)
		// We could sub to AppDomain.CurrentDomain.UnhandledException here. 
		// Though the UI would not be able to see it before the game dies, we 
		// could still force-empty the buffer to some file for crash reports. 
		// (Unless Godot already does that natively)

		PikeLogger.Log(LogTarget.All, $"[PikeConsole] interop connection established. Engine exceptions and warnings are logged.");
	}

	public void KillInteropLogger()
	{
		if (_engineLogger == null) // || console_inject_engine_logs (CVar) true
			return;

		OS.RemoveLogger(_engineLogger);
		_engineLogger.Dispose();
		_engineLogger = null;

		PikeLogger.Log(LogTarget.All, $"[PikeConsole] interop connection severed. Engine exceptions are no longer logged.");
	}
}
