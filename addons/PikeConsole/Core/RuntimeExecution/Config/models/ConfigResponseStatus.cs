namespace FractalPike.PikeConsole.Core.RuntimeExecution.Config;

public enum ConfigResponseStatus
{
	/// <summary>Default fallback.</summary>
	None = 0,
	/// <summary>Executed expectedly.</summary>
	Success = 1,
	/// <summary>Denied because of bad arguments.</summary>
	InvalidArgs = 2,
	/// <summary>A required file was not found.</summary>
	NotFound = 3,
	/// <summary>A conflicting file exists that prevents the action.</summary>
	FileConflict = 4,
	/// <summary>Failed expectedly (IE: Through gameplay context, or internal flags).</summary>
	Failed = 5,
	/// <summary>Failed unexpectedly.</summary>
	Error = 6
}