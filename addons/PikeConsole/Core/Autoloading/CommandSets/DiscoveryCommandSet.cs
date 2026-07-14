using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution;
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
using FractalPike.PikeConsole.Core.Utilities;
using System.Text;

namespace FractalPike.PikeConsole.Core.Autoloading;

public partial class DiscoveryCommandSet : CommandSet
{
	protected override Command[] InstantiateCommands() => [
		Command(
			"help",
			"Get detailed help of any command or CVar.",
			null,
			"help [signature]",
			false,
			static (args) => {
				if(!ArgumentParser.ValidateCount(args, 1, out string error))
					return new(ExecutionResponseStatus.InvalidArgs, error);

				string signature = args[0];

				if(RuntimeExecutableRegistry.TryGetExecutable(signature, out var rte))
				{
					PikeLogger.Log(LogTarget.Runtime, $"{rte.GetHelp()}", forceLog: true);
					return new(ExecutionResponseStatus.Success, null);
				}

				return new(ExecutionResponseStatus.Failed, $"Could not find runtime executable with signature \"{signature}\".");
			}
		),
		Command(
			"whereis",
			"Lists the source location of any runtime executables.",
			null,
			"whereis [..signatures]",
			false,
			static (args) => {
				if(args.Length < 1)
					return new(ExecutionResponseStatus.InvalidArgs, "\"whereis\" must be called with at least 1 argument.");

				StringBuilder sb = new("Listing location for resources...");
				string msg = string.Empty;

				foreach(string signature in args)
				{
					msg = RuntimeExecutableRegistry.TryGetExecutable(signature, out var rte)
					? rte.SourceLocation
					: $"No command or cvar found matching signature.";

					sb.Append($"\n[{signature}]\n\t\"{msg}\"");
				}

				PikeLogger.Log(LogTarget.Runtime, $"{sb.ToString()}");
				return new(ExecutionResponseStatus.Success, null);
			}
		),
		Command(
			"find",
			"Lists all comands and CVars with an optional search term.",
			null,
			"find [term?]",
			false,
			static (args) => {
				string term = string.Join(' ', args);
				var rtes = RegistryBrowser.FindExecutables(term, SearchMode.Contains, true);
				return FormatAndLogResults(rtes, term, "results");
			}
		),
		Command(
			"find_command",
			"Lists all comands with an optional search term.",
			null,
			"find_command [term?]",
			false,
			static (args) => {
				string term = string.Join(' ', args);
				var rtes = RegistryBrowser.FindCommands(term, SearchMode.Contains, true);
				return FormatAndLogResults(rtes, term, "commands");
			}
		),
		Command(
			"find_cvar",
			"Lists all CVars with an optional search term.",
			null,
			"find_cvar [term?]",
			false,
			static (args) => {
				string term = string.Join(' ', args);
				var rtes = RegistryBrowser.FindCVars(term, SearchMode.Contains, true);
				return FormatAndLogResults(rtes, term, "cvars");
			}
		),
	];

	// DRY code is nice code. This is basically just a router for all list commands.
	// Commands and CVars are both IRuntimeExecutables, so this is fine.
	static Response<ExecutionResponseStatus> FormatAndLogResults(IRuntimeExecutable[] rtes, string term, string nounPlural)
	{
		if (rtes.Length < 1)
			return new(ExecutionResponseStatus.Success, string.IsNullOrWhiteSpace(term) ? $"No {nounPlural} found." : $"No {nounPlural} found matching \"{term}\".");

		string header = string.IsNullOrWhiteSpace(term) ? $"Showing all {nounPlural}..." : $"Showing {nounPlural} matching \"{term}\"...";
		StringBuilder sb = new(header);

		foreach (var rte in rtes)
			sb.Append($"\n\n[{rte.DisplayType}] \"{rte.Signature}\"\n\t{rte.ShortDesc}");

		PikeLogger.Log(LogTarget.Runtime, $"{sb.ToString()}");

		return new(ExecutionResponseStatus.Success, null);
	}
}
