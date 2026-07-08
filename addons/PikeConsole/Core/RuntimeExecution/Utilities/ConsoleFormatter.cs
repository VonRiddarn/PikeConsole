using System.Runtime.CompilerServices;

namespace FractalPike.PikeConsole.Core.RuntimeExecution;

public static class ConsoleFormatter
{
	// Not super strict or anything, but this just trims the signature and turns spaces into underscores.
	// The statement parser separates commands from arguments with spaces, so it's important the signature is valid.
	public static string ToSignature(string name) => !string.IsNullOrWhiteSpace(name) ? name.Trim().ToLower().Replace(' ', '_') : "";

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string FormatHelp(IRuntimeExecutable rte)
	{
		return $"Signature: {rte.Signature}\nType: {rte.DisplayType.ToUpper()}\nIs cheat: {rte.IsCheat}\nBrief: {rte.ShortDesc}\nUsage: {rte.Usage}\nDescription: {rte.LongDesc}";
	}
}