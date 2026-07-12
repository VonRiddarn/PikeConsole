using System.IO;
using Godot;

namespace FractalPike.PikeConsole.Core.Utilities;

public static class FileSystemHelper
{

	public const string RAM_ONLY_FLAG = "ram_only";

	/// <summary>
	/// Static class that manages the User directory
	/// </summary>
	public static class UserDirectory
	{
		/// <summary>
		/// Combines strings into a full gloabl system path within the user directory.
		/// </summary>
		/// <returns>The full system path to the directory</returns>
		public static string Globalized(params string[] segments)
		{
			if (segments == null || segments.Length == 0)
				return Globalized(string.Empty);

			// Using GetFullPath instead of just returning path so that we make sure it's normalized.
			return Globalized(Path.Combine(segments));
		}

		/// <summary>
		/// Converts a local 
		/// </summary>
		/// <returns>The full system path to the directory</returns>
		public static string Globalized(string path)
		{
			if (path.StartsWith("user://"))
				path = path[7..];

			string usrDriGlobal = ProjectSettings.GlobalizePath("user://");

			path = Path.Combine(usrDriGlobal, path);

			// Using GetFullPath instead of just returning path so that we make sure it's normalized.
			return Path.GetFullPath(path);
		}
	}

	public static bool HasInvalidChars(string filename)
	{
		return filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0;
	}

	public static PathType GetPathType(string path)
	{
		if (path.StartsWith("user://"))
			return PathType.User;
		else if (path.StartsWith("res://"))
			return PathType.Resource;

		return PathType.Standard;
	}

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