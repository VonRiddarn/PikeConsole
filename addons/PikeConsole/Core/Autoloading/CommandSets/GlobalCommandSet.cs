using System.Text;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution;
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
using Godot;

namespace FractalPike.PikeConsole.Core.Autoloading;

public partial class GlobalCommandSet : CommandSet
{
	protected override Command[] InstantiateCommands() => [
		Command(
			"echo",
			"Send a message to the console.",
			"Combines all arguments into a string and returns the concatenated result.",
			"echo [..args]",
			false,
			(args) => {
				PikeLogger.Log(LogTarget.Runtime, $"{string.Join(' ', args)}", forceLog: true, domain: "PikeConsole.Frontend");
				return new(ExecutionResponseStatus.Success, null);
			}
		),
		Command(
			"push_warning",
			"Push a warning to the Godot engine using GD.PushWarning. Used to test interop logger.",
			"Combines all arguments into a string and pushes it to the Godot engine as a warning.",
			"echo [..args]",
			false,
			(args) => {
				GD.PushWarning(string.Join(' ', args));
				return new(ExecutionResponseStatus.Success, null);
			}
		),
		Command(
			"push_error",
			"Push a warning to the Godot engine using GD.PushError. Used to test interop logger.",
			"Combines all arguments into a string and pushes it to the Godot engine as an error.",
			"echo [..args]",
			false,
			(args) => {
				GD.PushError(string.Join(' ', args));
				return new(ExecutionResponseStatus.Success, null);
			}
		),
		Command(
			"count",
			"Count all passed arguments.",
			"Counts all arguments and logs an integer of the count.",
			"count [..args]",
			false,
			(args) => {
				PikeLogger.Log(LogTarget.Runtime, $"{args.Length.ToString()}", forceLog: true, domain: "PikeConsole.Frontend");
				return new(ExecutionResponseStatus.Success, null);
			}
		),
		Command(
			"help",
			"Get detailed help of any command or CVar.",
			null,
			"help [signature]",
			false,
			(args) => {
				if(!ArgumentParser.ValidateCount(args, 1, out string error))
					return new(ExecutionResponseStatus.InvalidArgs, error);

				string signature = args[0];

				if(RuntimeExecutableRegistry.TryGetExecutable(signature, out var rte))
				{
					PikeLogger.Log(LogTarget.Runtime, $"{rte.GetHelp()}", forceLog: true, domain: "PikeConsole.Frontend");
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
			(args) => {
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

				return new(ExecutionResponseStatus.Success, sb.ToString());
			}
		),
		Command(
			"reset",
			"Reset the value of a CVar.",
			"Reset the value of a CVar and remove persistance overrides from the player settings config.",
			"reset [signature]",
			false,
			(args) => {
				if(!ArgumentParser.ValidateCount(args, 1, out string error))
					return new(ExecutionResponseStatus.InvalidArgs, error);

				string signature = args[0];

				if(RuntimeExecutableRegistry.TryGetExecutable(signature, out var rte))
				{
					if(rte is ICVar cvar)
					{
						if(!cvar.ResetValue(ExecutionSource.Player))
							return new(ExecutionResponseStatus.DeniedCheat, $"Failed to reset value of \"{cvar.Signature}\". CVar is cheat protected.");

						return new(ExecutionResponseStatus.Success,$"\"{cvar.Signature}\" has been reset.");
					}

					return new(ExecutionResponseStatus.Failed, $"\"{rte.Signature}\" is not a CVar.");
				}

				return new(ExecutionResponseStatus.Failed, $"Unknown signature \"{signature}\".");
			}
		),
		Command(
			"list",
			"Lists all comands and CVars with an optional search term.",
			null,
			"list [term?]",
			false,
			(args) => {
				string term = string.Join(' ', args);
				var rtes = RegistryBrowser.FindExecutables(term, SearchMode.Contains, true);
				return FormatAndLogResults(rtes, term, "results");
			}
		),
		Command(
			"list_commands",
			"Lists all comands with an optional search term.",
			null,
			"list_commands [term?]",
			false,
			(args) => {
				string term = string.Join(' ', args);
				var rtes = RegistryBrowser.FindCommands(term, SearchMode.Contains, true);
				return FormatAndLogResults(rtes, term, "commands");
			}
		),
		Command(
			"list_cvars",
			"Lists all CVars with an optional search term.",
			null,
			"list_cvars [term?]",
			false,
			(args) => {
				string term = string.Join(' ', args);
				var rtes = RegistryBrowser.FindCVars(term, SearchMode.Contains, true);
				return FormatAndLogResults(rtes, term, "cvars");
			}
		),
	];

	// DRY code is nice code. This is basically just a router for all list commands.
	// Commands and CVars are both IRuntimeExecutables, so this is fine.
	Response<ExecutionResponseStatus> FormatAndLogResults(IRuntimeExecutable[] rtes, string term, string nounPlural)
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
