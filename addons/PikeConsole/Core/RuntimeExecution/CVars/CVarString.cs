using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars.Extensions;
using FractalPike.PikeConsole.Core.Utilities;
using Godot;
using System;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;

public partial class CVarString : CVarBase<string>
{
	public override string DisplayType => "CVar_String";

	[Export] protected override string _defaultValue { get; set; }
	[Export] protected override string _value { get; set; }
	[Export] public int MaxCharacters { get; private set; } = 0;

	protected override Response<CvarSetResponseStatus, string> ParseValue(ReadOnlySpan<string> args)
	{
		if (!ArgumentParser.ValidateCount(args, 1, out string error))
			return new(CvarSetResponseStatus.InvalidArgs, default, $"{error} : If your text contains spaces, wrap it in \"quotes\". To use quotes within quotes, use backslashes: \\\"");

		if (MaxCharacters > 0 && args[0].Length > MaxCharacters && !PikeConsoleCVars.CheatMode.Value)
			return new(CvarSetResponseStatus.InvalidArgs, default, $"Max character count for \"{Signature}\" is {MaxCharacters}");

		return new(CvarSetResponseStatus.Success, args[0], null);
	}
}
