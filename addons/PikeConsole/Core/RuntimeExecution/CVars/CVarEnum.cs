using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars.Extensions;
using FractalPike.PikeConsole.Core.Utilities;
using Godot;
using System;
using System.Text;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;

[GlobalClass]
public partial class CVarEnum : CVarBase<int>
{
	public override string DisplayType => "CVar_Enum";

	[Export]
	protected override int _defaultValue { get; set; }
	[Export]
	protected override int _value { get; set; }

	protected override string DescriptionInternal => _cachedHelpLst;

	string[] _options = [];
	string _cachedHelpLst = "";

	[Export]
	public string[] Options { get; set; }

	protected override void InitializeInternal()
	{
		// Since we have to expose the property to the editor we also expose it to other systems.
		// On startup, cache the enum definitions to protect them from runtime modification.
		_options = Options ?? [];

		// Early return if the options array is empty (Bad)
		if (_options.Length == 0)
		{
			PikeLogger.LogWarning(LogTarget.All, $"CVarEnum '{Signature}' has no options defined.");
			_defaultValue = 0;
			_value = 0;
			_cachedHelpLst = "\tOPTIONS:\n\t\tNone defined.";
			return;
		}

		if (!IsInRange(_defaultValue))
		{
			PikeLogger.LogWarning(LogTarget.All, $"DefaultValueEditor is out of range for the options array ({_options.Length}[{_defaultValue}]).");
			_defaultValue = Mathf.Clamp(_defaultValue, 0, _options.Length - 1);
		}

		if (!IsInRange(_value))
		{
			PikeLogger.LogWarning(LogTarget.All, $"ValueEditor is out of range for the options array ({_options.Length}[{_value}]). Clamping.");
			_value = Mathf.Clamp(_value, 0, _options.Length - 1);
		}

		// Upgrade from Unity framework!!
		// Cache the options rather than building them at runtime.
		StringBuilder sb = new("Options:");
		for (int i = 0; i < _options.Length; i++)
			sb.Append($"\n\t{i} = {_options[i]}");

		_cachedHelpLst = sb.ToString();
	}

	protected override Response<CvarSetResponseStatus, int> ParseValue(ReadOnlySpan<string> args)
	{
		if (!ArgumentParser.ValidateCount(args, 1, out string error))
			return new(CvarSetResponseStatus.InvalidArgs, default, error);

		if (!ArgumentParser.TryParseEnum(args[0], _options, out int index, out error))
			return new(CvarSetResponseStatus.Failed, default, error);

		if (Value == index)
			return new(CvarSetResponseStatus.NoChange, index, null);

		return new(CvarSetResponseStatus.Success, index, null);
	}

	// ----- ----- ----- -----
	//	HELPERS AND OVERRIDES
	// ----- ----- ----- -----

	bool IsInRange(int index) => index >= 0 && index < _options.Length;

	public override string DisplayValue(int value) => $"{value} ({_options[value]})";

	// ----- ----- ----- -----
	//			API
	// ----- ----- ----- -----
	/// <summary>
	/// Checks if the value's raw string is equal to the inputted string. Case-insensitive.
	/// </summary>
	public bool Is(string valueName) =>
		ValueName.Equals(valueName, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// The value name in its raw string format.
	/// </summary>
	public string ValueName =>
		_options is not null and not [] && _value >= 0 && _value < _options.Length ? _options[_value] : string.Empty;

}
