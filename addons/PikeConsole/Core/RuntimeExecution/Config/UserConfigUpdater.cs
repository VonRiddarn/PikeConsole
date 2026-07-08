using FractalPike.PikeConsole.Config;
using Godot;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Config;

public partial class UserConfigUpdater : Node
{
	public override void _EnterTree()
	{
		if (PikeConsoleConfig.UserConfigsEnabled)
			PersistentCVarRegistry.ValueUpdated += OnCVarChanged;

	}

	public override void _ExitTree()
	{
		if (PikeConsoleConfig.UserConfigsEnabled)
			PersistentCVarRegistry.ValueUpdated -= OnCVarChanged;
	}

	private void OnCVarChanged(ICVar _)
	{
		// Debounce the save
	}
}
