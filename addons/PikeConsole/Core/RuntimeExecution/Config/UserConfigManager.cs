using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.Utilities;
using Godot;
using System;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Config;

// TODO: HUGE CHANGE CFG SYSTEM!! LOOK HERE -- Bookmark 
// Huge change:
// The system will use the extention "ecfg" instead. It stands for ExecutableConfig.
// This makes us able to ignore other files that may potentially share directory without having to rely on hacky naming.
// Eg users: 
// active.cfg -- Godot file
// Timmy.ecfg -- Executable Config, and in this case, a valid profile

public static class UserConfigManager
{
	// Constants for easier access within the filesystem.
	// We are using a Godot config file to get the stored last user.
	const string FILENAME = "active.cfg";
	const string SECTION = "Boot";
	const string KEY = "last_used_config";
	const string DEFAULT_USER = "user_default";

	public static string[] GetAvailableConfigs()
	{
		throw new NotImplementedException();
	}

	public static void RenameConfig(string configName, string newName)
	{
		throw new NotImplementedException();
	}

	public static bool CreateUserConfig(bool selectOnCreate = true)
	{
		// Create a user profile file and potentially select it.
		// Returns true if a profile was successfully created.
		throw new NotImplementedException();
	}

	public static bool RemoveUserConfig(string configName)
	{
		// Match a filename with the name of the profile. Note that this can be without the "user_" prefix.
		// In those cases we manually add it so that we aren't able to destroy the "active.setting" file.
		throw new NotImplementedException();
	}

	public static bool SelectConfig(string name)
	{
		// Select a profile.
		// Note: 
		// This will clear all the persistant variables using persistantrepo.ResetAll(ramOnly: true)
		// Then select the new profile and run all commands within - which will automatically make settings apply to the persistent cache.
		throw new NotImplementedException();
	}

	public static void SaveCurrentConfig()
	{
		ConfigFile gdConfig = new();

		UserFileSystem.EnsureDirectory(ProjectSettings.GlobalizePath(PikeConsoleConfig.UserConfigsDirectory));

		if (gdConfig.Load($"{PikeConsoleConfig.UserConfigsDirectory}/{FILENAME}") != Error.Ok)
		{
			gdConfig.SetValue(SECTION, KEY, DEFAULT_USER);
		}
	}

	public static string GetCurrentConfig(string fallbackProfile = "default")
	{
		ConfigFile gdConfig = new();

		if (gdConfig.Load($"{PikeConsoleConfig.UserConfigsDirectory}/{FILENAME}") != Error.Ok)
			return fallbackProfile;

		return gdConfig.GetValue(SECTION, KEY, fallbackProfile).AsString();
	}

	// TODO: Implement logic here
	// Note: We might want to prime for multiple profiles right away...
	// Instead of a static config, we have a selected one, and a list of ones that can be selected.
	// All configs are located (by default) in : user://cfg/users
	// There they have the strict naming: "user_*"
	// 
	// This allows us to parse the folder for all user profiles.
	// We use Godots persistent settings to keep track of what profile was last selected.
	// When we re-open the game we compare all available setting files to the last selected.
	// If it exists, we boot into it. Otherwise we pick the first one we can get. Otherwise, we boot to defaults and create one: user_default.
	// 
	// We could also have static getters inside this script for that. 
	// Which allows us to even interact with user profiles through the GUI.
	// When we save the GUI, we don't write to "the" user profile. We write to the selected user profile.
	// NOTE TO SELF: Make sure to show the CFG system in action and why we use the `StatementExecutor` instead of Godots own `ConfigFile` system.
	// A good example is how the `StatementExecutor` will not crash when new settings are added or old removed.
	// It will print "Unknown command" and move on. 
}
