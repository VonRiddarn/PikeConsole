using System;
using FractalPike.PikeConsole.Config;
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

	protected override Response<CvarSetResponseStatus> SetValue(ReadOnlySpan<string> args)
	{
		if (!ArgumentParser.ValidateCount(args, 1, out string error))
			return new(CvarSetResponseStatus.InvalidArgs, error);

		if (!int.TryParse(args[0], out int v))
			return new(CvarSetResponseStatus.Failed, $"Can not convert {args[0]} to type int.");

		if (v == Value)
			return new(CvarSetResponseStatus.NoChange, null);

		// Prepare variables for the return status.
		// These are just overridden in order and ends up being the last check.
		// Which is why we go soft -> hard
		bool cm = PikeConsoleConfig.CheatMode.Value;
		string[] logTags = null;

		// Manage soft limits. Users cannot exceed without cheatmode (Bypassable)
		if (!cm)
		{
			if (UseLimitMin && v < MinLimitValue)
			{
				v = MinLimitValue;
				logTags = [RuntimeExecutionLogTags.ValueLimited];
			}
			else if (UseLimitMax && v > MaxLimitValue)
			{
				v = MaxLimitValue;
				logTags = [RuntimeExecutionLogTags.ValueLimited];
			}
		}

		// Manage hard limits. This can NEVER be exceedded.
		if (_useClampMin && v < _minClampValue)
		{
			v = _minClampValue;
			logTags = [RuntimeExecutionLogTags.ValueClamped];
		}
		else if (_useClampMax && v > _maxClampValue)
		{
			v = _maxClampValue;
			logTags = [RuntimeExecutionLogTags.ValueClamped];
		}

		Value = v;
		return new(CvarSetResponseStatus.Success, null, logTags);
	}
}
