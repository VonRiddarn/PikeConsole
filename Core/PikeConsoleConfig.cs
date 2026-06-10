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
	const string SETTINGS_RUNTIME = $"{SETTINGS_ROOT}/runtime";
	const string SETTINGS_EDITOR = $"{SETTINGS_ROOT}/editor";
	const string SETTINGS_EDITOR_COLORS = $"{SETTINGS_EDITOR}/colors";


	// Lazy initialize settings from the Godot engine when necessary. 
	// This ensures that we only cross the interop bridge once AND respect the users custom project settings.
	static string? _pathMap = null;
	public static string PathMap => _pathMap ??= ProjectSettings.GetSetting($"{SETTINGS_ROOT}/pathmap", "").AsString();

	static string? _cvarDirectory = null;
	public static string CvarDirectory => _cvarDirectory ??= ProjectSettings.GetSetting($"{SETTINGS_ROOT}/cvar_directory", "res://cvars").AsString();

	static int? _maxUiLogs = null;
	/// <summary>
	/// Max amount of log UI elements to spawn. FIFO system.
	/// </summary>
	public static int MaxUiLogs => _maxUiLogs ??= ProjectSettings.GetSetting($"{SETTINGS_RUNTIME}/max_ui_logs", 500).AsInt32();

	static bool? _supressDocumentationWarnings = null;
	/// <summary>
	/// If set to true, warnings about commands not containing propper documentation such as "usage" and "shortDesc" are supressed.
	/// </summary>
	public static bool SupressDocumentationWarnings =>
		_supressDocumentationWarnings ??= ProjectSettings.GetSetting($"{SETTINGS_EDITOR}/suppress_documentation_warnings", false).AsBool();

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


	public static string TestSettings() => $@"
PathMap: {PathMap}
CvarDirectory: {CvarDirectory}
MaxUiLogs: {MaxUiLogs}
SupressDocumentationWarnings: {SupressDocumentationWarnings}
InfoColor: {InfoColor.ToHtml(false)}
SuccessColor: {SuccessColor.ToHtml(false)}
WarningColor: {WarningColor.ToHtml(false)}
ErrorColor: {ErrorColor.ToHtml(false)}
";
}
