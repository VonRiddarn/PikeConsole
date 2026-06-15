namespace FractalPike.PikeConsole.Core.RuntimeExecution;

public enum ExecutionSource
{
	///<summary>Enum fallback for unknown source.</summary>
	None = 0,
	///<summary>The player is executing the command via the console.</summary>
	Player = 1,
	///<summary>The system is executing the command via any means.</summary>
	System = 2,
	// Network = 3 -- This would be cool if we ever add RCon commands!
}