using System;

#nullable enable
namespace FractalPike.PikeConsole.Core.Utilities;

/// <summary>
/// Struct containing a response status.
/// </summary>
public readonly struct Response<T>(T status, string message = "", string[]? flags = null) where T : Enum
{
	public readonly T Status = status;
	public readonly string Message = message ?? string.Empty;
	public readonly string[] Flags = flags ?? [];
}

/// <summary>
/// Struct containing a response status AND payload.
/// </summary>
public readonly struct Response<T, P>(T status, P payload, string message = "", string[]? flags = null) where T : Enum
{
	public readonly T Status = status;
	public readonly P Payload = payload;
	public readonly string Message = message ?? string.Empty;
	public readonly string[] Flags = flags ?? [];
}