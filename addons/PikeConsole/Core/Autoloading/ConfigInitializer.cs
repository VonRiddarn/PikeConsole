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

		if (PikeConsoleSettings.UserConfigsEnabled && _userConfigUpdater == null)
		{
			PikeLogger.LogError(LogTarget.All, $"User config updater is not set in the Godot editor. Cannot initialize the Node!", forceLog: true);
			return;
		}

		Node userConfigUpdater = _userConfigUpdater.Instantiate();
		AddChild(userConfigUpdater);
	}

	static void InitializeDirectories()
	{
		string path = FileSystemHelper.UserDirectory.Globalized(PikeConsoleSettings.ConfigDirectory);

		if (FileSystemHelper.EnsureDirectory(path))
			PikeLogger.Log(LogTarget.Editor, $"[PikeConsole] Config directory was missing. Created directory at: {path}");

		// Reusing path
		path = FileSystemHelper.UserDirectory.Globalized(PikeConsoleSettings.UserConfigsDirectory);

		if (PikeConsoleSettings.UserConfigsEnabled && FileSystemHelper.EnsureDirectory(path))
			PikeLogger.Log(LogTarget.Editor, $"[PikeConsole] User configs directory was missing. Created directory at: {path}");
	}
}
