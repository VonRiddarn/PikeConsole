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
	[Export] CVarInt _roundnessCvar;
	[Export] CVarBool _dotCvar;
	[Export] CVarBool _outLineCvar;
	[Export] CVarInt _outLineThicknessCvar;
	[Export] CVarColor _outLineColorCvar;

	[ExportGroup("Nodes")]
	[Export] Panel _top;
	[Export] Panel _bot;
	[Export] Panel _left;
	[Export] Panel _right;
	[Export] Panel _dot;

	public override void _EnterTree()
	{
		_sizeCvar.ValueInvalidated += UpdateCrosshair;
		_thicknessCvar.ValueInvalidated += UpdateCrosshair;
		_gapCvar.ValueInvalidated += UpdateCrosshair;
		_colorCvar.ValueInvalidated += UpdateCrosshair;
		_roundnessCvar.ValueInvalidated += UpdateCrosshair;
		_dotCvar.ValueInvalidated += UpdateCrosshair;
		_outLineCvar.ValueInvalidated += UpdateCrosshair;
		_outLineThicknessCvar.ValueInvalidated += UpdateCrosshair;
		_outLineColorCvar.ValueInvalidated += UpdateCrosshair;
	}

	public override void _ExitTree()
	{
		_sizeCvar.ValueInvalidated -= UpdateCrosshair;
		_thicknessCvar.ValueInvalidated -= UpdateCrosshair;
		_gapCvar.ValueInvalidated -= UpdateCrosshair;
		_colorCvar.ValueInvalidated -= UpdateCrosshair;
		_roundnessCvar.ValueInvalidated -= UpdateCrosshair;
		_dotCvar.ValueInvalidated -= UpdateCrosshair;
		_outLineCvar.ValueInvalidated -= UpdateCrosshair;
		_outLineThicknessCvar.ValueInvalidated -= UpdateCrosshair;
		_outLineColorCvar.ValueInvalidated -= UpdateCrosshair;
	}

	public override void _Ready() => UpdateCrosshair();

	void UpdateCrosshair()
	{
		int thickness = _thicknessCvar.Value;
		int length = _sizeCvar.Value;
		int gap = _gapCvar.Value;
		int halfThickness = thickness / 2;

		Vector2 verticalSize = new(thickness, length);
		Vector2 horizontalSize = new(length, thickness);

		// TOP
		_top.Size = verticalSize;
		_top.Position = new(-halfThickness, -gap - length);

		// BOT
		_bot.Size = verticalSize;
		_bot.Position = new(-halfThickness, gap);

		// LEFT
		_left.Size = horizontalSize;
		_left.Position = new(-gap - length, -halfThickness);

		// RIGHT
		_right.Size = horizontalSize;
		_right.Position = new(gap, -halfThickness);

		// DOT
		_dot.Visible = _dotCvar.Value;
		_dot.Size = new(thickness, thickness);
		_dot.Position = new(-halfThickness, -halfThickness);

		// STYLING (Color, outline etc)
		int cornerRadius = _roundnessCvar.Value;
		StyleBoxFlat crosshairStyling = new()
		{
			BgColor = _colorCvar.Value,
			CornerRadiusTopLeft = cornerRadius,
			CornerRadiusTopRight = cornerRadius,
			CornerRadiusBottomLeft = cornerRadius,
			CornerRadiusBottomRight = cornerRadius,
		};


		if (_outLineCvar.Value)
		{
			int outLineThickness = _outLineThicknessCvar.Value;

			crosshairStyling.BorderColor = _outLineColorCvar.Value;
			crosshairStyling.SetBorderWidthAll(outLineThickness);
			crosshairStyling.SetExpandMarginAll(outLineThickness);
		}

		_top.AddThemeStyleboxOverride("panel", crosshairStyling);
		_bot.AddThemeStyleboxOverride("panel", crosshairStyling);
		_left.AddThemeStyleboxOverride("panel", crosshairStyling);
		_right.AddThemeStyleboxOverride("panel", crosshairStyling);
		_dot.AddThemeStyleboxOverride("panel", crosshairStyling);
	}
}
