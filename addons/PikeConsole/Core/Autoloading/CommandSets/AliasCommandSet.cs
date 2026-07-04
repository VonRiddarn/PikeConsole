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
		)
	];

}
