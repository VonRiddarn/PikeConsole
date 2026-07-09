namespace FractalPike.PikeConsole.Core.RuntimeExecution;

public enum ExecutionSource
{
	///<summary>The player, or any other untrusted system is executing the command via console or file.</summary>
	Standard = 1,
	///<summary>The system is executing the command via protected means.</summary>
	System = 2,
	// Network = 3 -- This would be cool if we ever add RCon commands!
}