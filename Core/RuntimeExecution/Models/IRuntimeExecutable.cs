namespace FractalPike.PikeConsole.Core.RuntimeExecution;

public interface IRuntimeExecutable
{
	/// <summary>The user-facing name of this type. EG: "Boolean"</summary>
	public string DisplayType { get; }
	/// <summary>Command signature to run this executable, EG: "r_cleardecals" or "env_gc"</summary>
	/// <remarks>A signature has no set naming rules, but it is convention to name it after scope separated using underscores.</remarks>
	public string Signature { get; }

	/// <summary>The short description (1 line) for this executable. EG: "Force-activates the garbage collector"</summary>
	public string ShortDesc { get; }

	/// <summary>The long description for this executable. Go wild.</summary>
	public string LongDesc { get; }

	/// <summary>Usage example of this executable, EG: "em_find [enemy id | enemy type]" or "ph_gravity [integer]"</summary>
	public string Usage { get; }

	/// <summary>If set to true CheatMode must be active to run this executable.</summary>
	public bool IsCheat { get; }

	/// <summary>Runs the executable and returns a response object.</summary>
	public Response<ExecutionResponseStatus> Execute(string[] args);

	/// <summary>Returns a formatted summary of this object and all its properties.</summary>
	public string GetHelp();
}
