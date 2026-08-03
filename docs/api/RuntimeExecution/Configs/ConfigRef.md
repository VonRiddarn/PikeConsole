# ConfigRef

`public class ConfigRef`  

**Inherits**: None.  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution.Config`  

## Description

An instance of a path split into several executable config related properties.

## Properties  
| Scope | Return | Name |
|-------|--------|------|
| `public readonly` | `string` | [FullPath](#fullpath) |
| `public` | `string` | [FileName](#filename) |
| `public` | `string` | [DisplayName](#displayname) |
| `public` | `string` | [Directory](#directory) |

## Methods
| Scope | Return | Name |
|-------|--------|------|
| `public static` | `string` | [DisplayToFileName](#displaytofilename) |
| `public static` | `string` | [FileToDisplayName](#filetodisplayname) |

## Property Descriptions

### FullPath

**Signature**: `public readonly string FullPath`

**Description**:  
The full system path to the config file.

---

### FileName

**Signature**: `public string FileName { get; }`

**Description**:  
The config filename, including the file extention.

---

### DisplayName

**Signature**: `public string DisplayName { get; }`

**Description**:  
The config display name. Using spaces instead of underscores and does not include the file extention.

---

### Directory

**Signature**: `public string Directory { get; }`

**Description**:  
The path to the directory this config resides in.

---

## Method Descriptions  

### DisplayToFileName

**Signature**: `public static string DisplayToFileName(string displayName)`

/// details | Parameter details (Click to expand)  
`string[]` : `displayName`
: The display name of the config.
///

**Description**:  
Takes a config displayname and converts it to a valid filename which matches the name on disk.

**Returns**:  
The name as a filename.  
`Timmy the programmer`  
becomes  
`timmy_the_programmer.ecfg`  

---

### FileToDisplayName

///note
This method is mainly used internally when instancing, but may be useful for UI tasks.
///

**Signature**: `public static string FileToDisplayName(string fileName)`

/// details | Parameter details (Click to expand)  
`string[]` : `fileName`
: The filename name of the config.
///

**Description**:  
Takes a config filename and converts it to a displayable name.

**Returns**:  
The name as a displayname.  
`timmy_the_programmer.ecfg`  
becomes  
`Timmy The Programmer`  
_Notice that it becomes capitalized. This has no effect on internal comparisons as they are case insensitive._

---