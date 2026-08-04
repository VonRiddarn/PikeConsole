using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;
using Godot;

namespace FractalPike.PikeConsole.Examples;

public partial class Crosshair : Control
{
	[ExportGroup("CVars")]
	[Export] CVarInt _sizeCvar;
	[Export] CVarInt _thicknessCvar;
	[Export] CVarInt _gapCvar;
	[Export] CVarColor _colorCvar;
	[Export] CVarBool _dotCvar;

	[ExportGroup("Nodes")]
	[Export] ColorRect _top;
	[Export] ColorRect _bot;
	[Export] ColorRect _left;
	[Export] ColorRect _right;
	[Export] ColorRect _dot;

	public override void _EnterTree()
	{
		_sizeCvar.ValueInvalidated += UpdateCrosshair;
		_thicknessCvar.ValueInvalidated += UpdateCrosshair;
		_gapCvar.ValueInvalidated += UpdateCrosshair;
		_colorCvar.ValueInvalidated += UpdateCrosshair;
		_dotCvar.ValueInvalidated += UpdateCrosshair;

		UpdateCrosshair();
	}

	public override void _ExitTree()
	{
		_sizeCvar.ValueInvalidated -= UpdateCrosshair;
		_thicknessCvar.ValueInvalidated -= UpdateCrosshair;
		_gapCvar.ValueInvalidated -= UpdateCrosshair;
		_colorCvar.ValueInvalidated -= UpdateCrosshair;
		_dotCvar.ValueInvalidated -= UpdateCrosshair;
	}

	void UpdateCrosshair()
	{
		PikeLogger.Log(LogTarget.Editor, $"Crosshair printed");
	}
}
