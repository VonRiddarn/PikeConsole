using System.IO;
using System.Linq;
using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.Utilities;
using Godot;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Config;

public static class ConfigIO
{

	public static bool ExecuteFromFile(ExecutionSource source, string localPath)
	{
		if (string.IsNullOrEmpty(localPath))
			return false;

		if (!localPath.EndsWith(".ecfg"))
			localPath += ".ecfg";

		string[] lines = ReadConfig(localPath, true);
		if (lines.Length < 1)
			return false;

		foreach (string line in lines)
		{
			StatementExecutor.Execute(source, StatementParser.ParseLine(line));
		}

		return true;
	}

	public static string[] ReadConfig(string localPath, bool trimWhiteSpaceAndComments = true)
	{
		if (string.IsNullOrWhiteSpace(localPath))
			return [];

		if (!localPath.EndsWith(".ecfg"))
			localPath += ".ecfg";

		string path = ProjectSettings.GlobalizePath($"{PikeConsoleConfig.ConfigDirectory}/{localPath}");
		if (!File.Exists(path))
			return [];

		try
		{
			string[] lines = File.ReadAllLines(path);

			return !trimWhiteSpaceAndComments ? lines :
					[.. lines
					.Select(l => l.Trim())
					.Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("//"))];
		}
		catch (IOException e)
		{
			PikeLogger.LogError(LogTarget.All, $"Failed to read config file \"{localPath}\": {e.Message}", forceLog: true);
			return [];
		}
	}

	public static string[] GetConfigs(string localPath)
	{
		string globalPath = FileSystemHelper.GetGlobalPath(localPath);

		if (!Directory.Exists(globalPath))
			return [];

		return [.. Directory.GetFiles(globalPath, "*.ecfg").Select(Path.GetFileName)];
	}

}
