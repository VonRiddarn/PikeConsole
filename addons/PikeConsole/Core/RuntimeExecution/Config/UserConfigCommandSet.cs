using System.Text;
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Config;

public partial class UserConfigCommandSet : CommandSet
{
	const string PREFIX = "userconfig";
	protected override Command[] InstantiateCommands() => [
		Command(
			"cfg",
			false,
			(args) => {
				var response = ConfigIO.GetConfigs(args[0]);

				if(response.Status != ConfigResponseStatus.Success)
					return new(ExecutionResponseStatus.Failed, response.Message, response.Flags);

				StringBuilder sb = new();
				foreach(ConfigRef c in response.Payload)
				{
					sb.AppendLine(c.FileName);
				}

				return new(ExecutionResponseStatus.Success, sb.ToString(), response.Flags);
			}
		),
	];

}
