using System.Runtime.CompilerServices;

namespace FractalPike.PikeConsole.Core.RuntimeExecution;

/*
 * Note: This formatter is not necessarily agnoistic.
 * It uses BBCode to ensure correcct indentation in the runtime console.
 * To make the formatter agnostic, just remove all BBCode tags in the FormatHelp method.
*/

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

		if (rte is ICVar cvar)
			return $"Signature: {cvar.Signature}\n[indent]Type: {cvar.DisplayType.ToUpper()}[/indent]\n[indent]Is cheat: {cvar.IsCheat}[/indent]\n[indent]Value: \"{cvar.CurrentValueDisplay}\"[/indent]\n[indent]Default: \"{cvar.DefaultValueDisplay}\"[/indent]\n[indent]Brief: {shortDesc}[/indent]\n[indent]Usage:\n[indent]{string.Join("\n", usg)}[/indent][/indent]\n[indent]Description: {longDesc}[/indent]";
		else
			return $"Signature: {rte.Signature}\n[indent]Type: {rte.DisplayType.ToUpper()}[/indent]\n[indent]Is cheat: {rte.IsCheat}[/indent]\n[indent]Brief: {shortDesc}[/indent]\n[indent]Usage:\n[indent]{string.Join("\n", usg)}[/indent][/indent]\n[indent]Description: {longDesc}[/indent]";
	}
}