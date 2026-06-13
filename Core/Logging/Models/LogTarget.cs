using System;

namespace FractalPike.PikeConsole.Core.Logging;

[Flags]
public enum LogTarget
{
	Debug = 1,
	/// <summary>
	/// Note: Debug builds will also have access to the runtime logs.
	/// This is expected, as debug builds are also runtime builds.
	/// </summary>
	/// <remarks>
	/// If you want to block debug logs from the release runtime, use the "Debug" flag!
	/// </remarks>
	Runtime = 2,
	/// <summary>
	/// "Editor" means the editor output dock during playtesting using the play button.
	/// If you want to show logs in the UI when in editor, use Debug.
	/// </summary>
	Editor = 4,
	All = Debug | Runtime | Editor
};


/*  
	In plain text:

		Playtesting in editor:
			UI: 	Debug, Runtime
			Output:	Editor
		
		Compiled DEBUG build:
			UI:		Debug, Runtime
			Output:	-
		
		Compiled RELEASE build (final game):
			UI:		Runtime
			Output:	-
*/