using System;
using System.Collections.Generic;
using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.Utilities;
using Godot;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Cvars.Extensions;

#nullable enable

public abstract partial class CVarBase<T> : Resource, ICVar
{
	const string NOT_ASSIGNED = "n/a";

	/// <summary>
	/// Used for subscribing to the CVar value. If the value changes, this event fires and sends the new value.
	/// </summary>
	public event Action<T>? ValueChanged;
	/// <summary>
	/// Used for subscribing on the Cvar state. If the value changes, this event fires but sends nothing.
	/// </summary>
	/// <remarks>
	/// Used when several CVars share the same execution method. Such as a crosshair that reconstructs in its entirety no matter which CVar changed.
	/// </remarks>
	public event Action? ValueInvalidated;

	// Stupid Godot ruining my DRY code...
	/// <summary>A property that exists solely for serializing the value to the editor.</summary>
	/// <remarks>A hack we must use to make Godot compile, sadly.</remarks>
	protected abstract T _defaultValue { get; set; }

	/// <summary>A property that exists solely for serializing the value to the editor.</summary>
	/// <remarks>A hack we must use to make Godot compile, sadly.</remarks>
	protected abstract T _value { get; set; }

	[ExportGroup("CVar")]
	[Export] public bool Persist { get; private set; } = false;
	[Export] public bool IsCheat { get; private set; } = false;
	/// <summary>Used to apply description within the editor. Appended to the LongDesc property.</summary>
	/// <remarks>Use property <c>LongDesc</c> for the most accurate long description.</remarks>
	[Export(PropertyHint.MultilineText)] public string Description { get; private set; } = "";

	// Set automatic
	public string Signature { get; private set; } = string.Empty;
	public string ShortDesc => $"View or set the value of {Signature}";
	public virtual string Usage => $"\n\t{Signature} [new value]";

	// Set in child
	public abstract string DisplayType { get; }
	public string LongDesc => $"{Description}{(string.IsNullOrWhiteSpace(DescriptionInternal) ? string.Empty : $"\n{DescriptionInternal}")}";
	protected virtual string DescriptionInternal { get; } = "";

	string _resourceLocation = "Unknown location. Did you forget to initialize?";
	public string SourceLocation => _resourceLocation;

	/// <summary>
	/// Called at the end of initialize. This can be used by children to protect data or create special caches.
	/// </summary>
	protected virtual void InitializeInternal() { }

	// Used for checking state during save
	public bool IsModified => !EqualityComparer<T>.Default.Equals(_value, _defaultValue);
	public virtual string FormattedValue => _value?.ToString() ?? NOT_ASSIGNED;
	public string CurrentValueDisplay => DisplayValue(_value);

	// Current value getter / setter
	public T Value
	{
		get => _value;
		set
		{
			bool changed = !EqualityComparer<T>.Default.Equals(_value, value);

			SetRAM(value);

			if (changed && Persist)
				PersistentCVarRegistry.Update(this);
		}
	}

	/// <summary>
	/// Sets the value and triggers value changed events WITHOUT triggering an update for the persistent registry.
	/// </summary>
	/// <param name="value"></param>
	public void SetRAM(T value)
	{
		if (!EqualityComparer<T>.Default.Equals(_value, value))
		{
			_value = value;
			ValueChanged?.Invoke(value);
			ValueInvalidated?.Invoke();
		}
	}

	bool _isInitialized = false;

	/// <summary>
	/// This method is only used internally to manage arguments.  
	/// It is always called from the "Execute" method.
	/// </summary>
	/// <param name="args">Arguments passed by the runtime console</param>
	/// <returns>A response status with an optional message.</returns>
	protected abstract Response<CvarSetResponseStatus, T> ParseValue(ReadOnlySpan<string> args);

	/// <summary>
	/// Initialize is called by the CVar crawler when the resource is loaded into memory.
	/// It is self diagnostic and will log errors or warnings regarding registering to the command registry.
	/// Automatically no-ops if the resoure is already initialized.
	/// </summary>
	/// <remarks>
	/// If your CVar is in the designated CVar folder you DO NOT call this method, it is called automatically! <br />
	/// Only call this method if you have made a custom sub-system for CVars or are using them internally somewhere.
	/// </remarks>
	public void Initialize()
	{
		if (_isInitialized)
			return;


		_value = _defaultValue;
		// Note: This causes interop overhead at startup. That's okay and unavoidable.
		// The resource filename IS the command name. Convenience vs customization and all that.
		Signature = ConsoleFormatter.ToSignature(ResourcePath.GetFile().GetBaseName());

		_resourceLocation = ResourcePath;

		if (Persist)
			PersistentCVarRegistry.Write(Signature, this);

		var response = RuntimeExecutableRegistry.Register(this);

		switch (response.Status)
		{
			case RegisterExecutableResponseStatus.Success:
#if TOOLS
				// Stripped in compiled build so we don't even have to make the conditional check.
				if (PikeConsoleConfig.LogCvarOnRegister)
					PikeLogger.Log(LogTarget.Editor, $"{Signature} added to CVar registry!");
#endif
				break;
			case RegisterExecutableResponseStatus.AlreadyExists:
				PikeLogger.LogError(LogTarget.All, $"CVar {Signature} couldn't register as a command or CVar of this name already exists!", forceLog: true);
				break;
			case RegisterExecutableResponseStatus.ReplacedAlias:
				PikeLogger.LogWarning(LogTarget.All, $"CVar {Signature} was registered, but replaced an alias with th same signature.", forceLog: true);
				break;
			default:
				PikeLogger.LogError(LogTarget.All, $"CVar {Signature} didn't get a valid response from the command registry.", forceLog: true);
				break;
		}

		InitializeInternal();

		_isInitialized = true;
	}

	public bool ResetValue(ExecutionSource executionSource, bool ramOnly = false)
	{
		if (IsCheat && executionSource is not ExecutionSource.System && !PikeConsoleConfig.CheatMode.Value)
			return false;

		if (Persist && !ramOnly)
			Value = _defaultValue;
		else
			SetRAM(_defaultValue);

		return true;
	}

	// ----- ----- Note to future self: 
	// The StatementExecutor will be able to differ Commands from CVars. 
	// CVar response text will not be green unless we want to.
	// Success is a state, not an aesthetic! Calm down.
	// 
	// Also, remember that if no message is returned, there will be no log!
	// ----- -----
	public Response<ExecutionResponseStatus> Execute(ExecutionSource executionSource, string[] args)
	{
		// No arguments mean we want to check the value.
		// Early return with success message for the current value.
		if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
		{
			string currentValue = _value != null ? DisplayValue(_value) : NOT_ASSIGNED;
			string defaultValue = _defaultValue != null ? DisplayValue(_defaultValue) : NOT_ASSIGNED;

			return new(
				ExecutionResponseStatus.Success,
				$"Type: {DisplayType}\nCurrent value: {currentValue}\nDefault value: {defaultValue}\nIs cheat: {IsCheat}\nDescription: {Description}",
				[LogTags.NoHeader]);
		}

		// If this is a cheat AND we are not the system AND cheatmode is off. Fail the execution.
		// The system passes this check though, so we can still pass map specific overrides and cool stuff.
		if (IsCheat && executionSource is not ExecutionSource.System && !PikeConsoleConfig.CheatMode.Value)
			return new(ExecutionResponseStatus.DeniedCheat, $"Failed to set value of \"{Signature}\". CVar is cheat protected.");

		bool ramOnly = args.Length >= 2 && args[^1].Equals(FileSystemHelper.RAM_ONLY_FLAG, StringComparison.OrdinalIgnoreCase);

		Response<CvarSetResponseStatus, T> response;

		try
		{
			// If we have the RAM_ONLY flag, slice that argument from the parameters.
			// ParseValue will never have to deal with it as an argument.
			response = ParseValue(ramOnly ? args.AsSpan(0, args.Length - 1) : args.AsSpan());
		}
		catch (Exception e)
		{
			return new(ExecutionResponseStatus.Error, $"Uncaught exception when setting value of \"{Signature}\"\nin {SourceLocation}:\n{e.Message}");
		}

		if (response.Status == CvarSetResponseStatus.NoChange)
			return new(ExecutionResponseStatus.Success, $"CVar \"{Signature}\" is already set to {DisplayValue(Value)}", [LogTags.ValueNoChange, .. response.Tags]);

		if (response.Status == CvarSetResponseStatus.Success)
		{
			if (Persist && !ramOnly)
				Value = response.Payload!;
			else
				SetRAM(response.Payload!);

			return new(ExecutionResponseStatus.Success, MessageOrFallback(response.Message, $"Set \"{Signature}\" to {DisplayValue(Value)}"), response.Tags);
		}

		// Note, we are using "unexpected error" again here because a command creator could've caught the error and sent back null.
		return response.Status switch
		{
			CvarSetResponseStatus.InvalidArgs => new(ExecutionResponseStatus.InvalidArgs, MessageOrFallback(response.Message, $"Invalid arguments passed for \"{Signature}\"."), response.Tags),
			CvarSetResponseStatus.Failed => new(ExecutionResponseStatus.Failed, MessageOrFallback(response.Message, $"Failed to set the value for \"{Signature}\""), response.Tags),
			_ => new(ExecutionResponseStatus.Error, MessageOrFallback(response.Message, $"An unexpected error occurred when setting the value for \"{Signature}\""), response.Tags),
		};
	}

	public string GetHelp() => ConsoleFormatter.FormatHelp(this);

	// Generic helpers
	/// <summary>
	/// Override for how this value should be displayed in text.
	/// </summary>
	/// <remarks>
	/// Overriding the "ToString" property for the value is not adviced as that could ruin parsing.
	/// <remarks>
	/// <returns>Value as string</returns>
	public virtual string DisplayValue(T value) => value?.ToString() ?? NOT_ASSIGNED;


	static string MessageOrFallback(string message, string fallback) => string.IsNullOrWhiteSpace(message) ? fallback : message;

}
