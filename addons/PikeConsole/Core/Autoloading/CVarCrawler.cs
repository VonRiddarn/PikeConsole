using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution;
using Godot;

namespace FractalPike.PikeConsole.Core.Autoloading;

/*
    AUTHORS NOTE:
        The previous system relied on super hacky fixes and code smells due to the CVars generic nature.
		I updated and removed all of that so that it is as clean as possible!
		The recursion logic is more or less the same as the one used in Unity but with tighter semantics.

        Full transparency: AI was used to assist in translating the old Unity version into here.
        I have gone through the code and made sure I can put my name on it. I also added comments so that it is readable. 
*/

public partial class CVarCrawler : Node
{
	public override void _EnterTree() => CrawlForCVars(PikeConsoleConfig.CvarDirectory);

	private void CrawlForCVars(string currentPath)
	{
		using DirAccess dir = DirAccess.Open(currentPath);

		if (dir == null)
		{
			PikeLogger.LogError(LogTarget.All, $"Could not open directory at {currentPath}. Some CVars might not have been loaded!");
			return;
		}

		// Recursive magic...
		// Dive into all direct subdirectories (&>) where the same process is repeated.
		// If we have subdirectories, we will freeze until they are all scanned.
		foreach (string dirName in dir.GetDirectories())
		{
			// Skip hidden folders, like ".godot" etc for performance.
			if (dirName.StartsWith("."))
				continue;

			// Recursion!
			CrawlForCVars($"{currentPath}/{dirName}");
		}

		// This code is reached when there are no folders to recursively run through.
		// That either means we are at the bottom, or that we have looped through all previous folders.
		foreach (string fileName in dir.GetFiles())
		{
			// Apparently resource files get another extention in builds because they are converted to binary. (something.tres.remap)
			// We just strip this extension if it exists.
			string cleanFileName = fileName.Replace(".remap", "");

			// Make sure it's a resource file...
			if (cleanFileName.EndsWith(".tres") || cleanFileName.EndsWith(".res"))
			{
				string fullPath = $"{currentPath}/{cleanFileName}";

				// Load the resource using the clean path (Godot handles the remap internally)
				Resource loadedResource = ResourceLoader.Load(fullPath);

				// BIG CHANGE FROM UNITY FRAMEWORK!
				// We add the initialize method to the interface and use that to initialize.
				// ... instead of CVarInternal<int>, CVarInternal<bool>...
				if (loadedResource is ICVar cvar)
				{
					cvar.Initialize();
				}
			}
		}
	}
}
