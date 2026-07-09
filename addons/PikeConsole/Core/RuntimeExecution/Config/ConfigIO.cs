using System;
using System.IO;
using System.Linq;
using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.Utilities;

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

	public static void WriteToConfig(string[] rows, string globalPath)
	{
		if (globalPath.EndsWith(".ecfg"))
			globalPath = globalPath[..^5];

		// Prepare paths for all files needed for a save.
		// This might look overkill, but if the game crashes during save we do not want to corrupt or lose a player file.
		string path = $"{globalPath}.ecfg";
		string tempPath = $"{globalPath}.tmp";

		// Actually apply the settings to real files.
		try
		{
			FileSystemHelper.EnsureDirectory(Path.GetDirectoryName(globalPath));

			// Make a temp file.
			File.WriteAllLines(tempPath, rows);

			// If a real file exist, safe-replace the real file with the temp.
			// If not, just rename the temp file.
			if (File.Exists(path))
				File.Replace(tempPath, path, null);
			else
				File.Move(tempPath, path);
		}
		catch (Exception e)
		{
			PikeLogger.LogError(LogTarget.All, $"Failed to save config \"{globalPath}\": {e.Message}", forceLog: true);
		}
		finally
		{
			if (File.Exists(tempPath))
				File.Delete(tempPath);
		}
	}

	public static bool RemoveConfig(string globalPath)
	{
		if (!globalPath.EndsWith(".ecfg"))
			globalPath += ".ecfg";

		// Actually apply the settings to real files.
		try
		{
			if (!File.Exists(globalPath))
				return false;

			File.Delete(globalPath);
			return true;
		}
		catch (Exception e)
		{
			PikeLogger.LogError(LogTarget.All, $"Failed to remove config \"{globalPath}\": {e.Message}", forceLog: true);
			return false;
		}
	}

	public static string[] ReadConfig(string localPath, bool trimWhiteSpaceAndComments = true)
	{
		if (string.IsNullOrWhiteSpace(localPath))
			return [];

		if (!localPath.EndsWith(".ecfg"))
			localPath += ".ecfg";

		string path = FileSystemHelper.UserDirectory.Global(PikeConsoleConfig.ConfigDirectory, localPath);

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
			if (e is FileNotFoundException or DirectoryNotFoundException)
				PikeLogger.LogWarning(LogTarget.All, $"Failed to read find config file at: \"{localPath}\"", forceLog: true, tags: [RuntimeExecutionLogTags.Failed]);
			else
				PikeLogger.LogError(LogTarget.All, $"Failed to read config file \"{localPath}\": {e.Message}", forceLog: true);
			return [];
		}
	}

	public static string[] GetConfigs(string localPath, string term)
	{
		if (term.EndsWith(".ecfg"))
			term = term[..^5];

		if (string.IsNullOrWhiteSpace(term))
			term = "*";

		string globalPath = FileSystemHelper.UserDirectory.Global(localPath);

		if (!Directory.Exists(globalPath))
			return [];

		return [.. Directory.GetFiles(globalPath, $"{term}.ecfg").Select(Path.GetFileName)];
	}

}
