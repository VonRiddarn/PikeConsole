using System;
using System.Collections.Generic;
using FractalPike.PikeConsole.Core.Logging;
using Godot;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Cvars.Internal;

#nullable enable

public abstract partial class CVarBase<T> : Resource, ICVar
{
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
	protected abstract T DefaultValueEditor { get; set; }

	/// <summary>A property that exists solely for serializing the value to the editor.</summary>
	/// <remarks>A hack we must use to make Godot compile, sadly.</remarks>
	protected abstract T ValueEditor { get; set; }

	[ExportGroup("CVar")]
	[Export] public bool Persist { get; private set; } = false;
	[Export] public bool IsCheat { get; private set; } = false;

	// Used after the command to register it to the persistent registry without triggering a save.
	// Useful when running startup scripts etc.
	public const string RAM_ONLY_TAG = "ram_only";

	// Set automatic
	public string Signature { get; private set; } = string.Empty;
	public virtual string ShortDesc => $"View or set the value of {Signature}";
	public virtual string Usage => $"{Signature} [new value]";

	// Set in child
	public abstract string DisplayType { get; }
	public virtual string LongDesc { get; } = "";

	/// <summary>
	/// Called at the end of initialize. This can be used by children to protect data or create special caches.
	/// </summary>
	protected virtual void InitializeInternal() { }

	// Used for checking state during save
	public bool IsModified => !EqualityComparer<T>.Default.Equals(ValueEditor, DefaultValueEditor);
	public virtual string FormattedValue => ValueEditor?.ToString() ?? "null";

	// Current value getter / setter
	public T Value
	{
		get => ValueEditor;
		set
		{
			if (!EqualityComparer<T>.Default.Equals(ValueEditor, value))
			{
				ValueEditor = value;
				ValueChanged?.Invoke(value);
				ValueInvalidated?.Invoke();
			}
		}
	}

	/// <summary>
	/// This is the method used by the console to set the command.
	/// It is responsible for parsing the arguments and setting the value.
	/// </summary>
	/// <param name="args">Arguments passed by the runtime console</param>
	/// <returns>A response status with an optional message.</returns>
	public abstract Response<CvarSetResponseStatus> SetValue(string[] args);

	/// <summary>
	/// Initialize is called by the CVar crawler when the resource is loaded into memory.
	/// It is self diagnostic and will log errors or warnings regarding registering to the command registry.
	/// </summary>
	/// <remarks>
	/// If your CVar is in the designated CVar folder you DO NOT call this method, it is called automatically! <br />
	/// Only call this method if you have made a custom sub-system for CVars or are using them internally somewhere.
	/// </remarks>
	public void Initialize()
	{
		ValueEditor = DefaultValueEditor;
		// Note: This causes interop overhead at startup. That's okay and unavoidable.
		// The resource filename IS the command name. Convenience vs customization and all that.
		Signature = ConsoleFormatter.ToSignature(ResourcePath.GetFile().GetBaseName());

		if (IsCheat)
			PikeConsoleConfig.CheatModeChanged += OnCheatModeChanged;

		if (Persist)
			PersistentCVarRegistry.Write(Signature, this);

		var response = RuntimeExecutableRegistry.Register(this);

		switch (response.Status)
		{
			case RegisterExecutableResponseStatus.Success:
				break;
			case RegisterExecutableResponseStatus.AlreadyExists:
				PikeLogger.LogError(LogTarget.All, $"CVar {Signature} couldn't register as a command or CVar of this name already exists!");
				break;
			case RegisterExecutableResponseStatus.ReplacedAlias:
				PikeLogger.LogWarning(LogTarget.All, $"CVar {Signature} was registered, but replaced an alias with th same signature.");
				break;
			default:
				PikeLogger.LogError(LogTarget.All, $"CVar {Signature} didn't get a valid response from the command registry.");
				break;
		}

		InitializeInternal();
	}

	// 99% of the time, resources live from start to end and this wont be needed.
	// Its added for good hygiene and memory leak prevention during exceptional circumstances
	protected override void Dispose(bool disposing)
	{
		if (disposing && IsCheat)
			PikeConsoleConfig.CheatModeChanged -= OnCheatModeChanged;

		base.Dispose(disposing);
	}

	private void OnCheatModeChanged(bool cheatMode)
	{
		if (!cheatMode)
		{
			Value = DefaultValueEditor;
		}
	}

	public void ResetValue()
	{
		var modified = IsModified;

		Value = DefaultValueEditor;

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
	public Response<ExecutionResponseStatus> Execute(string[] args)
	{
		// No arguments mean we want to check the value.
		// Early return with success message for the current value.
		if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
		{
			string currentValue = ValueEditor != null ? DisplayValue(ValueEditor) : "null";
			string defaultValue = DefaultValueEditor != null ? DisplayValue(DefaultValueEditor) : "null";

			return new(ExecutionResponseStatus.Success, $"Current value: {currentValue}\nDefault value: {defaultValue}\nIs cheat: {IsCheat}");
		}

		// If we are in cheat mode, 
		if (IsCheat && !PikeConsoleConfig.CheatMode)
			return new(ExecutionResponseStatus.DeniedCheat, null);

		bool ramOnly = args.Length >= 2 && args[^1].Equals(RAM_ONLY_TAG, StringComparison.OrdinalIgnoreCase);

		Response<CvarSetResponseStatus> response;

		try
		{
			// If we have the RAM_ONLY flag, slice that argument from the parameters.
			// SetValue will never have to deal with it.
			response = SetValue(ramOnly ? args[..^1] : args);
		}
		catch (Exception e)
		{
			return new(ExecutionResponseStatus.Error, $"An unexpected error occured when setting value of {Signature}: {e.Message}");
		}

		// Return success, but log nothing.
		if (response.Status == CvarSetResponseStatus.NoChange)
			return new(ExecutionResponseStatus.Success, null);

		if (response.Status == CvarSetResponseStatus.Success)
		{
			if (Persist && !ramOnly)
				PersistentCVarRegistry.Update(this);

			string msg = response.Message ?? $"{Signature} set to {DisplayValue(Value)}";
			return new(ExecutionResponseStatus.Success, msg);
		}

		return response.Status switch
		{
			CvarSetResponseStatus.InvalidArgs => new(ExecutionResponseStatus.InvalidArgs, response.Message ?? $"Invalid arguments passed for {Signature}."),
			CvarSetResponseStatus.Failed => new(ExecutionResponseStatus.Failed, response.Message ?? $"Failed to set the value for {Signature}"),
			_ => new(ExecutionResponseStatus.Error, response.Message ?? $"An unexpected error occured when setting the value for {Signature}"),
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
	public virtual string DisplayValue(T value) => value?.ToString() ?? "null";

}
