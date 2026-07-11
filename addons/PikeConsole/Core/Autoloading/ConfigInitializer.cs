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

		if (PikeConsoleConfig.UserConfigsEnabled && _userConfigUpdater == null)
		{
			PikeLogger.LogError(LogTarget.All, $"User config updater is not set in the Godot editor. Cannot initialize the Node!", forceLog: true);
			return;
		}

		Node ucfgu = _userConfigUpdater.Instantiate();
		AddChild(ucfgu);
	}

	static void InitializeDirectories()
	{
		string path = FileSystemHelper.UserDirectory.Globalized(PikeConsoleConfig.ConfigDirectory);

		if (FileSystemHelper.EnsureDirectory(path))
			PikeLogger.Log(LogTarget.Editor, $"[PikeConsole] Config directory was missing. Created directory at: {path}");

		// Reusing path
		path = FileSystemHelper.UserDirectory.Globalized(PikeConsoleConfig.UserConfigsDirectory);

		if (PikeConsoleConfig.UserConfigsEnabled && FileSystemHelper.EnsureDirectory(path))
			PikeLogger.Log(LogTarget.Editor, $"[PikeConsole] User configs directory was missing. Created directory at: {path}");
	}
}
