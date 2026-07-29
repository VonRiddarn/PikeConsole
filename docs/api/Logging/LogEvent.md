# LogEvent

`public readonly struct LogEvent(int callerKeyHash, LogLevel logLevel, string message, bool forceLog, string[] tags, string sourcePath = "")`  

**Inherits**: [IRuntimeExecutable](../RuntimeExecution/IRuntimeExecutable.md)  
**Namespace**: `FractalPike.PikeConsole.Core.Logging`  

## Description

The data emitted by PikeLogger each time a log occurs. This contains all necessary meta-data about the log to do anything from filtering to styling.

## Properties  
| Scope | Return | Name |
|-------|--------|------|
| `public readonly` | `int` | [CallerKeyHash](#callerkeyhash) |
| `public readonly` | [`LogLevel`](LogLevel.md) | [LogLevel](#loglevel) |
| `public readonly` | `string` | [Message](#message) |
| `public readonly` | `string` | [ForceLog](#forcelog) |
| `public readonly` | `string[]` | [Tags](#tags) |
| `public readonly` | `string` | [SourcePath](#sourcepath) |

## Methods
| Scope | Return | Name |
|-------|--------|------|
| `public` | `bool` | [HasAnyTag](#initialize) |
| `public` | `bool` | [ResetValue](#resetvalue) |
| `public` | `bool` | [ResetValue](#resetvalue) |

## Property Descriptions

### CallerKeyHash

**Signature**: `public readonly int CallerKeyHash`

**Description**:  
Unique key built using the callers (compile time) filepath and linenumber.  
This is used by the frontend to throttle logs comming from the exact same line.  

///tip
If the throttle swallows logs it's not supposed to, you can set the [forceLog](#forcelog) flag to push the log through. This may however have am impact on performance.
///

---

### LogLevel

**Signature**: `public readonly LogLevel LogLevel`

**Description**:  
This holds the severity / category of the log, like "Info" or "Error".  
For a complete list of levels, check the [LogLevel](LogLevel.md) page.

---

### Message

**Signature**: `public readonly string Message`

**Description**:  
Holds the log message.

---

### ForceLog

**Signature**: `public readonly bool ForceLog`

**Description**:  
Flag used by listeners. If this flag is set to true, the frontend will ignore throttling for this log.

---

### Tags

**Signature**: `public readonly string[] Tags`

**Description**:  
META-tags for the log. This may be used for anything from styling to data processing. Currently (v1.0.0) this has yet to be used for anything but styling though.  

///tip
PikeConsole provides some default log tags found in:  
`Core` > `Utilities` > `LogTags.cs`  
These can be used to apply certain headers to log messages.
///

---

### SourcePath

**Signature**: `public readonly string SourcePath`

**Description**:  
The absolute (compile time) caller path in plain text.  
If a log method was called from [PikeLogger](PikeLogger.md) with `includePath` as `false` this will be empty.

---

## Method Descriptions  

### HasAnyTag

**Signature**: `public bool HasAnyTag(string[] searchTags)`

/// details | Parameter details (Click to expand)  
`string[]` : `searchtags`
: Tags to search for within this log event.
///

**Description**:  
Takes an array of strings and checks if any tag in the log event matches any tag in the string.


**Returns**:  
`True` if any of the tags exists.  
`False` if none of the tags exists.

---

### TryGetAnyTag

**Signature**: `public bool TryGetAnyTag(string[] searchTags, out string tag)`

/// details | Parameter details (Click to expand)  
`string[]` : `searchtags`
: Tags to search for within this log event.

`out string[]` : `tag`
: Out parameter for the first tag that was found.
///

**Description**:  
Takes an array of strings and checks if any tag in the log event matches any tag in the string. If it does, it'll set the first match in the out parameter and return.


**Returns**:  
`True` if any of the tags exists.  
`False` if none of the tags exists.  

Uses an `out` parameter for the found tag.

---

### HasTag

**Signature**: `public bool HasTag(string searchTag)`

/// details | Parameter details (Click to expand)  
`string` : `searchtag`
: Tag to search for within this log event.
///

**Description**:  
Takes a string and checks if any tag in the log event matches said string.


**Returns**:  
`True` if the tags exists.  
`False` the tags doesn't exists.  

---
