namespace FractalPike.PikeConsole.Core;

// TODO: Use the settings from the godot editor and lazy-initialize them in here.
// Note: Some settings will be CVars in the future.

// ----- ----- Logger ----- -----
public static class PikeConsoleConfig
{

	// META
	// MUST match the <PathMap> value in your .csproj exactly!
	public const string PATH_MAP = "";

	// EDITOR
	public const string COLOR_INFO = "#ebebeb";
	public const string COLOR_SUCCESS = "#B2FF73";
	public const string COLOR_WARNING = "#FFC973";
	public const string COLOR_ERROR = "#FF7373";

	/// <summary>
	/// If set to true, warnings about commands not containing propper documentation such as "usage" and "shortDesc" are supressed.
	/// </summary>
	public const bool SUPPRESS_DOCUMENTATION_WARNINGS = false;

	// RUNTIME SETTINGS
	/// <summary>
	/// If enabled logs are emitted from the PikeLogger as usual. Disabling this acts as a runtime killswitch, making the logger no-op.
	/// </summary>
	/// <value>State (on / off) for the runtime console logger.</value>
	public static bool EnableRuntimeLogging { get; set; } = true;
}
