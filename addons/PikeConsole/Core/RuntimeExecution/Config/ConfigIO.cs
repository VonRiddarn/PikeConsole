using System;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
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
	public static Response<ConfigResponseStatus> ExecuteFromConfig(ExecutionSource source, string path, bool silent = false)
	{
		if (string.IsNullOrEmpty(path))
			return new(ConfigResponseStatus.InvalidArgs, "Execution path is empty.");

		Response<ConfigResponseStatus, string[]> fileResponse = FileSystemHelper.GetPathType(path) switch
		{
			PathType.Resource => ReadConfigResource(path),
			PathType.User => ReadConfig(FileSystemHelper.UserDirectory.Globalized(path.Replace("user://", string.Empty))),
			_ => ReadConfig(path),
		};

		// This is kind of mixing concerns, but if we don't do it here, each listener must manually apply flags.
		if (fileResponse.Status != ConfigResponseStatus.Success)
			return new(fileResponse.Status, fileResponse.Message, fileResponse.Tags);

		// NOTE: At this point the lines should to 100% certainty be within the fileResponse payload (fileResponse.Payload)
		foreach (string line in fileResponse.Payload)
			StatementExecutor.Execute(source, StatementParser.ParseLine(line), silent);

		return new(ConfigResponseStatus.Success, null);
	}

	/// <summary>
	/// Execute all statements within an executable config file (.ecfg).
	/// </summary>
	/// <remarks>
	/// Automatically correctly routes "user://" and "res://".
	/// </remarks>
	/// <param name="source">Execution source. Used to contextually prevent cheating.</param>
	/// <param name="path">The path to the config. This can be a full or relative path.</param>
	/// <returns></returns>
	public static Response<ConfigResponseStatus> WriteToConfig(string[] rows, string path, bool overwrite = false)
	{
		var pathType = FileSystemHelper.GetPathType(path);

		if (pathType == PathType.Resource) // Do not allow mutation (even in the editor build)
			return new(ConfigResponseStatus.Failed, "Config resources in the binary are immutable!", [LogTags.Failed]);
		else if (pathType == PathType.User) // Remove the "user://" previx and globalize the path
			path = FileSystemHelper.UserDirectory.Globalized(path.Replace("user://", string.Empty));

		// Prepare paths for all files needed for a save.
		// This might look overkill, but if the game crashes during save we do not want to corrupt or lose a player file.
		string file = Path.ChangeExtension(path, EXT);
		string tempFile = Path.ChangeExtension(path, ".tmp");

		try
		{
			FileSystemHelper.EnsureDirectory(Path.GetDirectoryName(path));

			if (File.Exists(file) && !overwrite)
				return new(ConfigResponseStatus.FileConflict, $"Cannot write to file {Path.GetFileName(path)}. The file already exists.", [LogTags.Conflict]);

			// Make a temp file.
			File.WriteAllLines(tempFile, rows);

			// If a real file exist, safe-replace the real file with the temp.
			// If not, just rename the temp file.
			if (File.Exists(file))
				File.Replace(tempFile, file, null);
			else
				File.Move(tempFile, file);
		}
		catch (Exception e)
		{
			return new(ConfigResponseStatus.Error, $"Failed to save config \"{path}\": {e.Message}");
		}
		finally
		{
			if (File.Exists(tempFile))
				File.Delete(tempFile);
		}

		return new(ConfigResponseStatus.Success, null);
	}

	/// <summary>
	/// Rename a config using global path or <c>"user://"</c> path.
	/// </summary>
	/// <param name="path">Global or <c>"user://"</c></param>
	public static Response<ConfigResponseStatus> RenameConfig(string newName, string path)
	{
		var pathType = FileSystemHelper.GetPathType(path);

		if (pathType == PathType.Resource) // Do not allow mutation (even in the editor build)
			return new(ConfigResponseStatus.Failed, "Config resources in the binary are immutable!", [LogTags.Failed]);
		else if (pathType == PathType.User) // Remove the "user://" previx and globalize the path
			path = FileSystemHelper.UserDirectory.Globalized(path.Replace("user://", string.Empty));

		string file = Path.ChangeExtension(path, EXT);
		string movePath = Path.ChangeExtension($"{Path.GetDirectoryName(path)}/{newName}", EXT);

		if (File.Exists(movePath))
			return new(ConfigResponseStatus.FileConflict, $"Cannot rename file to \"{Path.GetFileName(movePath)}\". That file already exists.", [LogTags.Conflict]);

		try
		{
			File.Move(file, movePath);
		}
		catch (Exception e)
		{
			return new(ConfigResponseStatus.Error, $"Failed to rename config \"{path}\": {e.Message}");
		}

		return new(ConfigResponseStatus.Success, null);
	}

	/// <summary>
	/// Remove a config using global path or <c>"user://"</c> path.
	/// </summary>
	/// <param name="path">Global or <c>"user://"</c></param>
	public static Response<ConfigResponseStatus> RemoveConfig(string path)
	{
		var pathType = FileSystemHelper.GetPathType(path);

		if (pathType == PathType.Resource) // Do not allow mutation (even in the editor build)
			return new(ConfigResponseStatus.Failed, "Config resources in the binary are immutable!", [LogTags.InvalidArgs]);
		else if (pathType == PathType.User) // Remove the "user://" previx and globalize the path
			path = FileSystemHelper.UserDirectory.Globalized(path.Replace("user://", string.Empty));

		// Actually apply the settings to real files.
		try
		{
			if (!File.Exists(path))
				return new(ConfigResponseStatus.NotFound, $"Couldn't find config file at \"{path}\"", [LogTags.NotFound]);

			File.Delete(path);
		}
		catch (Exception e)
		{
			return new(ConfigResponseStatus.NotFound, $"Failed to remove file at \"{path}\": {e.Message}");
		}

		return new(ConfigResponseStatus.Success, null);
	}

	/// <summary>
	/// Try getting all lines from a config file. Returned inside the responses payload.
	/// </summary>
	/// <param name="globalPath">The path, assuming the <c>user://{cfg}</c> directory as the root.</param>
	/// <returns>A payloaded response.</returns>
	public static Response<ConfigResponseStatus, string[]> ReadConfig(string globalPath)
	{
		if (string.IsNullOrWhiteSpace(globalPath))
			return new(ConfigResponseStatus.InvalidArgs, default, "Config path is empty.", [LogTags.InvalidArgs]);

		globalPath = Path.ChangeExtension(globalPath, EXT);

		try
		{
			string[] lines = File.ReadAllLines(globalPath);
			return new(ConfigResponseStatus.Success, lines, null);
		}
		catch (IOException e)
		{
			if (e is FileNotFoundException or DirectoryNotFoundException)
				return new(ConfigResponseStatus.NotFound, default, $"Couldn't find config file at: \"{globalPath}\"", [LogTags.NotFound]);
			else
				return new(ConfigResponseStatus.Error, default, $"Failed to read config file \"{globalPath}\": {e.Message}");
		}
	}

	// Wrapper that reads a resource config (a config file within the compiled binary)
	static Response<ConfigResponseStatus, string[]> ReadConfigResource(string resPath)
	{
		if (string.IsNullOrWhiteSpace(resPath.Replace("res://", string.Empty)))
			return new(ConfigResponseStatus.InvalidArgs, default, "Resource config path is empty.", [LogTags.InvalidArgs]);

		resPath = Path.ChangeExtension(resPath, EXT);

		if (!Godot.FileAccess.FileExists(resPath))
			return new(ConfigResponseStatus.NotFound, default, $"Couldn't find resource config file at: \"{resPath}\"", [LogTags.NotFound]);

		using Godot.FileAccess file = Godot.FileAccess.Open(resPath, Godot.FileAccess.ModeFlags.Read);

		if (file == null)
		{
			Godot.Error err = Godot.FileAccess.GetOpenError();
			return new(ConfigResponseStatus.Error, default, $"Failed to open resource config file \"{resPath}\". Godot Error: {err}");
		}

		string content = file.GetAsText();
		string[] lines = content.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

		return new(ConfigResponseStatus.Success, lines, null);
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
	public static Response<ConfigResponseStatus, ConfigRef[]> GetConfigs(string searchPattern)
	{

		searchPattern = Path.ChangeExtension(searchPattern, EXT);

		Response<ConfigResponseStatus, ConfigRef[]> fileResponse = FileSystemHelper.GetPathType(searchPattern) switch
		{
			PathType.Resource => GetConfigResourcesInternal(searchPattern),
			PathType.User => GetConfigsInternal(FileSystemHelper.UserDirectory.Globalized(searchPattern.Replace("user://", string.Empty))),
			_ => GetConfigsInternal(searchPattern),
		};

		return fileResponse;
	}

	static Response<ConfigResponseStatus, ConfigRef[]> GetConfigsInternal(string globalPath)
	{
		string dir = Path.GetDirectoryName(globalPath);
		string term = Path.GetFileName(globalPath);

		if (!Directory.Exists(dir))
			return new(ConfigResponseStatus.NotFound, [], $"Directory \"{dir}\" does not exist. Cannot search for files.", [LogTags.NotFound]);

		try
		{
			ConfigRef[] configs = [.. Directory.GetFiles(dir, term).Select(f => new ConfigRef(f))];


			return configs.Length > 0 ?
				new(ConfigResponseStatus.Success, configs, null)
				: new(ConfigResponseStatus.Success, configs, null, [LogTags.NotFound]);
		}
		catch (Exception e)
		{
			return new(ConfigResponseStatus.Error, [], $"Couldn't get the files from {dir} using {term}. Error: {e.Message}");
		}
	}

	static Response<ConfigResponseStatus, ConfigRef[]> GetConfigResourcesInternal(string resSearchPattern)
	{
		// Note: Godot cannot search dynamically in compiled builds for files that weren't oficially imported, but we can reference them directly.
		// While this method is MIA in a release build, the Execution method works perfectly fine as long as the path is direct, like: "res://cfg/map_5.ecfg"
		// TOOD: Write a EditorImportPlugin to support dynamic browsing in release binary (low priority)
		// https://docs.godotengine.org/en/stable/classes/class_editorimportplugin.html

		// https://docs.godotengine.org/en/stable/tutorials/export/feature_tags.html
		if (!Godot.OS.HasFeature("editor"))
			return new(ConfigResponseStatus.Failed, [], "Cannot dynamically search for internal config files in the compiled binary!", [LogTags.Failed]);

		resSearchPattern = resSearchPattern.Replace("res://", string.Empty);
		string dir = Path.GetDirectoryName(resSearchPattern);
		string term = Path.GetFileName(resSearchPattern);

		// If we're on windows the Path method is translated to backslashes.
		// So we just normalize them back.
		dir = dir.Replace('\\', '/');

		if (!Godot.DirAccess.DirExistsAbsolute($"res://{dir}"))
			return new(ConfigResponseStatus.NotFound, [], $"Resource directory \"{dir}\" does not exist.", [LogTags.NotFound]);

		using Godot.DirAccess dirAccess = Godot.DirAccess.Open(dir);
		if (dirAccess == null)
			return new(ConfigResponseStatus.Error, [], $"Failed to open resource directory \"{dir}\". Godot Error: {Godot.DirAccess.GetOpenError()}");

		string[] allFiles = dirAccess.GetFiles();

		// This basically does exactly the same as "Directory.GetFiles". This allows us to use wildcards etc.
		// Also, Godot returns just the filenames, not the full path, so we rebuild it. 
		// All in all this is heavier than the pure C# version, but it's the best I can do.
		ConfigRef[] configs = [.. allFiles
		.Where(fileName => FileSystemName.MatchesSimpleExpression(term, fileName))
		.Select(fileName => new ConfigRef($"{dir}/{fileName}"))];

		return configs.Length > 0 ?
			new(ConfigResponseStatus.Success, configs, null)
			: new(ConfigResponseStatus.Success, configs, null, [LogTags.NotFound]);
	}

}
