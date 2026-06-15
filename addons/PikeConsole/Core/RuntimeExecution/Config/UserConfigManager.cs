using Godot;
using System;
using System.Collections.Generic;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Config;
// This manages the client_settings.cfg file and nothing else.
// It is not a generic config manager. It is the players saved prefered settings.
// This will run on startup, which is how all persistent CVar values are re-added, EG: r_viewdistance 5
public static class UserConfigManager
{
	private static readonly Dictionary<string, string> _activeConfig = [];

	private const string CONFIG_FILENAME = "client_settings.cfg";

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
