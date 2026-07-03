namespace FractalPike.PikeConsole.Core.RuntimeExecution.Aliases;

public readonly struct Alias(string signature, string statement)
{
	public readonly string Signature = signature;
	public readonly string Statement = statement;
}