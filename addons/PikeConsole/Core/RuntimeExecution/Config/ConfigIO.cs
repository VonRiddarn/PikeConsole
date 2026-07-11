using System;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.Utilities;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Config;

/*
 * ARE YOU HERE BECAUSE YOU CANNOT ACCESS ANY CONFIGS INSIDE THE "res://" FOLDER AT RUNTIME?
 * 
 * Godot strips unrecognized files from the exported version of the game.
 * Thus you must allow the files in the export settings: 
 * 
 * Project > Export > [Target] > Resources tab
 * 
 * In the text box for "filters to export non-resource files/folders" add: *.ecfg
 * 
 * NOTE: All files in the compiled binary are immutable. You cannot write to them.
*/

public static class ConfigIO
{
	public const string EXT = ".ecfg";

	/// <summary>
	/// Execute all statements within an executable config file (.ecfg).
	/// </summary>
	/// <remarks>
	/// Automatically correctly routes "user://" and "res://".
	/// </remarks>
	/// <param name="source">Execution source. Used to contextually prevent cheating.</param>
	/// <param name="path">The path to the executable. This can be a full or relative path.</param>
	/// <returns></returns>
	public static Response<ConfigCRUDEResponseStatus> ExecuteFromConfig(ExecutionSource source, string path)
	{
		if (string.IsNullOrEmpty(path))
			return new(ConfigCRUDEResponseStatus.InvalidArgs, "Execution path is empty.");

		Response<ConfigCRUDEResponseStatus, string[]> fileResponse = FileSystemHelper.GetPathType(path) switch
		{
			PathType.Resource => ReadConfigResource(path),
			PathType.User => ReadConfig(FileSystemHelper.UserDirectory.Globalized(path.Replace("user://", string.Empty))),
			_ => ReadConfig(path),
		};

		// This is kind of mixing concerns, but if we don't do it here, each listener must manually apply flags.
		if (fileResponse.Status != ConfigCRUDEResponseStatus.Success)
			return new(fileResponse.Status, fileResponse.Message, fileResponse.Flags);

		// NOTE: At this point the lines should to 100% certainty be within the fileResponse payload (fileResponse.Payload)
		foreach (string line in fileResponse.Payload)
			StatementExecutor.Execute(source, StatementParser.ParseLine(line));

		return new(ConfigCRUDEResponseStatus.Success, null);
	}

	public static void WriteToConfig(string[] rows, string globalPath)
	{
		if (globalPath.EndsWith(EXT))
			globalPath = globalPath[..^EXT.Length];

		// Prepare paths for all files needed for a save.
		// This might look overkill, but if the game crashes during save we do not want to corrupt or lose a player file.
		string path = $"{globalPath}{EXT}";
		string tempPath = $"{globalPath}.tmp";

		// Actually apply the settings to real files.
		try
		{
			FileSystemHelper.EnsureDirectory(Path.GetDirectoryName(globalPath));

			// Make a temp file.
			File.WriteAllLines(tempPath, rows);

			// If a real file exist, safe-replace the real file with the temp.
			// If not, just rename the temp file.
			if (File.Exists(path))
				File.Replace(tempPath, path, null);
			else
				File.Move(tempPath, path);
		}
		catch (Exception e)
		{
			PikeLogger.LogError(LogTarget.All, $"Failed to save config \"{globalPath}\": {e.Message}", forceLog: true);
		}
		finally
		{
			if (File.Exists(tempPath))
				File.Delete(tempPath);
		}
	}

	public static bool TryRenameConfig(string newName, string globalPath, out string error)
	{
		error = string.Empty;

		if (!globalPath.EndsWith(EXT))
			globalPath += EXT;

		if (!newName.EndsWith(EXT))
			newName += EXT;

		string originalFile = Path.GetFileName(globalPath);
		string movePath = $"{Path.GetDirectoryName(globalPath)}/{newName}";

		if (File.Exists(movePath))
		{
			error = $"Cannot rename config file \"{originalFile}\" to \"{newName}\". A file with that name already exists!";
			return false;
		}

		try
		{
			File.Move(globalPath, movePath);
		}
		catch (Exception e)
		{
			PikeLogger.LogError(LogTarget.All, $"Failed to rename config file \"{originalFile}\": {e.Message}", forceLog: true);
		}

		return true;
	}

	public static bool RemoveConfig(string globalPath)
	{
		if (!globalPath.EndsWith(EXT))
			globalPath += EXT;

		// Actually apply the settings to real files.
		try
		{
			if (!File.Exists(globalPath))
				return false;

			File.Delete(globalPath);
			return true;
		}
		catch (Exception e)
		{
			PikeLogger.LogError(LogTarget.All, $"Failed to remove config \"{globalPath}\": {e.Message}", forceLog: true);
			return false;
		}
	}

	/// <summary>
	/// Try getting all lines from a config file. Returned inside the responses payload.
	/// </summary>
	/// <param name="source">Execution source. Used to contextually prevent cheating.</param>
	/// <param name="globalPath">The path, assuming the <c>user://{cfg}</c> directory as the root.</param>
	/// <returns>A payloaded response.</returns>
	public static Response<ConfigCRUDEResponseStatus, string[]> ReadConfig(string globalPath)
	{
		if (string.IsNullOrWhiteSpace(globalPath))
			return new(ConfigCRUDEResponseStatus.InvalidArgs, default, "Config path is empty.", [LogFlags.InvalidArgs]);

		globalPath = Path.ChangeExtension(globalPath, EXT);

		try
		{
			string[] lines = File.ReadAllLines(globalPath);
			return new(ConfigCRUDEResponseStatus.Success, lines, null);
		}
		catch (IOException e)
		{
			if (e is FileNotFoundException or DirectoryNotFoundException)
				return new(ConfigCRUDEResponseStatus.NotFound, default, $"Couldn't find config file at: \"{globalPath}\"", [LogFlags.NotFound]);
			else
				return new(ConfigCRUDEResponseStatus.Error, default, $"Failed to read config file \"{globalPath}\": {e.Message}");
		}
	}

	// Wrapper that reads a resource config (a config file within the compiled binary)
	static Response<ConfigCRUDEResponseStatus, string[]> ReadConfigResource(string resPath)
	{
		if (string.IsNullOrWhiteSpace(resPath.Replace("res://", string.Empty)))
			return new(ConfigCRUDEResponseStatus.InvalidArgs, default, "Resource config path is empty.", [LogFlags.InvalidArgs]);

		resPath = Path.ChangeExtension(resPath, EXT);

		if (!Godot.FileAccess.FileExists(resPath))
			return new(ConfigCRUDEResponseStatus.NotFound, default, $"Couldn't find resource config file at: \"{resPath}\"", [LogFlags.NotFound]);

		using Godot.FileAccess file = Godot.FileAccess.Open(resPath, Godot.FileAccess.ModeFlags.Read);

		if (file == null)
		{
			Godot.Error err = Godot.FileAccess.GetOpenError();
			return new(ConfigCRUDEResponseStatus.Error, default, $"Failed to open resource config file \"{resPath}\". Godot Error: {err}");
		}

		string content = file.GetAsText();
		string[] lines = content.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

		return new(ConfigCRUDEResponseStatus.Success, lines, null);
	}

	/// <summary>
	/// Uses a glob pattern to search for files. This glob can be any path, including res:// or user://
	/// </summary>
	/// <remarks>
	/// This method automatically adds or replaces the extention to match <c>EXT</c>. 
	/// That means sending something like <c>user://cfg/users/*</c> will automatically result in <c>user://cfg/users/*.ecfg</c>. 
	/// Likewise a bad path like <c>user://cfg/users/</c> will automatically result in <c>user://cfg/users/.ecfg</c>. 
	/// </remarks>
	/// <param name="searchPattern">Glob pattern</param>
	/// <returns></returns>
	public static Response<ConfigCRUDEResponseStatus, ConfigRef[]> GetConfigs(string searchPattern)
	{

		searchPattern = Path.ChangeExtension(searchPattern, EXT);
		PikeLogger.Log(LogTarget.Runtime, $"{searchPattern}");

		Response<ConfigCRUDEResponseStatus, ConfigRef[]> fileResponse = FileSystemHelper.GetPathType(searchPattern) switch
		{
			PathType.Resource => GetConfigResourcesInternal(searchPattern),
			PathType.User => GetConfigsInternal(FileSystemHelper.UserDirectory.Globalized(searchPattern.Replace("user://", string.Empty))),
			_ => GetConfigsInternal(searchPattern),
		};

		return fileResponse;
	}

	static Response<ConfigCRUDEResponseStatus, ConfigRef[]> GetConfigsInternal(string globalPath)
	{
		string dir = Path.GetDirectoryName(globalPath);
		string term = Path.GetFileName(globalPath);
		PikeLogger.Log(LogTarget.Runtime, $"\nDIR: {dir}\nTERM: {term}");

		if (!Directory.Exists(dir))
			return new(ConfigCRUDEResponseStatus.NotFound, [], $"Directory \"{dir}\" does not exist. Cannot search for files.", [LogFlags.NotFound]);

		try
		{
			ConfigRef[] configs = [.. Directory.GetFiles(dir, term).Select(f => new ConfigRef(f))];
			return new(ConfigCRUDEResponseStatus.Success, configs, null);
		}
		catch (Exception e)
		{
			return new(ConfigCRUDEResponseStatus.Error, [], $"Couldn't get the files from {dir} using {term}. Error: {e.Message}");
		}
	}

	static Response<ConfigCRUDEResponseStatus, ConfigRef[]> GetConfigResourcesInternal(string resSearchPattern)
	{
		// Note: Godot cannot search dynamically in compiled builds for files that weren't oficially imported, but we can reference them directly.
		// While this method is MIA in a release build, the Execution method works perfectly fine as long as the path is direct, like: "res://cfg/map_5.ecfg"
		// TOOD: Write a EditorImportPlugin to support dynamic browsing in release binary (low priority)
		// https://docs.godotengine.org/en/stable/classes/class_editorimportplugin.html

		// https://docs.godotengine.org/en/stable/tutorials/export/feature_tags.html
		if (!Godot.OS.HasFeature("editor"))
			return new(ConfigCRUDEResponseStatus.Failed, [], "Cannot dynamically search for internal config files in the compiled binary!", [LogFlags.Failed]);

		resSearchPattern = resSearchPattern.Replace("res://", string.Empty);
		string dir = Path.GetDirectoryName(resSearchPattern);
		string term = Path.GetFileName(resSearchPattern);

		// If we're on windows the Path method is translated to backslashes.
		// So we just normalize them back.
		dir = dir.Replace('\\', '/');

		if (!Godot.DirAccess.DirExistsAbsolute($"res://{dir}"))
			return new(ConfigCRUDEResponseStatus.NotFound, [], $"Resource directory \"{dir}\" does not exist.", [LogFlags.NotFound]);

		using Godot.DirAccess dirAccess = Godot.DirAccess.Open(dir);
		if (dirAccess == null)
			return new(ConfigCRUDEResponseStatus.Error, [], $"Failed to open resource directory \"{dir}\". Godot Error: {Godot.DirAccess.GetOpenError()}");

		string[] allFiles = dirAccess.GetFiles();

		// This basically does exactly the same as "Directory.GetFiles". This allows us to use wildcards etc.
		// Also, Godot returns just the filenames, not the full path, so we rebuild it. 
		// All in all this is heavier than the pure C# version, but it's the best I can do.
		ConfigRef[] configs = [.. allFiles
		.Where(fileName => FileSystemName.MatchesSimpleExpression(term, fileName))
		.Select(fileName => new ConfigRef($"{dir}/{fileName}"))];

		return new(ConfigCRUDEResponseStatus.Success, configs, null);
	}

}
