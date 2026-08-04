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
		Color color = _colorCvar.Value;
		int thickness = _thicknessCvar.Value;
		int length = _sizeCvar.Value;
		int gap = _gapCvar.Value;
		int halfThickness = thickness / 2;

		Vector2 verticalSize = new(thickness, length);
		Vector2 horizontalSize = new(length, thickness);

		// TOP
		_top.Color = color;
		_top.Size = verticalSize;
		_top.Position = new(-halfThickness, -gap - length);

		// BOT
		_bot.Color = color;
		_bot.Size = verticalSize;
		_bot.Position = new(-halfThickness, gap);

		// LEFT
		_left.Color = color;
		_left.Size = horizontalSize;
		_left.Position = new(-gap - length, -halfThickness);

		// RIGHT
		_right.Color = color;
		_right.Size = horizontalSize;
		_right.Position = new(gap, -halfThickness);

		// DOT
		_dot.Visible = _dotCvar.Value;
		_dot.Color = color;
		_dot.Size = new(thickness, thickness);
		_dot.Position = new(-halfThickness, -halfThickness);
	}
}
