using System.IO;
using Godot;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Config;

/// <summary>
/// Helper class for managing config files.
/// </summary>
public class ConfigRef
{
	public readonly string FullPath;
	public string FileName { get; }
	public string DisplayName { get; }
	public string Directory { get; }

	public ConfigRef(string path)
	{
		FullPath = Path.GetFullPath(Path.ChangeExtension(path, ".ecfg"));
		FileName = Path.GetFileName(FullPath);
		DisplayName = FileToDisplayName(FileName);
		Directory = Path.GetDirectoryName(FullPath);
	}

	/// <summary>
	/// Translates a display name to a filename.
	/// </summary>
	/// <remarks>
	/// All config files must be snake_cased. This method assumes all lowercase separated with underscores!
	/// </remarks>
	public static string DisplayToFileName(string displayName)
	{
		return Path.ChangeExtension(displayName.Replace(' ', '_').Trim().ToLower(), ".ecfg");
	}

	public static string FileToDisplayName(string fileName)
	{
		string name = Path.GetFileNameWithoutExtension(fileName);
		return name.Replace('_', ' ').Trim().Capitalize();
	}
}