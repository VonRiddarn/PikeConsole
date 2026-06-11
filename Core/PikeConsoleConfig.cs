using System;
using Godot;

namespace FractalPike.PikeConsole.Core;

#nullable enable
// TODO: Use the settings from the godot editor and lazy-initialize them in here.
// Note: Some settings will be CVars in the future.

// ----- ----- Logger ----- -----
public static class PikeConsoleConfig
{
	// Shorthands for DRY code
	// Since this is interop stuff that is being set from "pike_console.gd" everything MUST MATCH!!
	// If there are import errors, check the definitions at: res://addons/PikeConsole/pike_console.gd -- This is the init script.
	const string SETTINGS_ROOT = "fractal_pike/pike_console";
	const string SETTINGS_CONFIG = $"{SETTINGS_ROOT}/config";
	const string SETTINGS_RUNTIME = $"{SETTINGS_ROOT}/runtime";
	const string SETTINGS_EDITOR = $"{SETTINGS_ROOT}/editor";
	const string SETTINGS_EDITOR_COLORS = $"{SETTINGS_EDITOR}/colors";


	// Lazy initialize settings from the Godot engine when necessary. 
	// This ensures that we only cross the interop bridge once AND respect the users custom project settings.
	static string? _pathMap = null;
	public static string PathMap => _pathMap ??= ProjectSettings.GetSetting($"{SETTINGS_ROOT}/pathmap", "").AsString();

	static string? _cvarDirectory = null;
	public static string CvarDirectory => _cvarDirectory ??= ProjectSettings.GetSetting($"{SETTINGS_ROOT}/cvar_directory", "res://cvars").AsString();

	static string? _configDirectory = null;
	public static string ConfigDirectory => _configDirectory ??= ProjectSettings.GlobalizePath(
		ProjectSettings.GetSetting($"{SETTINGS_CONFIG}/config_directory", "user://cfg").AsString());

	static string? _userConfigsDirectory = null;
	public static string UserConfigsDirectory => _userConfigsDirectory ??= ProjectSettings.GlobalizePath(
		ProjectSettings.GetSetting($"{SETTINGS_CONFIG}/config_directory", "user://cfg").AsString()) + "/users";

	static bool? _userConfigsEnabled = null;
	public static bool UserConfigsEnabled => _userConfigsEnabled ??= ProjectSettings.GetSetting($"{SETTINGS_CONFIG}/use_user_configs", false).AsBool();

	static int? _maxUiLogs = null;
	/// <summary>
	/// Max amount of log UI elements to spawn. FIFO system.
	/// </summary>
	public static int MaxUiLogs => _maxUiLogs ??= ProjectSettings.GetSetting($"{SETTINGS_RUNTIME}/max_ui_logs", 500).AsInt32();

	static bool? _suppressDocumentationWarnings = null;
	/// <summary>
	/// If set to true, warnings about commands not containing propper documentation such as "usage" and "shortDesc" are suppressed.
	/// </summary>
	public static bool SuppressDocumentationWarnings =>
		_suppressDocumentationWarnings ??= ProjectSettings.GetSetting($"{SETTINGS_EDITOR}/suppress_documentation_warnings", false).AsBool();

	// EDITOR
	static Color? _infoColor = null;
	public static Color InfoColor => _infoColor ??= ProjectSettings.GetSetting($"{SETTINGS_EDITOR_COLORS}/info", new Color("#EBEBEB")).AsColor();

	static Color? _successColor = null;
	public static Color SuccessColor => _successColor ??= ProjectSettings.GetSetting($"{SETTINGS_EDITOR_COLORS}/success", new Color("#B2FF73")).AsColor();

	static Color? _warningColor = null;
	public static Color WarningColor => _warningColor ??= ProjectSettings.GetSetting($"{SETTINGS_EDITOR_COLORS}/warning", new Color("#FFC973")).AsColor();

	static Color? _errorColor = null;
	public static Color ErrorColor => _errorColor ??= ProjectSettings.GetSetting($"{SETTINGS_EDITOR_COLORS}/error", new Color("#FF7373")).AsColor();

	// RUNTIME SETTINGS
	/// <summary>
	/// If enabled logs are emitted from the PikeLogger as usual. Disabling this acts as a runtime killswitch, making the logger no-op.
	/// </summary>
	/// <value>State (on / off) for the runtime console logger.</value>
	public static bool EnableRuntimeLogging { get; set; } = true;

	private static bool _cheatMode = false;


	public static event Action<bool>? CheatModeChanged;

	// TODO: CHEATMODE IS NOT A CVAR!!! NOTE TO SELF
	// BIG UPGRADE FROM THE UNITY FRAMEWORK.
	// We will not force inject cheats as a CVar. It makes no sense to add such a fragile wrapper.
	// Instead, we will register it as a COMMAND in the GlobalCommandSet. And that command will just affect this static variable.

	/// <summary>
	/// The state variable for cheatmode.
	/// </summary>
	/// <remarks>
	/// This should rarely be consumed directly. Check out the <c>PikeConsoleConfig.CheatModeChanged</c> event instead!
	/// </remarks>
	public static bool CheatMode
	{
		get => _cheatMode;
		set
		{
			if (_cheatMode == value) return;

			_cheatMode = value;
			CheatModeChanged?.Invoke(_cheatMode);
		}
	}


	public static string TestSettings() => $@"
PathMap: {PathMap}
CvarDirectory: {CvarDirectory}
ConfigDirectory: {ConfigDirectory}
UserConfigsEnabled: {UserConfigsEnabled}
UserConfigsDirectory: {UserConfigsDirectory}
MaxUiLogs: {MaxUiLogs}
SuppressDocumentationWarnings: {SuppressDocumentationWarnings}
InfoColor: {InfoColor.ToHtml(false)}
SuccessColor: {SuccessColor.ToHtml(false)}
WarningColor: {WarningColor.ToHtml(false)}
ErrorColor: {ErrorColor.ToHtml(false)}
";
}
