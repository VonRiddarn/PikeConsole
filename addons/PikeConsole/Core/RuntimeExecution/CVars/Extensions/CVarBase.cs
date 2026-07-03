using System;
using System.Collections.Generic;
using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.Logging;
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

	// Used after the command to register it to the persistent registry without triggering a save.
	// Useful when running startup scripts etc.
	public const string RAM_ONLY_TAG = "ram_only";

	// Set automatic
	public string Signature { get; private set; } = string.Empty;
	public string ShortDesc => $"View or set the value of {Signature}";
	public virtual string Usage => $"{Signature} [new value]";

	// Set in child
	public abstract string DisplayType { get; }
	public string LongDesc => $"{Description}\n{DescriptionInternal}";
	protected virtual string DescriptionInternal { get; } = "";

	/// <summary>
	/// Called at the end of initialize. This can be used by children to protect data or create special caches.
	/// </summary>
	protected virtual void InitializeInternal() { }

	// Used for checking state during save
	public bool IsModified => !EqualityComparer<T>.Default.Equals(_value, _defaultValue);
	public string FormattedValue => DisplayValue(_value) ?? NOT_ASSIGNED;

	// Current value getter / setter
	public T Value
	{
		get => _value;
		set
		{
			if (!EqualityComparer<T>.Default.Equals(_value, value))
			{
				_value = value;
				ValueChanged?.Invoke(value);
				ValueInvalidated?.Invoke();

				if (Persist)
					PersistentCVarRegistry.Update(this);
			}
		}
	}

	bool _isInitialized = false;

	/// <summary>
	/// This method is only used internally to manage arguments.  
	/// It is always called from the "Execute" method.
	/// </summary>
	/// <param name="args">Arguments passed by the runtime console</param>
	/// <returns>A response status with an optional message.</returns>
	protected abstract Response<CvarSetResponseStatus> SetValue(ReadOnlySpan<string> args);

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

	public void ResetValue()
	{
		var modified = IsModified;

		Value = _defaultValue;

		if (modified && Persist)
			PersistentCVarRegistry.Update(this);
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

			return new(ExecutionResponseStatus.Success, $"Description: {Description}\nCurrent value: {currentValue}\nDefault value: {defaultValue}\nIs cheat: {IsCheat}");
		}

		// If this is a cheat AND we are not the system AND cheatmode is off. Fail the execution.
		// The system passes this check though, so we can still pass map specific overrides and cool stuff.
		if (IsCheat && executionSource is not ExecutionSource.System && !PikeConsoleConfig.CheatMode.Value)
			return new(ExecutionResponseStatus.DeniedCheat, null);

		bool ramOnly = args.Length >= 2 && args[^1].Equals(RAM_ONLY_TAG, StringComparison.OrdinalIgnoreCase);

		Response<CvarSetResponseStatus> response;

		try
		{
			// If we have the RAM_ONLY flag, slice that argument from the parameters.
			// SetValue will never have to deal with it.
			// Updated Using a ReadOnlySpan to make this non-alloc!
			response = SetValue(ramOnly ? args.AsSpan(0, args.Length - 1) : args.AsSpan());
		}
		catch (Exception e)
		{
			return new(ExecutionResponseStatus.Error, $"An unexpected error occurred when setting value of \"{Signature}\": {e.Message}");
		}

		// Return success, but log nothing.
		if (response.Status == CvarSetResponseStatus.NoChange)
			return new(ExecutionResponseStatus.Success, null);

		if (response.Status == CvarSetResponseStatus.Success)
		{
			if (Persist && !ramOnly)
				PersistentCVarRegistry.Update(this);

			return new(ExecutionResponseStatus.Success, MessageOrFallback(response.Message, $"Set \"{Signature}\" to {DisplayValue(Value)}"));
		}

		// Note, we are using "unexpected error" again here because a command creator could've caught the error and sent back null.
		return response.Status switch
		{
			CvarSetResponseStatus.InvalidArgs => new(ExecutionResponseStatus.InvalidArgs, MessageOrFallback(response.Message, $"Invalid arguments passed for \"{Signature}\".")),
			CvarSetResponseStatus.Failed => new(ExecutionResponseStatus.Failed, MessageOrFallback(response.Message, $"Failed to set the value for \"{Signature}\"")),
			_ => new(ExecutionResponseStatus.Error, MessageOrFallback(response.Message, $"An unexpected error occurred when setting the value for \"{Signature}\"")),
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


	string MessageOrFallback(string message, string fallback) => string.IsNullOrWhiteSpace(message) ? fallback : message;

}
