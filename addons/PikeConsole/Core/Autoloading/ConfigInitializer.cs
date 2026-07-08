using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.Utilities;
using Godot;

namespace FractalPike.PikeConsole.Core.Autoloading;

public partial class ConfigInitializer : Node
{

	[Export] PackedScene _userConfigUpdater;

	public override void _EnterTree()
	{
		InitializeDirectories();
	}

	static void InitializeDirectories()
	{
		string path = ProjectSettings.GlobalizePath(PikeConsoleConfig.ConfigDirectory);

		if (FileSystemHelper.EnsureDirectory(path))
			PikeLogger.Log(LogTarget.Editor, $"[PikeConsole] Config directory was missing. Created directory at: {path}");

		// Reusing path
		path = ProjectSettings.GlobalizePath(PikeConsoleConfig.UserConfigsDirectory);

		if (PikeConsoleConfig.UserConfigsEnabled && FileSystemHelper.EnsureDirectory(path))
			PikeLogger.Log(LogTarget.Editor, $"[PikeConsole] User configs directory was missing. Created directory at: {path}");
	}
}
