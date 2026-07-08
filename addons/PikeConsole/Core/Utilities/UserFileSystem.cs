using System.IO;
using Godot;

namespace FractalPike.PikeConsole.Core.Utilities;

public static class UserFileSystem
{
	/// <summary>
	/// The full path to the user directory on the system.
	/// </summary>
	public static string UserDirectory => ProjectSettings.GlobalizePath("user://");

	/// <summary>
	/// Combines strings into a full system path within the user directory.
	/// </summary>
	/// <param name="segments">Segments leading towards the path to find. Eg: "cfg", "users" will locate the system path for user://cfg/users</param>
	/// <returns>The full system path to the directory</returns>
	public static string GetPath(params string[] segments)
		=> Path.Combine([UserDirectory, .. segments]);

	/// <summary>
	/// Ensures a directory exists at the desired location.
	/// </summary>
	/// <param name="path">Full path to directory</param>
	/// <returns>True if a directory was created, false if it already existed.</returns>
	public static bool EnsureDirectory(string path)
	{
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
			return true;
		}
		return false;
	}
}