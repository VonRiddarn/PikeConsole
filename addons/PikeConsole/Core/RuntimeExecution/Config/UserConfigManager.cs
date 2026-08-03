using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.Utilities;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Config;

// This class was not fun to work with. It sucked.
// I hope I never have to go back in here, ever.
public static class UserConfigManager
{
	// Constants for easier access within the filesystem.
	// We are using a Godot config file to get the stored last user.
	const string TRACKER_FILENAME = "active.cfg";
	const string SECTION = "Boot";
	const string KEY = "last_used_config";

	const string DEFAULT_CONFIG_NAME = "default.ecfg";

	static string GetPath(string profileName) =>
		FileSystemHelper.UserDirectory.Globalized(PikeConsoleSettings.UserConfigsDirectory, ConfigRef.DisplayToFileName(profileName));

	static string TrackerPath =>
		FileSystemHelper.UserDirectory.Globalized(PikeConsoleSettings.UserConfigsDirectory, TRACKER_FILENAME);

	public static event Action<ConfigRef> ActiveConfigChanged;

	static ConfigRef _activeConfig = null;
	public static ConfigRef ActiveConfig
	{
		get
		{
			if (_activeConfig != null)
				return _activeConfig;

			ConfigFile config = new();

			// If the tracker file (users/active.cfg) is missing we just force create it and push the user into a default config
			if (config.Load(TrackerPath) != Error.Ok || !config.HasSectionKey(SECTION, KEY))
			{
				_activeConfig = new ConfigRef(GetPath(DEFAULT_CONFIG_NAME));

				var response = CreateAndSelectDefaultConfig();

				if (response.Status != ConfigResponseStatus.Success)
					PikeLogger.LogError(LogTarget.All, $"The config selection failed. Error: {response.Message}", tags: response.Tags);

				return _activeConfig;
			}

			// If the tracker is loaded we just read the value, apply it to the cache and return.
			string lastUsed = config.GetValue(SECTION, KEY).AsString();
			_activeConfig = new ConfigRef(GetPath(lastUsed));
			return _activeConfig;
		}
	}

	/// <summary>
	/// Creates a file for the default config and selects the file within the current config tracker.
	/// </summary>
	public static Response<ConfigResponseStatus> CreateAndSelectDefaultConfig()
	{
		var defaultConfig = new ConfigRef(GetPath(DEFAULT_CONFIG_NAME));

		string[] rows = [
			"// ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ",
			"// THIS FILE IS VOLATILE AND CHANGES / ADDITIONS MAY BE OVERWRITTEN!",
			"// ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- "
		];

		if (!File.Exists(defaultConfig.FullPath))
			ConfigIO.WriteToConfig(rows, defaultConfig.FullPath, false);

		return SelectConfig(DEFAULT_CONFIG_NAME);
	}

	// Methods are ordered in CRUD.
	// ----- ----- CREATE ----- -----
	public static Response<ConfigResponseStatus> CreateConfig(string configName, bool selectOnCreate = true)
	{
		ConfigRef newConfig = new(GetPath(configName));

		var writeRes = ConfigIO.WriteToConfig([$"// {configName}"], newConfig.FullPath, overwrite: false);

		if (writeRes.Status != ConfigResponseStatus.Success)
			return writeRes;

		if (selectOnCreate)
			return SelectConfig(newConfig.FileName);

		return new(ConfigResponseStatus.Success, $"Profile \"{newConfig.DisplayName}\" created successfully.");
	}

	// ----- ----- READ ----- -----
	public static Response<ConfigResponseStatus, ConfigRef[]> GetAvailableConfigs(string term = "*")
	{
		// "*" becomes something like c:/.../users/*.ecfg
		// "Tompa Tjompa" becomes something like: c:/.../users/tompa_tjompa.ecfg
		return ConfigIO.GetConfigs(GetPath(term));
	}

	// ----- ----- UPDATE ----- -----
	public static Response<ConfigResponseStatus> RenameConfig(string configName, string newName)
	{
		if (FileSystemHelper.HasInvalidChars(newName))
			return new(
				ConfigResponseStatus.InvalidArgs,
				$"User profile contains invalid characters! Filenames may not include: [{string.Join(", ", Path.GetInvalidFileNameChars())}]",
				[LogTags.InvalidArgs]);

		ConfigRef oldConfig = new(GetPath(configName));
		newName = ConfigRef.DisplayToFileName(newName);

		// Before we actually apply anything, cache if we are renaming the current active profile.
		bool isActiveProfile = oldConfig.FileName == ActiveConfig.FileName;

		var response = ConfigIO.RenameConfig(newName, oldConfig.FullPath);

		// If the active profile was renamed, select it again to get rid of the zomvbie state in active.cfg
		// This will also trigger the event, which in turn could help update any UI elements
		if (response.Status == ConfigResponseStatus.Success && isActiveProfile)
			UpdateActiveConfigTracker(new ConfigRef(GetPath(newName)));

		return response;
	}

	public static Response<ConfigResponseStatus> SaveCurrentConfig() => SaveConfig(ActiveConfig.DisplayName);
	public static Response<ConfigResponseStatus> SaveConfig(string configName)
	{
		ConfigRef config = new(GetPath(configName));
		if (!File.Exists(config.FullPath))
			return new(ConfigResponseStatus.NotFound, $"Cannot save \"{config.DisplayName}\". Profile does not exist.", [LogTags.NotFound]);

		var snapshot = PersistentCVarRegistry.GetSnapshot();
		List<string> rows = [
			"// ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ",
			"// THIS FILE IS VOLATILE AND CHANGES / ADDITIONS MAY BE OVERWRITTEN!",
			"// ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- "
			];

		foreach (ICVar cvar in snapshot.Values)
		{
			if (cvar.IsModified)
				rows.Add($"{cvar.Signature} {cvar.FormattedValue} {FileSystemHelper.RAM_ONLY_FLAG}; // [{cvar.DisplayType}] {cvar.CurrentValueDisplay}");
		}

		return ConfigIO.WriteToConfig([.. rows], config.FullPath, overwrite: true);
	}

	/// <summary>
	/// Resets all cvars in the persistent memory to default values.
	/// </summary>
	/// <remarks>
	/// This will overwrite the current config, deleting all data.  
	/// It is recommended to not call this method before prompting the player with a "are you sure" window, if triggerable in the GUI.
	/// </remarks>
	/// <returns></returns>
	public static Response<ConfigResponseStatus> FullResetCurrentConfig()
	{
		try
		{
			PersistentCVarRegistry.ResetAll(ramOnly: false);
		}
		catch (Exception e)
		{
			return new(ConfigResponseStatus.Error, $"Failed to reset all Cvars. Error: {e.Message}");
		}

		return new(ConfigResponseStatus.Success, null);
	}

	public static Response<ConfigResponseStatus> SelectConfig(string configName)
	{
		if (string.IsNullOrWhiteSpace(configName))
			return new(ConfigResponseStatus.InvalidArgs, "Config name cannot be empty.");

		ConfigRef targetConfig = new(GetPath(configName));

		if (!File.Exists(targetConfig.FullPath))
			return new(ConfigResponseStatus.NotFound, $"Cannot select profile \"{targetConfig.DisplayName}\". File does not exist.");

		if (!UpdateActiveConfigTracker(targetConfig))
			return new(ConfigResponseStatus.Error, $"Failed to save active config to file!");

		// IMPORTANT: The ramOnly flag here is F-ing crucial. Do not remove it!
		// If ram only is not set, all variables will reset using persistence, meaning we clear and save the current config.
		PersistentCVarRegistry.ResetAll(ramOnly: true);

		var initializeResponse = ConfigIO.ExecuteFromConfig(ExecutionSource.Standard, targetConfig.FullPath, silent: true);

		if (initializeResponse.Status != ConfigResponseStatus.Success)
			return initializeResponse;

		return new(ConfigResponseStatus.Success, $"Active profile set to \"{targetConfig.DisplayName}\".");
	}

	// ----- ----- DELETE ----- -----
	public static Response<ConfigResponseStatus> RemoveConfig(string configName)
	{
		ConfigRef target = new(GetPath(configName));

		// Prevent deleting the active profile, as that would leave the cache and tracker in a zombie state
		if (target.FileName == ActiveConfig.FileName)
			return new(ConfigResponseStatus.Failed, "Cannot delete the currently active profile.", [LogTags.Failed]);

		var response = ConfigIO.RemoveConfig(target.FullPath);

		if (response.Status == ConfigResponseStatus.Success)
			return new(ConfigResponseStatus.Success, $"Removed profile \"{target.DisplayName}\".");

		return response;
	}


	// ----- ----- HELPERS ----- -----
	/// <summary>
	/// Updates the active.cfg tracker and cached ConfigRef variable.
	/// </summary>
	private static bool UpdateActiveConfigTracker(ConfigRef target)
	{
		ConfigFile gdConfig = new();
		gdConfig.Load(TrackerPath);

		gdConfig.SetValue(SECTION, KEY, target.FileName);

		if (gdConfig.Save(TrackerPath) != Error.Ok)
			return false;

		_activeConfig = target;
		ActiveConfigChanged?.Invoke(_activeConfig);
		return true;
	}
}