using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;

namespace FractalPike.PikeConsole.Config;

/*
 * Semi-Internal class that centralizes CVars.
 * End users shouldn't interact with this directly, 
 * they should be routed through PikeConsoleAPI wrapper.
*/

#nullable enable

public static class PikeConsoleCVars
{
	// ----- ----- ----- ----- -----
	// 			INTERNAL
	// ----- ----- ----- ----- -----
	const string INTERNAL_CVARS_PATH = "res://addons/PikeConsole/Config/variables";

	// ----- ----- ----- ----- -----
	// 			 CVars
	// ----- ----- ----- ----- -----

	static CVarInt? _maxUiLogs;
	/// <summary>
	/// Lazy initialized CVar. If it is not initialized at the point of contact, it initializes and caches the reference.
	/// </summary>
	public static CVarInt MaxUiLogs
	{
		get
		{
			if (_maxUiLogs == null)
			{
				// Assign the maxlogs BEFORE we initialize!!
				// Thus when initialize call PikeLogger and PikeLogger asks for MaxLogs we return the cached CVar...
				_maxUiLogs = CvarLoader.LoadInternalCVar<CVarInt>($"{INTERNAL_CVARS_PATH}/console", "console_max_ui_logs");
				_maxUiLogs.Initialize();
			}
			return _maxUiLogs;
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
		_ = MaxUiLogs;
		_ = RuntimeConsoleEnabled;
	}
}
