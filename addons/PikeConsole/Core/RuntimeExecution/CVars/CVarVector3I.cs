using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars.Extensions;
using FractalPike.PikeConsole.Core.Utilities;
using Godot;
using System;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;

[GlobalClass]
public partial class CVarVector3I : CVarBase<Vector3I>
{
	public override string DisplayType => "CVar_Vector3I";

	[Export]
	protected override Vector3I _defaultValue { get; set; }
	[Export]
	protected override Vector3I _value { get; set; }

	// The limits are user-facing and can be used in GUIs to hydrate sliders or clamp before making an execution call.
	[ExportGroup("Limits | Cheatmode")]
	[ExportSubgroup("Minimum")]
	[Export] public bool LimitMinX { get; private set; } = false;
	[Export] public bool LimitMinY { get; private set; } = false;
	[Export] public bool LimitMinZ { get; private set; } = false;
	[Export] public Vector3I MinLimitValue { get; private set; } = Vector3I.Zero;

	[ExportSubgroup("Maximum")]
	[Export] public bool LimitMaxX { get; private set; } = false;
	[Export] public bool LimitMaxY { get; private set; } = false;
	[Export] public bool LimitMaxZ { get; private set; } = false;
	[Export] public Vector3I MaxLimitValue { get; private set; } = Vector3I.Zero;

	// These are of no use to the frontend. Thus they are hidden.
	[ExportGroup("Clamps | Engine")]
	[ExportSubgroup("Minimum")]
	[Export] bool _clampMinX = false;
	[Export] bool _clampMinY = false;
	[Export] bool _clampMinZ = false;
	[Export] Vector3I _minClampValue = Vector3I.Zero;

	[ExportSubgroup("Maximum")]
	[Export] bool _clampMaxX = false;
	[Export] bool _clampMaxY = false;
	[Export] bool _clampMaxZ = false;
	[Export] Vector3I _maxClampValue = Vector3I.Zero;

	protected override Response<CvarSetResponseStatus, Vector3I> ParseValue(ReadOnlySpan<string> args)
	{
		if (!ArgumentParser.TryParseVector3IContextual(args, Value, out Vector3I value, out string error))
			return new(CvarSetResponseStatus.Failed, default, error);

		if (Value == value)
			return new(CvarSetResponseStatus.NoChange, value, null);

		bool cm = PikeConsoleStates.CheatMode.Value;
		string[] logTags = null;

		Vector3I requestedPreLimit = value;

		if (!cm)
		{
			if (LimitMinX) value.X = Mathf.Max(value.X, MinLimitValue.X);
			if (LimitMinY) value.Y = Mathf.Max(value.Y, MinLimitValue.Y);
			if (LimitMinZ) value.Z = Mathf.Max(value.Z, MinLimitValue.Z);

			if (LimitMaxX) value.X = Mathf.Min(value.X, MaxLimitValue.X);
			if (LimitMaxY) value.Y = Mathf.Min(value.Y, MaxLimitValue.Y);
			if (LimitMaxZ) value.Z = Mathf.Min(value.Z, MaxLimitValue.Z);

			if (value != requestedPreLimit)
			{
				logTags = [LogTags.ValueLimited];
				requestedPreLimit = value;
			}
		}

		if (_clampMinX) value.X = Mathf.Max(value.X, _minClampValue.X);
		if (_clampMinY) value.Y = Mathf.Max(value.Y, _minClampValue.Y);
		if (_clampMinZ) value.Z = Mathf.Max(value.Z, _minClampValue.Z);

		if (_clampMaxX) value.X = Mathf.Min(value.X, _maxClampValue.X);
		if (_clampMaxY) value.Y = Mathf.Min(value.Y, _maxClampValue.Y);
		if (_clampMaxZ) value.Z = Mathf.Min(value.Z, _maxClampValue.Z);

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
	// We MUST override the method here, as the default Vector3I.ToString() doesn't give us the value as we expect it in the parser.
	public override string FormattedValue => $"{_value.X} {_value.Y} {_value.Z}";

	// This is just used to display the value in a cool / readable way.
	public override string DisplayValue(Vector3I value) => $"INT[x: {value.X} | y: {value.Y} | z: {value.Z}]";

	public override string[] Usages => [$"{Signature} [x] [y] [z]"];

	protected override string DescriptionInternal => "Variables:\n\tx, y, z";
}
