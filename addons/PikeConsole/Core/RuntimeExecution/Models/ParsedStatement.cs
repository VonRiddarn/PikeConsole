namespace FractalPike.PikeConsole.Core.RuntimeExecution;

public readonly struct ParsedStatement(string signature, string[] arguments)
{
	public readonly string Signature = signature;
	public readonly string[] Arguments = arguments;
}
