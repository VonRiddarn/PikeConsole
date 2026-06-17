using System;
using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars.Extensions;
using Godot;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;

[GlobalClass]
public partial class CVarInt : CVarBase<int>
{
	public override string DisplayType => "CVar_Int";

	[Export]
	protected override int _defaultValue { get; set; }
	[Export]
	protected override int _value { get; set; }

	[ExportGroup("Limits")]
	[ExportSubgroup("Minimum")]
	[Export] bool _useMin = false;
	[Export] int _minValue = 0;
	[ExportSubgroup("Maximum")]
	[Export] bool _useMax = false;
	[Export] int _maxValue = 0;

	public override Response<CvarSetResponseStatus> SetValue(ReadOnlySpan<string> args)
	{
		if (!ArgumentParser.ValidateCount(args, 1, out string error))
			return new(CvarSetResponseStatus.InvalidArgs, error);

		if (!int.TryParse(args[0], out int i))
			return new(CvarSetResponseStatus.Failed, $"Can not convert {args[0]} to type int.");

		if (i == Value)
			return new(CvarSetResponseStatus.NoChange, null);

		if (_useMin && i < _minValue)
			return new(CvarSetResponseStatus.Failed, $"Cannot set {Signature} to anything less than {_minValue}.");

		if (_useMax && i > _maxValue)
			return new(CvarSetResponseStatus.Failed, $"Cannot set {Signature} to anything higher than {_maxValue}.");

		Value = i;
		return new(CvarSetResponseStatus.Success, null);
	}
}
