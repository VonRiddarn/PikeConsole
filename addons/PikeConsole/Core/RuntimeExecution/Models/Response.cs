using System;

#nullable enable
namespace FractalPike.PikeConsole.Core.RuntimeExecution;

/// <summary>
/// Struct containing a response within the RuntimeExecution ecosystem.
/// </summary>
public readonly struct Response<T>(T status, string message = "", string[]? flags = null) where T : Enum
{
	public readonly T Status = status;
	public readonly string Message = message ?? string.Empty;
	public readonly string[] Flags = flags ?? [];
}
