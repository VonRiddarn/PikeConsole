using System;
using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars.Extensions;
using Godot;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;

[GlobalClass]
public partial class CVarBool : CVarBase<bool>
{
	public override string DisplayType => "CVar_Bool";

	[Export]
	protected override bool _defaultValue { get; set; }

	[Export]
	protected override bool _value { get; set; }

	protected override Response<CvarSetResponseStatus> SetValue(ReadOnlySpan<string> args)
	{
		if (!ArgumentParser.ValidateCount(args, 1, out string error))
			return new(CvarSetResponseStatus.InvalidArgs, error);

		if (!ArgumentParser.TryParseBool(args[0], out bool value, out error))
			return new(CvarSetResponseStatus.Failed, error);

		if (Value == value)
			return new(CvarSetResponseStatus.NoChange, null);

		Value = value;
		return new(CvarSetResponseStatus.Success, null);
	}
}
