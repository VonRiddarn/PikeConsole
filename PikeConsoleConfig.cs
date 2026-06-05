namespace FractalPike.PikeConsole;
// ----- ----- Logger ----- -----
public static class PikeConsoleConfig
{
	// META
	// MUST match the <PathMap> value in your .csproj exactly!
	public const string PATH_MAP_ALIAS = "";

	// EDITOR
	public const string COLOR_INFO = "#ebebeb";
	public const string COLOR_SUCCESS = "#B2FF73";
	public const string COLOR_WARNING = "#FFC973";
	public const string COLOR_ERROR = "#FF7373";

	// RUNTIME SETTINGS
	/// <summary>
	/// If enabled logs are emitted from the PikeLogger as usual. Disabling this acts as a runtime killswitch, making the logger no-op.
	/// </summary>
	/// <value>State (on / off) for the runtime console logger.</value>
	public static bool EnableRuntimeLogging { get; set; } = true;
}
