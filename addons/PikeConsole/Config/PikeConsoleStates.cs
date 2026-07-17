using System;
using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;

namespace FractalPike.PikeConsole.Config;

/*
 * Semi-Internal class that centralizes CVars.
 * End users shouldn't interact with this directly, 
 * they should be routed through PikeConsoleAPI wrapper.
*/

#nullable enable

public static class PikeConsoleStates
{
	// ----- ----- ----- ----- -----
	// 			INTERNAL
	// ----- ----- ----- ----- -----
	const string INTERNAL_CVARS_PATH = "res://addons/PikeConsole/Config/variables";

	// ----- ----- ----- ----- -----
	// 		   CENTRALIZED
	// ----- ----- ----- ----- -----

	/// <summary>Subscribe to the UI state of the console. Useful for hooking up pre-existing pause systems to the console state.</summary>
	public static event Action<bool>? ConsoleUIActiveChanged;

	static bool _consoleUIActive = false;
	/// <summary>Get the current state of the runtime console. False makes the console and logger are no-op!</summary>
	/// <remarks>This has nothing to do with if the UI is active. To check the state of the UI, use IsActive.</remarks>
	public static bool ConsoleUIActive
	{
		get => _consoleUIActive;
		set
		{
			if (_consoleUIActive == value)
				return;

			_consoleUIActive = value;
			ConsoleUIActiveChanged?.Invoke(value);
		}
	}

	public static bool ToggleConsoleUI()
	{
		_consoleUIActive = !_consoleUIActive;
		return _consoleUIActive;
	}

	// ----- ----- ----- ----- -----
	// 			 CVars
	// ----- ----- ----- ----- -----

	static CVarInt? _consoleMaxLines;
	/// <summary>
	/// Lazy initialized CVar. If it is not initialized at the point of contact, it initializes and caches the reference.
	/// </summary>
	public static CVarInt ConsoleMaxLines
	{
		get
		{
			if (_consoleMaxLines == null)
			{
				// Assign the maxlogs BEFORE we initialize!!
				// Thus when initialize call PikeLogger and PikeLogger asks for MaxLogs we return the cached CVar...
				_consoleMaxLines = CvarLoader.LoadInternalCVar<CVarInt>($"{INTERNAL_CVARS_PATH}/console", "console_max_lines");
				_consoleMaxLines.Initialize();
			}
			return _consoleMaxLines;
		}
	}
	static CVarInt? _consoleUpdateRate;
	/// <summary>
	/// Lazy initialized CVar. If it is not initialized at the point of contact, it initializes and caches the reference.
	/// </summary>
	public static CVarInt ConsoleUpdateRate
	{
		get
		{
			if (_consoleUpdateRate == null)
			{
				// Assign the maxlogs BEFORE we initialize!!
				// Thus when initialize call PikeLogger and PikeLogger asks for MaxLogs we return the cached CVar...
				_consoleUpdateRate = CvarLoader.LoadInternalCVar<CVarInt>($"{INTERNAL_CVARS_PATH}/console", "console_update_rate");
				_consoleUpdateRate.Initialize();
			}
			return _consoleUpdateRate;
		}
	}

	static CVarBool? _runtimeConsoleEnabled;
	/// <summary>
	/// Lazy initialized CVar. If it is not initialized at the point of contact, it initializes and caches the reference.
	/// </summary>
	public static CVarBool RuntimeConsoleEnabled
	{
		get
		{
			if (_runtimeConsoleEnabled == null)
			{
				_runtimeConsoleEnabled = CvarLoader.LoadInternalCVar<CVarBool>($"{INTERNAL_CVARS_PATH}/console", "console_enabled");
				_runtimeConsoleEnabled.Initialize();
			}
			return _runtimeConsoleEnabled;
		}
	}

	static CVarBool? _cheatMode;
	/// <summary>
	/// Lazy initialized CVar. If it is not initialized at the point of contact, it initializes and caches the reference.
	/// </summary>
	public static CVarBool CheatMode
	{
		get
		{
			if (_cheatMode == null)
			{
				_cheatMode = CvarLoader.LoadInternalCVar<CVarBool>(INTERNAL_CVARS_PATH, "cheatmode");
				_cheatMode.Initialize();
			}
			return _cheatMode;
		}
	}

	public static void Boot()
	{
		// Since the config CVars are lazy initialized we poke them at startup to make sure they
		// exist before we start registering cvars from the auto-crawl directory.
		// This is done from within the CVarCrawler.
		// Note: This might be hacky, but it makes the autoloader less fragile.

		_ = CheatMode;
		_ = ConsoleMaxLines;
		_ = RuntimeConsoleEnabled;
		_ = ConsoleUpdateRate;
	}
}
