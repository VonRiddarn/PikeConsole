using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars.Internal;
using Godot;
using System;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;

public partial class CVarString : CVarBase<string>
{
	public override string DisplayType => "CVar_String";

	[Export] protected override string _defaultValue { get; set; }
	[Export] protected override string _value { get; set; }

	public override Response<CvarSetResponseStatus> SetValue(string[] args)
	{
		if (args.Length == 1)
		{
			Value = args[0];
			return new(CvarSetResponseStatus.Success, null);
		}

		// Allocationg, but kinda necessary.
		Value = string.Join(' ', args);
		return new(CvarSetResponseStatus.Success, null);
	}
}
