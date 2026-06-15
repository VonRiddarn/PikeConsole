namespace FractalPike.PikeConsole.Core.RuntimeExecution;

public enum CvarSetResponseStatus
{
	/// <summary>Default fallback.</summary>
	None = 0,
	/// <summary>The value passed is the same as the current value.</summary>
	NoChange = 1,
	/// <summary>Executed expectedly.</summary>
	Success = 2,
	/// <summary>Denied because of invalid arguments.</summary>
	InvalidArgs = 3,
	/// <summary>Failed expectedly (IE: Through gameplay context).</summary>
	Failed = 4,
	/// <summary>Failed unexpectedly.</summary>
	Error = 5
}