using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars.Extensions;
using Godot;
using System;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;

public partial class CVarString : CVarBase<string>
{
	public override string DisplayType => "CVar_String";

	[Export] protected override string _defaultValue { get; set; }
	[Export] protected override string _value { get; set; }
	[Export] public int MaxCharacters { get; private set; } = 0;

	protected override Response<CvarSetResponseStatus> SetValue(ReadOnlySpan<string> args)
	{
		if (!ArgumentParser.ValidateCount(args, 1, out string error))
			return new(CvarSetResponseStatus.InvalidArgs, $"{error} : If your text contains spaces, wrap it in \"quotes\". To use quotes within quotes, use backslashes: \\\"");

		if (MaxCharacters > 0 && args[0].Length > MaxCharacters && !PikeConsoleConfig.CheatMode.Value)
			return new(CvarSetResponseStatus.InvalidArgs, $"Max character count for \"{Signature}\" is {MaxCharacters}");

		Value = args[0];
		return new(CvarSetResponseStatus.Success, null);
	}
}
