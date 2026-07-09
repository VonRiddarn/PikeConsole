using System.Collections.Generic;
using System.Text;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Config;

public partial class UserConfigCommandSet : CommandSet
{
	const string PREFIX = "userconfig";
	protected override Command[] InstantiateCommands() => [
		Command(
			$"{PREFIX}_find",
			"Displays a list of all available user configs.",
			"Allows searching using wildcards, like \"sa*\" -> [\"Sam\", \"Sara\"]",
			$"{PREFIX}_find [searchTerm?]",
			false,
			static (args) => {

				List<string> configs = [];
				string message = string.Empty;

				if(args.Length < 1)
				{
					configs.AddRange(UserConfigManager.GetAvailableConfigs());
					message = "Showing all available user configs...\n";
				}
				else
				{
					string term = args[0];
					configs.AddRange(UserConfigManager.GetAvailableConfigs(term));
					message = $"Showing user configs mathcing term {term}...\n";
				}

				StringBuilder sb = new(message);
				foreach(string configName in configs)
				{
					sb.Append($"\t{configName}");
				}

				PikeLogger.Log(LogTarget.Runtime, $"{sb.ToString()}");
				return new(ExecutionResponseStatus.Success, null);
			}
		)
	];

}
