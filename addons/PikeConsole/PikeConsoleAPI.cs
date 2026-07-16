/*
 * Public facing facade pattern class.
 * This is a simplified way of interacting with complex and scattered internal systems.
 * If a user (that might be you) wants to check cheatmode through code or whatever, 
 * this class manages the hard bits and provides ready to use variables and methods.
 * 
 * IMPORTANT NOTE:
 * This wrapper is for the end user only!
 * It is meant for one-way complexity diving.
 * Internal systems should NEVER access this property.
 * 
 * The one exception to this rule is the standard frontend implementation, 
 * which is not truly internal.
*/

using System;
using FractalPike.PikeConsole.Config;

namespace FractalPike.PikeConsole;

public static class PikeConsoleAPI
{
	public static bool CheatMode => PikeConsoleCVars.CheatMode.Value;
	public static bool SetCheatMode(bool newState) => PikeConsoleCVars.CheatMode.Value = newState;

	/// <summary>
	/// Wrapper for all things related to the Runtime console.
	/// </summary>
	/// <remarks>
	/// This class manages everything from events to enabling / disabling the console. <br />
	/// No unnecessary middleman data is allocated, and requests are routed using static references.
	/// </remarks>
	public static class RuntimeConsole
	{
		/// <summary>Get the current state of the runtime console. False makes the console and logger are no-op!</summary>
		public static bool IsEnabled => PikeConsoleCVars.RuntimeConsoleEnabled.Value;
		/// <summary>Set the current state of the runtime console. False makes the console and logger are no-op!</summary>
		public static void SetEnabled(bool newState) => PikeConsoleCVars.RuntimeConsoleEnabled.Value = newState;
		/// <summary>Toggle the current state of the runtime console. When false, the console and logger are no-op!</summary>
		public static void ToggleEnabled() => PikeConsoleCVars.RuntimeConsoleEnabled.Value = !PikeConsoleCVars.RuntimeConsoleEnabled.Value;
		/// <summary>Subscribe to the state of the console. Useful for hooking up pre-existing pause systems to the console state.</summary>
		public static event Action<bool> EnabledChanged
		{
			add => PikeConsoleCVars.RuntimeConsoleEnabled.ValueChanged += value;
			remove => PikeConsoleCVars.RuntimeConsoleEnabled.ValueChanged -= value;
		}

		/// <remarks>
		/// This is a wrapper for "PikeConsoleCVars.MaxUiLogs". <br />
		/// If subscriptions to ValueChanged is needed for some reason, access the property directly instead.
		/// </remarks>
		public static int MaxUiLogs
		{
			get => PikeConsoleCVars.MaxUiLogs.Value;
			set { PikeConsoleCVars.MaxUiLogs.Value = value; }
		}
	}

	// Future stuff.
	// public static class UserConfig
	// {
	// 	public static class Current
	// 	{
	// 		public static ConfigRef Info => UserConfigManager.ActiveConfig;
	// 		public static void Save() => UserConfigManager.SaveCurrentConfig();
	// 	}

	// 	public static ConfigRef[] All => UserConfigManager.GetAvailableConfigs().Payload;
	// 	public static void Select(string name) => UserConfigManager.SelectConfig(name);
	// 	public static void Create(string name, bool selectOnCreate) => UserConfigManager.CreateConfig(name, selectOnCreate);

	// }
}
