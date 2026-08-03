# ConfigIO

`public static class ConfigIO`  

**Inherits**: None  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution.Config`  

## Description

Backend class for managing the executable config system.  
Systems like the [`UserConfigManager`](UserConfigManager.md) rely heavily on this class.  

///tip | Make sure to include internal config files in the export!
Godot strips unrecognized files from the exported version of the game.  
Thus you must allow the files in the export settings: 
`Project` > `Export` > `(Target Env)` > `Resources`  

In the text box for "_filters to export non-resource files/folders_" add: `*.ecfg`

NOTE:  
All files in the compiled binary are immutable and thus read only.
///


## Constants  
| Scope | Type | Name |
|-------|--------|------|
| `public const` | `string` | [EXT](#ext) |

## Methods
| Scope | Return | Name |
|-------|--------|------|
| `public` | [`Response`](../../RuntimeExecution/Response_T.md) `<` [`ConfigResponseStatus>`](ConfigResponseStatus.md)`>` | [ExecuteFromConfig](#executefromconfig) |
| `public` | [`Response`](../../RuntimeExecution/Response_T.md) `<` [`ConfigResponseStatus>`](ConfigResponseStatus.md)`>` | [WriteToConfig](#writetoconfig) |
| `public` | [`Response`](../../RuntimeExecution/Response_T.md) `<` [`ConfigResponseStatus>`](ConfigResponseStatus.md)`>` | [RenameConfig](#renameconfig) |
| `public` | [`Response`](../../RuntimeExecution/Response_T.md) `<` [`ConfigResponseStatus>`](ConfigResponseStatus.md)`>` | [RemoveConfig](#removeconfig) |
| `public` | [`Response`](../../RuntimeExecution/Response_T.md) `<` [`ConfigResponseStatus`](ConfigResponseStatus.md), `string[] >` | [ReadConfig](#readconfig) |
| `public` | [`Response`](../../RuntimeExecution/Response_T.md) `<` [`ConfigResponseStatus`](ConfigResponseStatus.md), [`ConfigRef[]`](ConfigRef.md) `>` | [GetConfigs](#getconfigs) |

## Event Descriptions  

### ActiveConfigChanged

**Signature**: `public static ConfigRef ActiveConfig`  

**Description**:  
Invoked when the [`ActiveConfig`](#activeconfig) is changed using [`SelectConfig`](#selectconfig).  
The new config is passed in the delegate as a [`ConfigRef`](ConfigRef.md).

## Constant Descriptions  

### EXT
**Signature**: `public const string EXT`  
**Value**: `".ecfg"`  

**Description**:  
The file extention to use for executable config files.  


---

## Method Descriptions  

### ExecuteFromConfig
**Signature**: `public static Response<ConfigResponseStatus> ExecuteFromConfig(ExecutionSource source, string path, bool silent = false)`

/// details | Parameter details (Click to expand)  
[`ExecutionSource`](../ExecutionSource.md) : `source`
: Who is calling this config file.  
_Note that public facing configs `user://` almost never should use anything but standard._  
_Only invoke as system on files that come from the `res://` directory as that bypasses cheat detection._

`string` : `path`
: Path to the config file. Accepts absolute and relative (`user://`/ `res://`). 

`bool` : `silent`
: If set to true, success messages will not print to the runtime console.  
_Useful for when internal systems make changes or invokes events._ 

///

**Description**:  
Executes the executable config file by running it line by line through the [`StatementExecutor`](../StatementExecutor.md).

**Example(s)**:  
_Excerpt from `ConfigCommandSet.cs`._
```csharp
Command(
	"exec",
	// . . .
	static (args) => {
		var response = ConfigIO.ExecuteFromConfig(ExecutionSource.Standard, $"{PikeConsoleSettings.ConfigDirectory}/{args[0]}");

		if(response.Status != ConfigResponseStatus.Success)
			return new(ExecutionResponseStatus.Failed, response.Message, response.Tags);

		return new(ExecutionResponseStatus.Success, response.Message, response.Tags);
	}
),
```

**Returns**:  
A [response status](ConfigResponseStatus.md) informing if the operation was successfull or not.

---

### WriteToConfig
**Signature**: `public static Response<ConfigResponseStatus> WriteToConfig(string[] rows, string path, bool overwrite = false)`

/// details | Parameter details (Click to expand)  
`string[]` : `rows`
: An array of statements to write to the config file. Each statement will be one line. 

`string` : `path`
: Path to the config file. Accepts absolute and relative (`user://`). 
_Note: `res://` will not work as it is immutable._

`bool` : `overwrite`
: If set to true the method will ignore file conflicts and override the old file.
///

**Description**:  
Writes a set of statements to a file.

**Example(s)**:  
_Excerpt from `UserConfigManager.cs`._
```csharp
public static Response<ConfigResponseStatus> CreateAndSelectDefaultConfig()
{
	var defaultConfig = new ConfigRef(GetPath(DEFAULT_CONFIG_NAME));

	string[] rows = [
		"// ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ",
		"// THIS FILE IS VOLATILE AND CHANGES / ADDITIONS MAY BE OVERWRITTEN!",
		"// ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- "
	];

	if (!File.Exists(defaultConfig.FullPath))
		ConfigIO.WriteToConfig(rows, defaultConfig.FullPath, false);

	return SelectConfig(DEFAULT_CONFIG_NAME);
}
```

**Returns**:  
A [response status](ConfigResponseStatus.md) informing if the operation was successfull or not.

---

### RenameConfig
**Signature**: `public static Response<ConfigResponseStatus> RenameConfig(string newName, string path)`

/// details | Parameter details (Click to expand)  
`string` : `newName`
: The new desired name of the config. Must be a valid filename.

`string` : `path`
: Path to the config file. Accepts absolute and relative (`user://`). 
_Note: `res://` will not work as it is immutable._
///

**Description**:  
Renames a config file on disk.

**Example(s)**:  
_Excerpt from `UserConfigManager.cs`._  
_The method depicted is not the implementation of ConfigIO's RenameConfig. They simply share a name._
```csharp
public static Response<ConfigResponseStatus> RenameConfig(string configName, string newName)
{
	// . . .

	var response = ConfigIO.RenameConfig(newName, oldConfig.FullPath);

	if (response.Status == ConfigResponseStatus.Success && isActiveProfile)
		UpdateActiveConfigTracker(new ConfigRef(GetPath(newName)));

	return response;
}
```

**Returns**:  
A [response status](ConfigResponseStatus.md) informing if the operation was successfull or not.

---

### RemoveConfig
**Signature**: `public static Response<ConfigResponseStatus> RemoveConfig(string path)`

/// details | Parameter details (Click to expand)  
`string` : `path`
: Path to the config file. Accepts absolute and relative (`user://`). 
_Note: `res://` will not work as it is immutable._
///

**Description**:  
Removes a config file from disk.

**Example(s)**:  
_Excerpt from `UserConfigManager.cs`._  
_The method depicted is not the implementation of ConfigIO's RemoveConfig. They simply share a name._
```csharp
public static Response<ConfigResponseStatus> RemoveConfig(string configName)
{
	ConfigRef target = new(GetPath(configName));

	if (target.FileName == ActiveConfig.FileName)
		return new(ConfigResponseStatus.Failed, "Cannot delete the currently active profile.", [LogTags.Failed]);

	var response = ConfigIO.RemoveConfig(target.FullPath);

	if (response.Status == ConfigResponseStatus.Success)
		return new(ConfigResponseStatus.Success, $"Removed profile \"{target.DisplayName}\".");

	return response;
}
```

**Returns**:  
A [response status](ConfigResponseStatus.md) informing if the operation was successfull or not.

---

### ReadConfig
**Signature**: `public static Response<ConfigResponseStatus, string[]> ReadConfig(string path)`

/// details | Parameter details (Click to expand)  
`string` : `path`
: Path to the config file. Accepts absolute and relative (`user://` / `res://`).
///

**Description**:  
Reads all lines from a config and returns them as a Payload.  
Useful for debugging executable config files at runtime.

**Example(s)**:  
_Excerpt from `UserConfigCommandSet.cs`._  
```csharp
Command(
	Signature("peek"),
	// . . .
	static (args) => {
		StringBuilder sb = new();

		if (args.Length == 0)
			ReadAndAppendConfig(sb, UserConfigManager.ActiveConfig);
		else
		{
			// Using a HashSet so that we don't printthe same file twice. 
			// basically just a list with unique values.
			HashSet<string> processedPaths = [];

			foreach (string term in args)
			{
				var response = UserConfigManager.GetAvailableConfigs(term);

				if (response.Status != ConfigResponseStatus.Success || response.Payload == null || response.Payload.Length == 0)
				{
					sb.AppendLine($"----- {term} does not exist. -----");
					continue;
				}

				foreach (ConfigRef cr in response.Payload)
				{
					if (processedPaths.Add(cr.FullPath))
						ReadAndAppendConfig(sb, cr);
				}
			}
		}

		PikeLogger.Log(LogTarget.Runtime, $"{sb.ToString().Trim()}", forceLog: true);

		return new(ExecutionResponseStatus.Success, null);
	}
),
```

**Returns**:  
A [response status](ConfigResponseStatus.md) informing if the operation was successfull or not and a [payload](../Response_T.md) containing the rows as `string[]`.

---

### GetConfigs
**Signature**: `public static Response<ConfigResponseStatus, ConfigRef[]> GetConfigs(string searchPattern)`

/// details | Parameter details (Click to expand)  
`string` : `searchPattern`
: The search pattern to use when searching the config files. Uses default [.NET search patterns](https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.getfiles?view=net-10.0#remarks).

| Example term | Result |
| ------------ | ------ |
| `*` | All |  
| `*son` | All ending with "_son_" |  
| `*test*` | All containing "_test_" anywhere in the string | 
///

**Description**:  
Search for config files at certain directories.  
Accepts absolute and relative (`user://` / `res://`) paths for the pattern.  

///warning
The `res://` folder does not index unknown resources, such as the `.ecfg` files.  
Thus you can only dynamically search for these when running the game from the Godot editor. In runtime builds no results will be returned.
///

**Example(s)**:  
_Excerpt from `UserConfigManager.cs`._  
```csharp
public static Response<ConfigResponseStatus, ConfigRef[]> GetAvailableConfigs(string term = "*")
{
	// "*" becomes something like c:/.../users/*.ecfg
	// "Tompa Tjompa" becomes something like: c:/.../users/tompa_tjompa.ecfg
	return ConfigIO.GetConfigs(GetPath(term));
}
```

**Returns**:  
A [response status](ConfigResponseStatus.md) informing if the operation was successfull or not and a [payload](../Response_T.md) containing the found configs as [`ConfigRef[]`](ConfigRef.md).

---