using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.Utilities;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Config;

// TODO: HUGE CHANGE CFG SYSTEM!! LOOK HERE -- Bookmark 
// Huge change:
// The system will use the extention "ecfg" instead. It stands for ExecutableConfig.
// This makes us able to ignore other files that may potentially share directory without having to rely on hacky naming.
// Eg users: 
// active.cfg -- Godot file
// Timmy.ecfg -- Executable Config, and in this case, a valid profile

// TODO: Add a generic "UserConfigCRUDEResponseStatus" enum.
public static class UserConfigManager
{
	// Constants for easier access within the filesystem.
	// We are using a Godot config file to get the stored last user.
	const string FILENAME = "active.cfg";
	const string SECTION = "Boot";
	const string KEY = "last_used_config";

	public static ConfigRef[] GetAvailableConfigs(string term = "*")
	{
		throw new NotImplementedException();
	}

	public static bool RenameConfig(string configName, string newName, out string error)
	{
		throw new NotImplementedException();
	}

	// TODO: Add a "CreateUserConfigResponse" - This can help with GUI additions later on.
	public static bool CreateUserConfig(string configName, bool selectOnCreate = true)
	{
		// Create a user profile file and potentially select it.
		// Returns true if a profile was successfully created.
		throw new NotImplementedException();
	}

	// TODO: Add a "RemoveUserConfigResponse" - This can help with GUI additions later on.
	public static bool RemoveUserConfig(string configName)
	{
		throw new NotImplementedException();
	}

	// TODO: Add a "SelectUserConfigResponse" - This can help with GUI additions later on.
	public static bool TrySelectConfig(string configName, out string error)
	{
		throw new NotImplementedException();
	}

	public static void SaveCurrentConfig()
	{
		throw new NotImplementedException();
	}

	public static string GetCurrentConfig(string fallbackProfile = "default")
	{
		throw new NotImplementedException();
	}

	public static bool TrySetCurrentConfig(string configName, out string error)
	{
		throw new NotImplementedException();
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
