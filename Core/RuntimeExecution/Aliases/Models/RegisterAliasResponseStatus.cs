namespace FractalPike.PikeConsole.Core.RuntimeExecution.Aliases;

public enum RegisterAliasResponseStatus
{
	None = 0,
	Success = 1,
	Replaced = 2,
	AlreadyExistsAsExecutable = 3,
	AlreadyExists = 4
}
