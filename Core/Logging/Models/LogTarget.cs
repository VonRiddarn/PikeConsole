using System;

namespace FractalPike.PikeConsole.Core.Logging;

[Flags]
public enum LogTarget
{
	Debug = 1,
	/// <summary>
	/// Note: Debug builds will also have access to the release logs.
	/// This is expected, as debug builds are just "extended" release builds.
	/// </summary>
	Release = 2,
	/// <summary>
	/// "Editor" means the editor output dock during playtesting using the play button.
	/// If you want to show logs in the UI when in editor, use Debug.
	/// </summary>
	Editor = 4,
	AnyRuntime = Debug | Release,
	All = Debug | Release | Editor
};