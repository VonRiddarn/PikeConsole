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
		var usg = (rte.Usages is null or [] or [""] or [null]) ? ["No usage instructions"] : rte.Usages;
		var shortDesc = string.IsNullOrWhiteSpace(rte.ShortDesc) ? "No description available." : rte.ShortDesc;
		var longDesc = string.IsNullOrWhiteSpace(rte.LongDesc) ? "No long description available." : rte.LongDesc;

		return $"Signature: {rte.Signature}\n\tType: {rte.DisplayType.ToUpper()}\n\tIs cheat: {rte.IsCheat}\n\tBrief: {shortDesc}\n\tUsage:\n\t\t{string.Join("\n\t\t", usg)}\n\tDescription: {longDesc}";
	}
}