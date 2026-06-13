using System;

namespace FractalPike.PikeConsole.Core.Logging;

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

/// <summary>
/// Debug = QA | Runtime = End user | Editor = Developer
/// </summary>
[Flags]
public enum LogTarget
{
	/// <summary>Blocked from RELEASE builds.</summary>
	Debug = 1,
	/// <summary>Shows in RELEASE and DEBUG builds.</summary>
	/// <remarks>Anything you want to show the end-user is tagged with this!</remarks>
	Runtime = 2,
	/// <summary>Only shows up in the editor output window.</summary>
	/// <remarks>Blocked AND stripped from builds.</remarks>
	Editor = 4,
	All = Debug | Runtime | Editor
};