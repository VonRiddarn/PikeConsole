using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars.Extensions;
using FractalPike.PikeConsole.Core.Utilities;
using Godot;
using System;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;

[GlobalClass]
public partial class CVarVector2I : CVarBase<Vector2I>
{
	public override string DisplayType => "CVar_Vector2I";

	[Export]
	protected override Vector2I _defaultValue { get; set; }
	[Export]
	protected override Vector2I _value { get; set; }

	// The limits are user-facing and can be used in GUIs to hydrate sliders or clamp before making an execution call.
	[ExportGroup("Limits | Cheatmode")]
	[ExportSubgroup("Minimum")]
	[Export] public bool LimitMinX { get; private set; } = false;
	[Export] public bool LimitMinY { get; private set; } = false;
	[Export] public Vector2I MinLimitValue { get; private set; } = Vector2I.Zero;

	[ExportSubgroup("Maximum")]
	[Export] public bool LimitMaxX { get; private set; } = false;
	[Export] public bool LimitMaxY { get; private set; } = false;
	[Export] public Vector2I MaxLimitValue { get; private set; } = Vector2I.Zero;

	// These are of no use to the frontend. Thus they are hidden.
	[ExportGroup("Clamps | Engine")]
	[ExportSubgroup("Minimum")]
	[Export] bool _clampMinX = false;
	[Export] bool _clampMinY = false;
	[Export] Vector2I _minClampValue = Vector2I.Zero;

	[ExportSubgroup("Maximum")]
	[Export] bool _clampMaxX = false;
	[Export] bool _clampMaxY = false;
	[Export] Vector2I _maxClampValue = Vector2I.Zero;

	protected override Response<CvarSetResponseStatus, Vector2I> ParseValue(ReadOnlySpan<string> args)
	{
		if (!ArgumentParser.TryParseVector2IContextual(args, Value, out Vector2I value, out string error))
			return new(CvarSetResponseStatus.Failed, default, error);

		if (Value == value)
			return new(CvarSetResponseStatus.NoChange, value, null);

		bool cm = PikeConsoleCVars.CheatMode.Value;
		string[] logTags = null;

		Vector2I requestedPreLimit = value;

		if (!cm)
		{
			if (LimitMinX) value.X = Mathf.Max(value.X, MinLimitValue.X);
			if (LimitMinY) value.Y = Mathf.Max(value.Y, MinLimitValue.Y);

			if (LimitMaxX) value.X = Mathf.Min(value.X, MaxLimitValue.X);
			if (LimitMaxY) value.Y = Mathf.Min(value.Y, MaxLimitValue.Y);

			if (value != requestedPreLimit)
			{
				logTags = [LogTags.ValueLimited];
				requestedPreLimit = value;
			}
		}

		if (_clampMinX) value.X = Mathf.Max(value.X, _minClampValue.X);
		if (_clampMinY) value.Y = Mathf.Max(value.Y, _minClampValue.Y);

		if (_clampMaxX) value.X = Mathf.Min(value.X, _maxClampValue.X);
		if (_clampMaxY) value.Y = Mathf.Min(value.Y, _maxClampValue.Y);

		if (value != requestedPreLimit)
		{
			logTags = [LogTags.ValueClamped];
		}

		return new(CvarSetResponseStatus.Success, value, null, logTags);
	}

	// ----- ----- ----- -----
	//	HELPERS AND OVERRIDES
	// ----- ----- ----- -----

	// CRITICAL INFORMATION: Formatted value on CVars must be 2 way parseable!!
	// We MUST override the method here, as the default Vector2I.ToString() doesn't give us the value as we expect it in the parser.
	public override string FormattedValue => $"{_value.X} {_value.Y}";

	// This is just used to display the value in a cool / readable way.
	public override string DisplayValue(Vector2I value) => $"INT[x: {value.X} | y: {value.Y}]";

	public override string[] Usages => [$"{Signature} [x] [y]"];

	protected override string DescriptionInternal => "\tVariables:\n\t\tx, y";
}
