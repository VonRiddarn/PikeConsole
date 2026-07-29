using System;
using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars.Extensions;
using FractalPike.PikeConsole.Core.Utilities;
using Godot;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;

[GlobalClass]
public partial class CVarColor : CVarBase<Color>
{
	public override string DisplayType => "CVar_Color";

	[Export]
	protected override Color _defaultValue { get; set; } = Colors.White;

	[Export]
	protected override Color _value { get; set; } = Colors.White;

	protected override Response<CvarSetResponseStatus, Color> ParseValue(ReadOnlySpan<string> args)
	{
		if (!ArgumentParser.TryParseColor(args, out Color value, out string error))
			return new(CvarSetResponseStatus.Failed, default, error);

		if (Value == value)
			return new(CvarSetResponseStatus.NoChange, value);

		return new(CvarSetResponseStatus.Success, value, default);
	}

	// ----- ----- ----- -----
	//	HELPERS AND OVERRIDES
	// ----- ----- ----- -----

	// CRITICAL INFORMATION: Formatted value on CVars must be 2 way parseable!!
	// We MUST override the method here, as the default Color.ToString returns a CSV of the RGB, which we cannot parse.
	// Thus, we will save the colors using .ToHtml(), which gives a hex value.
	public override string FormattedValue => $"#{_value.ToHtml()}";

	// This is just used to display the value in a cool / readable way.
	public override string DisplayValue(Color value) => $"({value.R8}, {value.G8}, {value.B8}, {value.A8}) | #{value.ToHtml()}";

	public override string[] Usages =>
	[
		$"{Signature} [hex value]",
		$"{Signature} [Red 0-255] [Green 0-255] [Blue 0-255] [Alpha? 0-255]"
	];
}
