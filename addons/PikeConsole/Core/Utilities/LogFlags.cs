namespace FractalPike.PikeConsole.Core.Utilities;

public static class LogFlags
{
	public const string NoHeader = "pikeconsole_no_header";

	public const string InvalidArgs = "pikeconsole_invalid_arguments";
	public const string Failed = "pikeconsole_failed";
	public const string DeniedCheat = "pikeconsole_denied_cheat";
	public const string ValueLimited = "pikeconsole_value_limited";
	public const string ValueClamped = "pikeconsole_value_clamped";

	public const string NotFound = "pikeconsole_not_found";

	// TODO: Implement this - requires going through all exceptions in RTE.
	//public const string Exception = "pikeconsole_exception";
}