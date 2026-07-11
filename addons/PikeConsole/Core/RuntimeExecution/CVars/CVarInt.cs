using System;
using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars.Extensions;
using FractalPike.PikeConsole.Core.Utilities;
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

	// The limits are user-facing and can be used in GUIs to hydrate sliders or clamp before making an execution call.
	[ExportGroup("Limits | Cheatmode")]
	[ExportSubgroup("Minimum")]
	[Export] public bool UseLimitMin { get; private set; } = false;
	[Export] public int MinLimitValue { get; private set; } = 0;
	[ExportSubgroup("Maximum")]
	[Export] public bool UseLimitMax { get; private set; } = false;
	[Export] public int MaxLimitValue { get; private set; } = 0;

	// These are of no use to the frontend. Thus they are hidden.
	[ExportGroup("Clamps | Engine")]
	[ExportSubgroup("Minimum")]
	[Export] bool _useClampMin = false;
	[Export] int _minClampValue = 0;
	[ExportSubgroup("Maximum")]
	[Export] bool _useClampMax = false;
	[Export] int _maxClampValue = 0;

	protected override Response<CvarSetResponseStatus, int> ParseValue(ReadOnlySpan<string> args)
	{
		if (!ArgumentParser.ValidateCount(args, 1, out string error))
			return new(CvarSetResponseStatus.InvalidArgs, default, error);

		if (!int.TryParse(args[0], out int value))
			return new(CvarSetResponseStatus.Failed, default, $"Can not convert {args[0]} to type int.");

		if (Value == value)
			return new(CvarSetResponseStatus.NoChange, value, null);

		// Prepare variables for the return status.
		// These are just overridden in order and ends up being the last check.
		// Which is why we go soft -> hard
		bool cm = PikeConsoleConfig.CheatMode.Value;
		string[] logTags = null;

		// Manage soft limits. Users cannot exceed without cheatmode (Bypassable)
		if (!cm)
		{
			if (UseLimitMin && value < MinLimitValue)
			{
				value = MinLimitValue;
				logTags = [LogFlags.ValueLimited];
			}
			else if (UseLimitMax && value > MaxLimitValue)
			{
				value = MaxLimitValue;
				logTags = [LogFlags.ValueLimited];
			}
		}

		// Manage hard limits. This can NEVER be exceedded.
		if (_useClampMin && value < _minClampValue)
		{
			value = _minClampValue;
			logTags = [LogFlags.ValueClamped];
		}
		else if (_useClampMax && value > _maxClampValue)
		{
			value = _maxClampValue;
			logTags = [LogFlags.ValueClamped];
		}

		return new(CvarSetResponseStatus.Success, value, null, logTags);
	}
}
