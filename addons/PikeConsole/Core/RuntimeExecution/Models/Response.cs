using System;

namespace FractalPike.PikeConsole.Core.RuntimeExecution;

/// <summary>
/// Struct containing a response within the RuntimeExecution ecosystem.
/// </summary>
public readonly struct Response<T>(T status, string message = "") where T : Enum
{
	public readonly T Status = status;
	public readonly string Message = message ?? string.Empty;
}
