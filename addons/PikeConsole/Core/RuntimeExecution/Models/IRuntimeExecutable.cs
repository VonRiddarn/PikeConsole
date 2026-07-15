using FractalPike.PikeConsole.Core.Utilities;

namespace FractalPike.PikeConsole.Core.RuntimeExecution;

public interface IRuntimeExecutable
{
	/// <summary>The user-facing name of this type. EG: "CVar_Boolean"</summary>
	public string DisplayType { get; }
	/// <summary>Command signature to run this executable, EG: "r_cleardecals" or "env_gc"</summary>
	/// <remarks>A signature has no set naming rules, but it is convention to name it after scope separated using underscores.</remarks>
	public string Signature { get; }

	/// <summary>The short description (1 line) for this executable. EG: "Force-activates the garbage collector"</summary>
	public string ShortDesc { get; }

	/// <summary>The long description for this executable. Go wild.</summary>
	public string LongDesc { get; }

	// TODO: Make Usage a string array instead. 
	// If the list of CVars and commands in need of usage overrides grow, we might opt for a centralized approach instead.
	/// <summary>Usage example of this executable, EG: "em_find [enemy id | enemy type]" or "env_gc [no args]"</summary>
	public string Usage { get; }

	/// <summary>If set to true CheatMode must be active to run this executable.</summary>
	public bool IsCheat { get; }

	public string SourceLocation { get; }

	// public bool IsLocal { get; } -- Placing this here as a reminder if we want to tackle multiplayer someday.
	// If (!IsLocal && !Rcon.TryPermissions(Role.Admin)) -- Or something like that.

	/// <summary>Runs the executable and returns a response object.</summary>
	public Response<ExecutionResponseStatus> Execute(ExecutionSource source, string[] args);

	/// <summary>Returns a formatted summary of this object and all its properties.</summary>
	public string GetHelp();
}
