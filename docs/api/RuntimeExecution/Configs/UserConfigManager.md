# UserConfigManager

///note | Important notice
In order to use the User configuration system you first need to enable it through the project settings.  

To do so, go to:  
`Project` > `Project settings (General)` > `Fractasl Pike` > `Pike Console`  
And enable `Use user configs`.
///

`public static class UserConfigManager`  

**Inherits**: None  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution.Config`  

## Description

Root class for managing anything related to the user config system.  
Interacting with this class is crucial to provide multiple config support in the UI.  

///note
Even if UI support is not added, PikeConsole ships with premade commands that allows QA testers, developers and power users to still access the features using commands like `user_create` and `user_active`.
///

## Events  
| Scope | Delegate | Name |
|-------|--------|------|
| `public` | `Action <`[`ConfigRef`](ConfigRef.md)`>` | [ActiveConfigChanged](#activeconfigchanged) |

## Properties  
| Scope | Delegate | Name |
|-------|--------|------|
| `public` | [`ConfigRef`](ConfigRef.md) | [ActiveConfig](#activeconfig) |

## Methods
| Scope | Return | Name |
|-------|--------|------|
| `public` | [`Response`](../../RuntimeExecution/Response_T.md) `<` [`ConfigResponseStatus>`](ConfigResponseStatus.md)`>` | [CreateAndSelectDefaultConfig](#createandselectdefaultconfig) |
| `public` | [`Response`](../../RuntimeExecution/Response_T.md) `<` [`ConfigResponseStatus`](ConfigResponseStatus.md)`>` | [CreateConfig](#createconfig) |
| `public` | [`Response`](../../RuntimeExecution/Response_T.md) `<` [`ConfigResponseStatus>`](ConfigResponseStatus.md)`>` | [GetAvailableConfigs](#getavailableconfigs) |
| `public` | [`Response`](../../RuntimeExecution/Response_T.md) `<` [`ConfigResponseStatus`](ConfigResponseStatus.md), [`ConfigRef[]`](ConfigRef.md)`>` | [CreateConfig](#createconfig) |
| `public` | [`Response`](../../RuntimeExecution/Response_T.md) `<` [`ConfigResponseStatus>`](ConfigResponseStatus.md)`>` | [RenameConfig](#renameconfig) |
| `public` | [`Response`](../../RuntimeExecution/Response_T.md) `<` [`ConfigResponseStatus>`](ConfigResponseStatus.md)`>` | [SaveCurrentConfig](#savecurrentconfig) |
| `public` | [`Response`](../../RuntimeExecution/Response_T.md) `<` [`ConfigResponseStatus>`](ConfigResponseStatus.md)`>` | [SaveConfig](#saveconfig) |
| `public` | [`Response`](../../RuntimeExecution/Response_T.md) `<` [`ConfigResponseStatus>`](ConfigResponseStatus.md)`>` | [FullResetCurrentConfig](#fullresetcurrentconfig) |
| `public` | [`Response`](../../RuntimeExecution/Response_T.md) `<` [`ConfigResponseStatus>`](ConfigResponseStatus.md)`>` | [SelectConfig](#selectconfig) |
| `public` | [`Response`](../../RuntimeExecution/Response_T.md) `<` [`ConfigResponseStatus>`](ConfigResponseStatus.md)`>` | [RemoveConfig](#removeconfig) |

## Event Descriptions  

### ActiveConfigChanged

**Signature**: `public static ConfigRef ActiveConfig`  

**Description**:  
Invoked when the [`ActiveConfig`](#activeconfig) is changed using [`SelectConfig`](#selectconfig).  
The new config is passed in the delegate as a [`ConfigRef`](ConfigRef.md).

## Property Descriptions  

### ActiveConfig
**Signature**: `public static event Action<ConfigRef> ActiveConfigChanged`

**Description**:  
Gets a [`ConfigRef`](ConfigRef.md) of the currently selected config.  
If there is an error loading the current config, or there is no current config, it will fall back to the default config.

---

## Method Descriptions  

### CreateAndSelectDefaultConfig
**Signature**: `public static Response<ConfigResponseStatus> CreateAndSelectDefaultConfig()`

/// Note | No Parameters  
///

**Description**:  
Creates the default config file and selects it in the current config tracker.  
This method is used when needing to fallback on the default config.

**Example(s)**:  
_Excerpt from `UserConfigUpdater.cs`._
```csharp
var response = UserConfigManager.SelectConfig(UserConfigManager.ActiveConfig.FileName);

if (response.Status != ConfigResponseStatus.Success)
	UserConfigManager.CreateAndSelectDefaultConfig();
```

**Returns**:  
A [response status](ConfigResponseStatus.md) informing if the operation was successfull or not.

---

### CreateConfig
**Signature**: `public static Response<ConfigResponseStatus> CreateConfig(string configName, bool selectOnCreate = true)`

/// details | Parameter details (Click to expand)  
`string` : `configName`
: The desired name of the new config.  
_Note: This can be both a displayname or filename. The backend automatically manages parsing using spaces, and correct application of `.efcg`._
**Examples:**  
`Mr Timmy` = Valid  
`mr_timmy.ecfg` = Valid  

`bool` : `selectOnCreate`
: If set to true, the new config will automatically be selected upon creation and trigger [`ActiveConfigChanged`](#activeconfigchanged).   

///

**Description**:  
Creates a new user config.  

**Example(s)**:  
_Excerpt from `UserConfigCommandSet.cs`._
```csharp
Command(
	Signature("create"),
	// . . .
	(args) => {
		// . . .
		var response = UserConfigManager.CreateConfig(args[0], select);

		ExecutionResponseStatus s = response.Status == ConfigResponseStatus.Success ? ExecutionResponseStatus.Success : ExecutionResponseStatus.Failed;
		return new(s, response.Message, response.Tags);
	}
),
```

**Returns**:  
A [response status](ConfigResponseStatus.md) informing if the operation was successfull or not.  

///tip
The most common fail status when creating a config is `FileConflict`. This simply means there is already a config with that name already present.  

Always check the response's `Message` property when debugging.
///

---

### GetAvailableConfigs
**Signature**: `public static Response<ConfigResponseStatus, ConfigRef[]> GetAvailableConfigs(string term = "*")`

/// details | Parameter details (Click to expand)  
`string` : `term`
: The search pattern to use when searching the config files. Uses default [.NET search patterns](https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.getfiles?view=net-10.0#remarks) in the backend.

| Example term | Result |
| ------------ | ------ |
| `*` | All |  
| `*son` | All ending with "_son_" |  
| `*test*` | All containing "_test_" anywhere in the string | 

///

**Description**:  
Finds all user config files matching the search term.  
`term` defaults to `*` which returns all user configs.  

**Example(s)**:  
_Fictional example where we fetch all configs to display in a dropdown UI._
```csharp

var response = UserConfigManager.GetAvailableConfigs();

if(response.Status != ConfigResponseStatus.Success)
	return;

ConfigRef[] foundConfigs = response.Payload;

UpdateDropDown(foundConfigs);
```

_Excerpt from `UserConfigCommandSet.cs`._  

/// note
This exerpt is made to accept several arguments at once, making the code a little hard to read.  
The fetching of available configs is a lot easier than it may appear in this script (as demonstrated above).
///

```csharp
Command(
	Signature("find"),
	// . . .
	static (args) => {

		Dictionary<string, Response<ConfigResponseStatus, ConfigRef[]>> responseDict = [];

		if(args.Length < 1)
			responseDict.Add("*", UserConfigManager.GetAvailableConfigs());
		else
			foreach(string s in args)
				if(!responseDict.ContainsKey(s))
					responseDict.Add(s, UserConfigManager.GetAvailableConfigs($"*{s}*"));

		StringBuilder sb = new();

		foreach(string key in responseDict.Keys)
		{
			if(key == "*")
				sb.Append($"Showing all available user configs...\n");
			else
				sb.Append($"Showing user configs matching \"{key}\"...\n");
			
			foreach(ConfigRef cr in responseDict[key].Payload)
				sb.AppendLine($"\t{cr.DisplayName}");
		}
		PikeLogger.Log(LogTarget.Runtime, $"{sb.ToString().Trim()}", forceLog: true);
		return new(ExecutionResponseStatus.Success, null);
	}
),
```


**Returns**:  
A [response status](ConfigResponseStatus.md) informing if the operation was successfull or not and a payload containing the results as [`ConfigRef[]`](ConfigRef.md).

---

### RenameConfig
**Signature**: `public static Response<ConfigResponseStatus> RenameConfig(string configName, string newName)`

/// details | Parameter details (Click to expand)  
`string` : `configName`
: The name of the config to rename.  
_Note: This can be both a displayname or filename. The backend automatically manages parsing using spaces, and correct application of `.efcg`._

`string` : `newName`
: The desired new name of the config.  
_Note: This can be both a displayname or filename. The backend automatically manages parsing using spaces, and correct application of `.efcg`._

///

**Description**:  
Renames a config if the new name is valid and does not collide with other existing files.  

**Example(s)**:  
_Fictional example where the player can rename the current config through the UI._
```csharp
void OnInputSubmitted(string desiredName)
{
	string currentname = UserConfigManager.ActiveConfig.FileName;
	var response = RenameConfig(currentName, newName);

	if(response != ConfigResponseStatus.Success)
	{
		NotificationSystem.PromptError(response.Message);
		return;
	}
}
```


**Returns**:  
A [response status](ConfigResponseStatus.md) informing if the operation was successfull or not and a payload containing the results as [`ConfigRef[]`](ConfigRef.md).  

///tip
The most common fail statuses when renaming a config are `FileConflict` when the new name is already taken by another config, and `InvalidArgs` when the new name [contains invalid characters](https://learn.microsoft.com/en-us/dotnet/api/system.io.path.getinvalidfilenamechars?view=net-10.0).  

Always check the response's `Message` property when debugging.
///

---

### SaveCurrentConfig
**Signature**: `public static Response<ConfigResponseStatus> SaveCurrentConfig()`

/// note | No Parameters  
///

**Description**:  
Shorthand for calling [`SaveConfig`](#saveconfig) with the current config as an argument.  

Behind the scenes, this is the full implementation of the method:  
```csharp
public static Response<ConfigResponseStatus> SaveCurrentConfig() 
	=> SaveConfig(ActiveConfig.DisplayName);
```

**Example(s)**:  
_No examples made for this method._


**Returns**:  
A [response status](ConfigResponseStatus.md) informing if the operation was successfull or not.  

---

### SaveConfig
**Signature**: `public static Response<ConfigResponseStatus> SaveConfig(string configName)`

/// details | Parameter details (Click to expand)  
`string` : `configName`
: The name of the config to save.  
_Note: This can be both a displayname or filename. The backend automatically manages parsing using spaces, and correct application of `.efcg`._
///

**Description**:  
Iterates through all persistent CVars in the [`PersistentCVarRegistry`](../../CVars/PersistentCVarRegistry.md) and saves all changed values to the selected config.  

**Example(s)**:  
_Fictional scenario where the user presses "Save" after selecting settings in the UI._  

/// note
This scenario also assumes the settings were applied using `ramOnly` for preview / batching purposes.  
Applying a setting without `ramOnly` will automatically trigger a save.
///

```csharp
void SaveSettings()
{
	var response = UserConfigManager.SaveConfig(active.FileName);

	// If successful, early return.
	if(response.Status == ConfigResponseStatus.Success)
		return;
	
	// If unsuccessful, log an error to the console (or implement a notification system).
	if (response.Status != ConfigResponseStatus.Error)
		PikeLogger.LogWarning(LogTarget.Runtime, $"{response.Message}", forceLog: true, tags: response.Tags);
	else
		PikeLogger.LogError(LogTarget.All, $"{response.Message}", forceLog: true, tags: response.Tags);
	
}
```

_Excerpt from `UserConfigManager.cs`._  
```csharp

// This is the default debounced saving system.  
// If lots of variables are changed at once, 
// the system waits before saving to a file.

async void OnCVarChanged(ICVar _)
{
	_debounceCts?.Cancel();

	_debounceCts = new();
	var tempToken = _debounceCts.Token;

	try
	{
		await Task.Delay(DEBOUNCE_MS, tempToken);

		var active = UserConfigManager.ActiveConfig;
		var response = UserConfigManager.SaveConfig(active.FileName);

		if (response.Status == ConfigResponseStatus.Success && LogOnSave != null && LogOnSave.Value)
			PikeLogger.LogSuccess(LogTarget.Runtime, $"config \"{active.DisplayName}\" has been saved.", forceLog: true);
		else if (response.Status != ConfigResponseStatus.Error)
			PikeLogger.LogWarning(LogTarget.Runtime, $"{response.Message}", forceLog: true, tags: response.Tags);
		else
			PikeLogger.LogError(LogTarget.All, $"{response.Message}", forceLog: true, tags: response.Tags);
	}
	catch (TaskCanceledException)
	{
		// Temptoken is dead due to debounce. (A new save was triggered)
		// Just ignore and no op.
	}
}
```


**Returns**:  
A [response status](ConfigResponseStatus.md) informing if the operation was successfull or not.  

---

### FullResetCurrentConfig
**Signature**: `public static Response<ConfigResponseStatus> FullResetCurrentConfig()`

/// note | No Parameters  
///

**Description**:  
Uses the [`PersistentCVarRegistry`](../../CVars/PersistentCVarRegistry.md) to reset all persistent CVars to their default state with `ramOnly` set to false. This will automatically trigger the save (debounced) when all variables are reset.  

**Example(s)**:  
_Fictional scenario where the user presses "Reset all" in the UI._  
```csharp
void ResetSettings()
{
	var response = UserConfigManager.FullResetCurrentConfig();

	// If successful, early return.
	if(response.Status == ConfigResponseStatus.Success)
		return;
	
	PikeLogger.LogError(LogTarget.All, $"{response.Message}", forceLog: true, tags: response.Tags);
}
```

**Returns**:  
A [response status](ConfigResponseStatus.md) informing if the operation was successfull or not.  

---

### SelectConfig
**Signature**: `public static Response<ConfigResponseStatus> SelectConfig(string configName)`

/// details | Parameter details (Click to expand)  
`string` : `configName`
: The name of the config to save.  
_Note: This can be both a displayname or filename. The backend automatically manages parsing using spaces, and correct application of `.efcg`._
///

**Description**:  
Selects a new config to be the active config.  
Upon a successfull selection all persistent values will be reset using `ramOnly` before applying the new config, also using `ramOnly`.

**Example(s)**:  
_Excerpt from `UserConfigCommandSet.cs`._  
```csharp
Command(
	Signature("active"),
	// . . .
	(args) => {
		if(args.Length < 1)
			return new(ExecutionResponseStatus.Success, UserConfigManager.ActiveConfig.DisplayName);

		var response = UserConfigManager.SelectConfig(args[0]);

		ExecutionResponseStatus s = response.Status == ConfigResponseStatus.Success ? ExecutionResponseStatus.Success : ExecutionResponseStatus.Failed;
		return new(s, response.Message, response.Tags);
	}
),
```

**Returns**:  
A [response status](ConfigResponseStatus.md) informing if the operation was successfull or not.  

---

### RemoveConfig
**Signature**: `public static Response<ConfigResponseStatus> RemoveConfig(string configName)`

/// details | Parameter details (Click to expand)  
`string` : `configName`
: The name of the config to save.  
_Note: This can be both a displayname or filename. The backend automatically manages parsing using spaces, and correct application of `.efcg`._
///

**Description**:  
Deletes a config file.

**Example(s)**:  
_Excerpt from `UserConfigCommandSet.cs`._  
```csharp
Command(
	Signature("remove"),
	// . . .
	(args) => {
		if(!ArgumentParser.ValidateCount(args, 1, out string error))
			return new(ExecutionResponseStatus.InvalidArgs, error, [LogTags.InvalidArgs]);

		var response = UserConfigManager.RemoveConfig(args[0]);

		ExecutionResponseStatus s = response.Status == ConfigResponseStatus.Success ? ExecutionResponseStatus.Success : ExecutionResponseStatus.Failed;
		return new(s, response.Message, response.Tags);
	}
),
```

**Returns**:  
A [response status](ConfigResponseStatus.md) informing if the operation was successfull or not.  

///tip
The most common fail statuses when removing a config is `Failed` due to trying to remove the current config. There must always exist at least one config file at runtime.  

Always check the response's `Message` property when debugging.
///

---