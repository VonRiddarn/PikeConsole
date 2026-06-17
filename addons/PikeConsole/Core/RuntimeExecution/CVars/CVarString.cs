using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars.Extensions;
using Godot;
using System;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;

public partial class CVarString : CVarBase<string>
{
	public override string DisplayType => "CVar_String";

	[Export] protected override string _defaultValue { get; set; }
	[Export] protected override string _value { get; set; }

	public override Response<CvarSetResponseStatus> SetValue(ReadOnlySpan<string> args)
	{
		if (!ArgumentParser.ValidateCount(args, 1, out string error))
			return new(CvarSetResponseStatus.InvalidArgs, $"{error} : If your text contains spaces, wrap it in \"quotes\". To use quotes within quotes, use backslashes: \\\"");

		Value = args[0];
		return new(CvarSetResponseStatus.Success, null);
	}
}
