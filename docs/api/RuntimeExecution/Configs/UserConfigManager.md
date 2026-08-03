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
_Excerpt from `UserConfigUpdater.cs`_
```csharp
var response = UserConfigManager.SelectConfig(UserConfigManager.ActiveConfig.FileName);

if (response.Status != ConfigResponseStatus.Success)
	UserConfigManager.CreateAndSelectDefaultConfig();
```

**Returns**:  
A response status informing if the operation was successfull or not.

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
: If set to true, the new profile will automatically be selected upon creation and trigger [`ActiveConfigChanged`](#activeconfigchanged).   

///

**Description**:  
Creates a new user profile.

**Returns**:  
A response status informing if the operation was successfull or not.  

///tip
The most common fail status when creating a profile is `FileConflict`. This simply means there is already a profile with that name already present.
///

---

# TODO: Continue documenting tomorrow