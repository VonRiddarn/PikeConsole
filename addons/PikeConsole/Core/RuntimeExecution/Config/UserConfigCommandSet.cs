using System.Collections.Generic;
using System.Text;
using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
using FractalPike.PikeConsole.Core.Utilities;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Config;

public partial class UserConfigCommandSet : CommandSet
{
	protected override string Prefix => "user";
	protected override Command[] InstantiateCommands() => [
		Command(
			Signature("create"),
			$"Create a new user config in {PikeConsoleSettings.UserConfigsDirectory}.",
			null,
			$"{Signature("create")} [config name] [select now (Bool)]",
			false,
			(args) => {
				if(!ArgumentParser.ValidateCount(args, 2, out string error))
					return new(ExecutionResponseStatus.InvalidArgs, error, [LogTags.InvalidArgs]);

				if(!ArgumentParser.TryParseBool(args[1], out bool b, out string _))
					return new(ExecutionResponseStatus.Failed, "PARSE ERROR", [LogTags.Failed]);

				var response = UserConfigManager.CreateConfig(args[0], b);

				ExecutionResponseStatus s = response.Status == ConfigResponseStatus.Success ? ExecutionResponseStatus.Success : ExecutionResponseStatus.Failed;
				return new(s, response.Message, response.Tags);
			}
		),
		Command(
			Signature("active"),
			"Displays the or switches the active config.",
			null,
			$"{Signature("active")} [config name?]",
			false,
			(args) => {
				if(args.Length < 1)
					return new(ExecutionResponseStatus.Success, UserConfigManager.ActiveConfig.DisplayName);

				var response = UserConfigManager.SelectConfig(args[0]);

				ExecutionResponseStatus s = response.Status == ConfigResponseStatus.Success ? ExecutionResponseStatus.Success : ExecutionResponseStatus.Failed;
				return new(s, response.Message, response.Tags);
			}
		),
		Command(
			Signature("find"),
			"Find one or more config of a certain name.",
			"Defaults to \"*\" which will return all available configs.",
			$"{Signature("find")} [search pattern?]",
			false,
			static (args) => {

				Dictionary<string, Response<ConfigResponseStatus, ConfigRef[]>> responseDict = [];

				if(args.Length < 1)
					responseDict.Add("*", UserConfigManager.GetAvailableConfigs());
				else
					foreach(string s in args)
						if(!responseDict.ContainsKey(s))
							responseDict.Add(s, UserConfigManager.GetAvailableConfigs($"{s}"));

				StringBuilder sb = new();

				foreach(string key in responseDict.Keys)
				{
					sb.Append($"Showing user configs matching query \"{key}\"...\n");
					foreach(ConfigRef cr in responseDict[key].Payload)
						sb.AppendLine($"\t{cr.DisplayName}");
				}
				PikeLogger.Log(LogTarget.Runtime, $"{sb.ToString()}", forceLog: true);
				return new(ExecutionResponseStatus.Success, null);
			}
		),
	];

}
