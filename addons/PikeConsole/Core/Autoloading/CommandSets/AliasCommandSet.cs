using System.Text;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution;
using FractalPike.PikeConsole.Core.RuntimeExecution.Aliases;
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;

namespace FractalPike.PikeConsole.Core.Autoloading;

public partial class AliasCommandSet : CommandSet
{
	protected override Command[] InstantiateCommands() => [
		Command(
			"alias",
			"Add or replace an alias in the registry.",
			null,
			"alias [alias signature] \"[alias statement]\"",
			false,
			(args) => {
				if(!ArgumentParser.ValidateCount(args, 2, out string error))
					return new(ExecutionResponseStatus.InvalidArgs, error);

				// We're forcing a double qoutation syntax for better readability.
				// Previous system used string.join for subsequent arguments which made it hard to read and parse.
				var response = AliasRegistry.Register(args[0], args[1]);

				if(response.Status == RegisterAliasResponseStatus.Success || response.Status == RegisterAliasResponseStatus.Replaced)
					return new(ExecutionResponseStatus.Success, response.Message);

				return new(ExecutionResponseStatus.Failed, response.Message);
			}
		),
		Command(
			"alias_list",
			"Lists all aliases with an optional search term.",
			null,
			"alias_list [..term?]",
			false,
			(args) => {
				string term = string.Join(' ', args);
				var aliases = RegistryBrowser.FindAliases(term, SearchMode.Contains, true);

				if (aliases.Length < 1)
					return new(ExecutionResponseStatus.Success, string.IsNullOrWhiteSpace(term) ? $"No aliases found." : $"No aliases found matching \"{term}\".");

				string header = string.IsNullOrWhiteSpace(term) ? $"Showing all aliases..." : $"Showing aliases matching \"{term}\"...";

				StringBuilder sb = new(header);

				foreach (var alias in aliases)
					sb.Append($"\n\n[Alias] {alias.Signature}\n]\t{alias.Statement}");

				PikeLogger.Log(LogTarget.Runtime, $"{sb.ToString()}");

				return new(ExecutionResponseStatus.Success, null);
			}
		),
		Command(
			"whereis",
			"Lists all aliases with an optional search term.",
			null,
			"alias_list [..term?]",
			false,
			(args) => {
				var a = RegistryBrowser.FindExecutables(args[0], SearchMode.Exact, true)[0];
				if(a == null)
					return new(ExecutionResponseStatus.Success, "No result");

				return new(ExecutionResponseStatus.Success, a.SourceLocation);
			}
		),
	];

}
