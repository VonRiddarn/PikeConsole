namespace FractalPike.PikeConsole.Core.RuntimeExecution;

public enum ExecutionResponseStatus
{
	/// <summary>Default fallback.</summary>
	None = 0,
	/// <summary>Executed expectedly.</summary>
	Success = 1,
	/// <summary>Denied because of cheat flag.</summary>
	DeniedCheat = 2,
	/// <summary>Denied because of permission system.</summary>
	//DeniedPermission = 3,
	/// <summary>Denied because of invalid arguments.</summary>
	InvalidArgs = 4,
	/// <summary>Failed expectedly (IE: Through gameplay context).</summary>
	Failed = 5,
	/// <summary>Failed unexpectedly.</summary>
	Error = 6
}