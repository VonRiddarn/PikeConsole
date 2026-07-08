using System.IO;
using System.Linq;
using FractalPike.PikeConsole.Core.Utilities;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Config;

public static class ConfigIO
{
	public static string[] GetConfigs(string localPath)
	{
		string globalPath = FileSystemHelper.GetGlobalPath(localPath);

		if (!Directory.Exists(globalPath))
			return [];

		return [.. Directory.GetFiles(globalPath, "*.ecfg").Select(Path.GetFileName)];
	}

}
