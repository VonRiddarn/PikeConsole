namespace FractalPike.PikeConsole.Core.RuntimeExecution.Commands;

/// <remarks>Since commands are context based an alias can be registered in a 
/// window where the command is not yet, or no longer, registered.
/// <b>Commands will automatically force-remove any alias currently registered with the same signature.</b></remarks>
public enum RegisterCommandResponseStatus
{
	/// <summary>Default fallback.</summary>
	None = 0,
	/// <summary>Command registered.</summary>
	Success = 1,
	/// <summary>Command force-removed an alias of the same signature on registration.</summary>
	ReplacedAlias = 2,
	/// <summary>Denied because a command with that signature already exists.</summary>
	AlreadyExists = 3
}
