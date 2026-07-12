using System;
using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;
using Godot;

namespace FractalPike.PikeConsole.Config;

#nullable enable

public static class PikeConsoleConfig
{

	// ----- ----- ----- ----- -----
	// 			INTERNAL
	// ----- ----- ----- ----- -----
	const string INTERNAL_CVARS_PATH = "res://addons/PikeConsole/Config/variables";

	const string PROJECT_SETTINGS_ROOT = "fractal_pike/pike_console";
	const string PROJECT_SETTINGS_CONFIG = $"{PROJECT_SETTINGS_ROOT}/config";
	const string PROJECT_SETTINGS_RUNTIME = $"{PROJECT_SETTINGS_ROOT}/runtime";
	const string PROJECT_SETTINGS_EDITOR = $"{PROJECT_SETTINGS_ROOT}/editor";
	const string PROJECT_SETTINGS_EDITOR_COLORS = $"{PROJECT_SETTINGS_EDITOR}/colors";

	// ----- ----- ----- ----- -----
	// 			 CVars
	// ----- ----- ----- ----- -----

	static CVarInt? _maxUiLogs;
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

	static CVarBool? _consoleLoggerEnabled;
	public static CVarBool ConsoleLoggerEnabled
	{
		get
		{
			if (_consoleLoggerEnabled == null)
			{
				_consoleLoggerEnabled = CvarLoader.LoadInternalCVar<CVarBool>($"{INTERNAL_CVARS_PATH}/console", "console_logger_enabled");
				_consoleLoggerEnabled.Initialize();
			}
			return _consoleLoggerEnabled;
		}
	}

	static CVarBool? _cheatMode;
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
		_ = ConsoleLoggerEnabled;
	}

	// ----- ----- ----- ----- -----
	// 		PROJECT SETTINGS
	// ----- ----- ----- ----- -----

	// ----- ----- PIKE CONSOLE ----- -----
	static string? _pathMap = null;
	public static string PathMap => _pathMap ??= ProjectSettings.GetSetting($"{PROJECT_SETTINGS_ROOT}/pathmap", "").AsString();

	static string? _cvarDirectory = null;
	public static string CvarDirectory => _cvarDirectory ??= ProjectSettings.GetSetting($"{PROJECT_SETTINGS_ROOT}/cvar_directory", "res://cvars").AsString();

	// ----- ----- CONFIG SYSTEM ----- -----
	static string? _configDirectory = null;
	public static string ConfigDirectory => _configDirectory ??= ProjectSettings.GetSetting($"{PROJECT_SETTINGS_ROOT}/config/config_directory_path", "user://cfg").AsString();

	// User configs are automatically placed within the configs directory.
	static string? _userConfigsDirectory = null;
	public static string UserConfigsDirectory => _userConfigsDirectory ??= ConfigDirectory + "/users";

	// The user config system is an opt-in system. This is done in the project settings.
	static bool? _userConfigsEnabled = null;
	public static bool UserConfigsEnabled => _userConfigsEnabled ??= ProjectSettings.GetSetting($"{PROJECT_SETTINGS_CONFIG}/use_user_configs", false).AsBool();

	// ----- ----- RUNTIME ----- -----
	static string? _frontendScenePath = null;
	public static string FrontendScenePath => _frontendScenePath ??= ProjectSettings.GlobalizePath(
		ProjectSettings.GetSetting($"{PROJECT_SETTINGS_RUNTIME}/frontend_scene", "res://addons/PikeConsole/Frontend/pike_console_ui.tscn").AsString());

	// ----- ----- EDITOR ----- -----
	static bool? _logCvarOnRegister = null;
	public static bool LogCvarOnRegister =>
		_logCvarOnRegister ??= ProjectSettings.GetSetting($"{PROJECT_SETTINGS_EDITOR}/log_cvar_on_regsiter", true).AsBool();

	static bool? _logCommandOnRegister = null;
	public static bool LogCommandOnRegister =>
		_logCommandOnRegister ??= ProjectSettings.GetSetting($"{PROJECT_SETTINGS_EDITOR}/log_command_on_regsiter", true).AsBool();

	static bool? _suppressDocumentationWarnings = null;
	public static bool SuppressDocumentationWarnings =>
		_suppressDocumentationWarnings ??= ProjectSettings.GetSetting($"{PROJECT_SETTINGS_EDITOR}/suppress_documentation_warnings", false).AsBool();

	// ----- COLORS -----
	static Color? _infoColor = null;
	public static Color InfoColor => _infoColor ??= ProjectSettings.GetSetting($"{PROJECT_SETTINGS_EDITOR_COLORS}/info", new Color("#EBEBEB")).AsColor();

	static Color? _successColor = null;
	public static Color SuccessColor => _successColor ??= ProjectSettings.GetSetting($"{PROJECT_SETTINGS_EDITOR_COLORS}/success", new Color("#B2FF73")).AsColor();

	static Color? _warningColor = null;
	public static Color WarningColor => _warningColor ??= ProjectSettings.GetSetting($"{PROJECT_SETTINGS_EDITOR_COLORS}/warning", new Color("#FFC973")).AsColor();

	static Color? _errorColor = null;
	public static Color ErrorColor => _errorColor ??= ProjectSettings.GetSetting($"{PROJECT_SETTINGS_EDITOR_COLORS}/error", new Color("#FF7373")).AsColor();
}
