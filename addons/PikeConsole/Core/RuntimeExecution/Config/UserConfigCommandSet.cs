using System.Text;
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
using FractalPike.PikeConsole.Core.Utilities;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Config;

public partial class UserConfigCommandSet : CommandSet
{
	const string PREFIX = "profile";
	protected override Command[] InstantiateCommands() => [
		Command(
			"u_create",
			false,
			(args) => {
				if(!ArgumentParser.TryParseBool(args[1], out bool b, out string _))
					return new(ExecutionResponseStatus.Failed, "PARSE ERROR", [LogFlags.Failed]);

				var response = UserConfigManager.CreateConfig(args[0], b);

				ExecutionResponseStatus s = response.Status == ConfigResponseStatus.Success ? ExecutionResponseStatus.Success : ExecutionResponseStatus.Failed;
				return new(s, response.Message, response.Flags);
			}
		),
		Command(
			"u_active",
			false,
			(args) => {
				if(args.Length < 1)
					return new(ExecutionResponseStatus.Success, UserConfigManager.ActiveConfig.DisplayName);

				var response = UserConfigManager.SelectConfig(args[0]);

				ExecutionResponseStatus s = response.Status == ConfigResponseStatus.Success ? ExecutionResponseStatus.Success : ExecutionResponseStatus.Failed;
				return new(s, response.Message, response.Flags);
			}
		),
	];

}
