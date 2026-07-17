using Godot;

namespace FractalPike.PikeConsole.Frontend;

[GlobalClass]
public partial class HeaderOverride : Resource
{
	[Export] public string LogTag { get; set; } = string.Empty;
	[Export] public string Label { get; set; } = string.Empty;
	[Export] public Color Color { get; set; } = Colors.White;

	public HeaderOverride() { }

	public HeaderOverride(string logTag, string label, Color color)
	{
		LogTag = logTag;
		Label = label;
		Color = color;
	}
}