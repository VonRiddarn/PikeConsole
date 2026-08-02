using FractalPike.PikeConsole.Config;
using Godot;

namespace FractalPike.PikeConsole.Frontend.Controllers;

public partial class StateController : Node
{
	[ExportGroup("Dependencies")]
	[Export] CanvasLayer _consoleUI;
	[Export] InputController _inputController;

	Input.MouseModeEnum _cachedMouseMode = 0;

	public override void _EnterTree() =>
		PikeConsoleStates.RuntimeConsoleEnabled.ValueChanged += OnEnabledChanged;


	public override void _ExitTree() =>
		PikeConsoleStates.RuntimeConsoleEnabled.ValueChanged -= OnEnabledChanged;


	public override void _Ready() =>
		CloseConsole();

	// If the console is open when the runtime is disabled, kill it.
	void OnEnabledChanged(bool newState)
	{
		if (PikeConsoleStates.ConsoleUIActive)
		{
			PikeConsoleStates.ConsoleUIActive = false;
			CloseConsole();
		}
	}

	public override void _Input(InputEvent e)
	{
		if (e.IsActionPressed(PikeConsoleSettings.ToggleConsoleActionName) && PikeConsoleStates.RuntimeConsoleEnabled.Value)
		{
			ToggleConsole();

			// This prevents "§" (or tilde on american keyboards) from being added to the input field when we focus.
			GetViewport().SetInputAsHandled();
		}
		// Jesus.
		else if (e is InputEventKey keyEvent && PikeConsoleStates.ConsoleUIActive && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
			CloseConsole();
	}

	void ToggleConsole()
	{
		if (PikeConsoleStates.ConsoleUIActive)
			CloseConsole();
		else
			OpenConsole();
	}

	void OpenConsole()
	{
		_cachedMouseMode = Input.MouseMode;
		Input.MouseMode = Input.MouseModeEnum.Visible;
		PikeConsoleStates.ConsoleUIActive = true;
		_consoleUI.Show();
		_inputController.GrabFocus();
	}

	void CloseConsole()
	{
		Input.MouseMode = _cachedMouseMode;
		PikeConsoleStates.ConsoleUIActive = false;
		_consoleUI.Hide();
		// _inputController.Clear(); -- Might want this later. Not sure.
	}
}
