using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars.Internal;
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

	public override Response<CvarSetResponseStatus> SetValue(string[] args)
	{
		if (!ArgumentParser.ValidateCount(args, 1, out string error))
			return new(CvarSetResponseStatus.InvalidArgs, error);

		if (!ArgumentParser.TryParseBool(args[0], out bool value))
			return new(CvarSetResponseStatus.Failed, $"Could not parse {args[0]} into type bool.");

		if (Value == value)
			return new(CvarSetResponseStatus.NoChange, null);

		Value = value;
		return new(CvarSetResponseStatus.Success, $"Set {Signature} to {Value}.");
	}
}
