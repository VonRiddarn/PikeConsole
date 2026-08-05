using System;
using System.Collections.Generic;
using System.Linq;
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
			[$"{Signature("create")} [config name]",
			$"{Signature("create")} [config name] [select now (Bool)]"],
			false,
			(args) => {
				if(!ArgumentParser.ValidateCount(args, [1, 2], out int count, out string error))
					return new(ExecutionResponseStatus.InvalidArgs, error, [LogTags.InvalidArgs]);

				bool select = true;
				if(count == 2)
				{
					if(!ArgumentParser.TryParseBool(args[1], out bool wantSelect, out error))
						return new(ExecutionResponseStatus.Failed, error, [LogTags.Failed]);
					select = wantSelect;
				}

				var response = UserConfigManager.CreateConfig(args[0], select);

				ExecutionResponseStatus s = response.Status == ConfigResponseStatus.Success ? ExecutionResponseStatus.Success : ExecutionResponseStatus.Failed;
				return new(s, response.Message, response.Tags);
			}
		),
		Command(
			Signature("remove"),
			$"Remove a user config from {PikeConsoleSettings.UserConfigsDirectory}.",
			null,
			[$"{Signature("remove")} [config name]"],
			false,
			(args) => {
				if(!ArgumentParser.ValidateCount(args, 1, out string error))
					return new(ExecutionResponseStatus.InvalidArgs, error, [LogTags.InvalidArgs]);

				var response = UserConfigManager.RemoveConfig(args[0]);

				ExecutionResponseStatus s = response.Status == ConfigResponseStatus.Success ? ExecutionResponseStatus.Success : ExecutionResponseStatus.Failed;
				return new(s, response.Message, response.Tags);
			}
		),
		Command(
			Signature("active"),
			"Displays or switches the active config.",
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
			"Defaults to all available configs.",
			[$"{Signature("find")}",
			$"{Signature("find")} [search pattern 1] [search pattern 2] ..."],
			false,
			static (args) => {

				Dictionary<string, Response<ConfigResponseStatus, ConfigRef[]>> responseDict = [];

				if(args.Length < 1)
					responseDict.Add("*", UserConfigManager.GetAvailableConfigs());
				else
					foreach(string s in args)
						if(!responseDict.ContainsKey(s))
							responseDict.Add(s, UserConfigManager.GetAvailableConfigs($"*{s}*"));

				StringBuilder sb = new();

				foreach(string key in responseDict.Keys)
				{
					if(key == "*")
						sb.Append($"Showing all available user configs...\n");
					else
						sb.Append($"Showing user configs matching \"{key}\"...\n");

					foreach(ConfigRef cr in responseDict[key].Payload)
						sb.AppendLine($"\t{cr.DisplayName}");
				}
				PikeLogger.Log(LogTarget.Runtime, $"{sb.ToString().Trim()}", forceLog: true);
				return new(ExecutionResponseStatus.Success, null);
			}
		),
		Command(
			Signature("peek"),
			"Peek at the contents of a user config file.",
			"Defaults to the active user config if no argument name is passed.",
			[$"{Signature("peek")}",
			$"{Signature("peek")} [config_name1] [config_name2] ..."],
			false,
			static (args) => {
				StringBuilder sb = new();

				if (args.Length == 0)
					ReadAndAppendConfig(sb, UserConfigManager.ActiveConfig);
				else
				{
					// Using a HashSet so that we don't printthe same file twice. 
					// basically just a list with unique values.
					HashSet<string> processedPaths = [];

					foreach (string term in args)
					{
						var response = UserConfigManager.GetAvailableConfigs(term);

						if (response.Status != ConfigResponseStatus.Success || response.Payload == null || response.Payload.Length == 0)
						{
							sb.AppendLine($"----- {term} does not exist. -----");
							continue;
						}

						foreach (ConfigRef cr in response.Payload)
						{
							if (processedPaths.Add(cr.FullPath))
								ReadAndAppendConfig(sb, cr);
						}
					}
				}

				PikeLogger.Log(LogTarget.Runtime, $"{sb.ToString().Trim()}", forceLog: true);

				return new(ExecutionResponseStatus.Success, null);
			}
		),
	];


	// Basically only used by the peek command.
	static void ReadAndAppendConfig(StringBuilder sb, ConfigRef cr)
	{
		var response = ConfigIO.ReadConfig(cr.FullPath);

		sb.AppendLine($"----- {cr.DisplayName} -----");

		if (response.Status == ConfigResponseStatus.Success)
		{
			var filtered = response.Payload
				.Where(static s => !string.IsNullOrWhiteSpace(s) && !s.AsSpan().TrimStart().StartsWith("//"))
				.ToArray();

			if (filtered.Length == 0)
				sb.AppendLine("\tFile is empty.");
			else
				foreach (string line in filtered)
					sb.AppendLine($"\t{line}");
		}
		else
			sb.AppendLine($"\tFailed to read file: {response.Message}");
	}
}
