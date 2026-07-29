# PikeLogger

`public static class PikeLogger`  

**Inherits**: None  
**Namespace**: `FractalPike.PikeConsole.Core.Logging`  

## Description

Logging utility with the sole purpose of routing logs. Uses a custom `InterpolatedStringHandler` and `#if TOOLS` compiler directives to reduce processing overhead on non-targeted environments.

///tip
PikeLogger is supposed to be a full replacement for `GD.Print`.  
Replacing `GD.Print` calls with PikeLogger calls will have a beneficial impact on performance.
///

## Events  
| Scope | Delegate | Name |
|-------|--------|------|
| `public` | `LogEventHandler` | [LogEmitted](#logemitted) |

## Methods
| Scope | Return | Name |
|-------|--------|------|
| `public` | `bool` | [IsTargetEnabled](#istargetenabled) |
| `public` | `void` | [Log](#log) |
| `public` | `void` | [LogSuccess](#logsuccess) |
| `public` | `void` | [LogWarning](#logwarning) |
| `public` | `void` | [LogError](#logerror) |

## Event Descriptions  

### LogEmitted
Called when a log has been processed and greenlit to be emitted to the console. This is only ivoked on the correct target environment.  

///note
Unless you are creating a custom, or additional console UI, you will not need to subscribe to this. PikeConsole's default frontend already subscribes to this automatically.
///

## Property Descriptions
No properties present for this class.

## Method Descriptions  

### IsTargetEnabled
**Signature**: `[MethodImpl(MethodImplOptions.AggressiveInlining)]` `public static bool IsTargetEnabled(LogTarget target)`

/// details | Parameter details (Click to expand)  
[`LogTarget`](LogTarget.md) : `target`
: The environment we want to see if it's currently active.
///

**Description**:  
Takes a target environment and checks if that environment is valid / active for the current session.  

///warning | Heads up
This method is mainly for internal use, and serves a crucial part in PikeConsole's performance management. Changes made to this method may result in accidental removal of performance benefits.
///

**Returns**:  
`True` if the target environment is valid.  
`False` if it is not.

---

### Log
**Signature**: `public static void Log(
		LogTarget logTarget,
		[InterpolatedStringHandlerArgument("logTarget")] ref LogInterpolatedStringHandler handler,
		LogLevel logLevel = LogLevel.Info,
		bool forceLog = false,
		string[] tags = null,
		bool includePath = false,
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0,
		[CallerMemberName] string memberName = "")`

/// details | Parameter details (Click to expand)  
[`LogTarget`](LogTarget.md) : `logTarget`
: The target environment for this log.

`interpolated string` : `handler`
: The message to send with this log.
	/// warning | Must be an interpolated string!  
	Example: `$"Hello World!"`
	///

[`LogLevel`](LogLevel.md) : `logLevel`
: The severity of this log. Defaulted to "info".

`bool` : `forceLog`
: Boolean flag applied to log telling other systems not to throttle it.

`string[]` : `tags`
: META-tags for the log. This may be used for anything from styling to data processing. 

`bool` : `includePath`
: Boolean flag used to decide if the path should be concatenated for this log.  
If this is set to `false` the [LogEvent](LogEvent.md)'s sourcepath will remain empty.

`[CallerFilePath] string` : `includePath`
: Compile time variable used for diagnostic data. DO NOT SET.

`[CallerFilePath] int` : `lineNumber`
: Compile time variable used for diagnostic data. DO NOT SET.

`[CallerFilePath] string` : `memberName`
: Compile time variable used for diagnostic data. DO NOT SET.
///

**Description**:  
Send a log to the specified target environment.  
If the environment is invalid, such as using `LogTarget.Debug` when running a release build, the log will not process **nor build the string**. Saving performance.  

Example(s):  

```csharp
// Prints to all runtime UI's and the Godot output
PikeLogger.Log(LogTarget.All, $"Hello world!");

// Prints only to debug runtime UI's.
PikeLogger.Log(LogTarget.Debug, $"We are a debug environment!");
```

---

### LogSuccess
**Signature**: `public static void LogSuccess(
		LogTarget logTarget,
		[InterpolatedStringHandlerArgument("logTarget")] ref LogInterpolatedStringHandler handler,
		bool forceLog = false,
		string[] tags = null,
		bool includePath = false,
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0,
		[CallerMemberName] string memberName = "")`

/// details | Parameter details (Click to expand)  
[`LogTarget`](LogTarget.md) : `logTarget`
: The target environment for this log.

`interpolated string` : `handler`
: The message to send with this log.
	/// warning | Must be an interpolated string!  
	Example: `$"Hello World!"`
	///

`bool` : `forceLog`
: Boolean flag applied to log telling other systems not to throttle it.

`string[]` : `tags`
: META-tags for the log. This may be used for anything from styling to data processing. 

`bool` : `includePath`
: Boolean flag used to decide if the path should be concatenated for this log.  
If this is set to `false` the [LogEvent](LogEvent.md)'s sourcepath will remain empty.

`[CallerFilePath] string` : `includePath`
: Compile time variable used for diagnostic data. DO NOT SET.

`[CallerFilePath] int` : `lineNumber`
: Compile time variable used for diagnostic data. DO NOT SET.

`[CallerFilePath] string` : `memberName`
: Compile time variable used for diagnostic data. DO NOT SET.
///

**Description**:  
Send a log to the specified target environment with the severity of `Success`. The built-in PikeConsole UI will automatically attatch a header for this severity.  

If the environment is invalid, such as using `LogTarget.Debug` when running a release build, the log will not process **nor build the string**. Saving performance.  

Example(s):  

```csharp
// Prints to all runtime UI's and the Godot output
PikeLogger.Log(LogTarget.All, $"Hello world!");

// Prints only to debug runtime UI's.
PikeLogger.Log(LogTarget.Debug, $"We are a debug environment!");
```

---

### LogWarning
**Signature**: `public static void LogWarning(
		LogTarget logTarget,
		[InterpolatedStringHandlerArgument("logTarget")] ref LogInterpolatedStringHandler handler,
		bool forceLog = false,
		string[] tags = null,
		bool includePath = true,
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0,
		[CallerMemberName] string memberName = "")`

/// details | Parameter details (Click to expand)  
[`LogTarget`](LogTarget.md) : `logTarget`
: The target environment for this log.

`interpolated string` : `handler`
: The message to send with this log.
	/// warning | Must be an interpolated string!  
	Example: `$"Hello World!"`
	///

`bool` : `forceLog`
: Boolean flag applied to log telling other systems not to throttle it.

`string[]` : `tags`
: META-tags for the log. This may be used for anything from styling to data processing. 

`bool` : `includePath`
: Boolean flag used to decide if the path should be concatenated for this log.  
If this is set to `false` the [LogEvent](LogEvent.md)'s sourcepath will remain empty.

`[CallerFilePath] string` : `includePath`
: Compile time variable used for diagnostic data. DO NOT SET.

`[CallerFilePath] int` : `lineNumber`
: Compile time variable used for diagnostic data. DO NOT SET.

`[CallerFilePath] string` : `memberName`
: Compile time variable used for diagnostic data. DO NOT SET.
///

**Description**:  
Send a log to the specified target environment with the severity of `Warning`. This log includes the source path by default. The built-in PikeConsole UI will automatically attatch a header for this severity.  

If the environment is invalid, such as using `LogTarget.Debug` when running a release build, the log will not process **nor build the string**. Saving performance.  

Example(s):  

```csharp
// Prints to all runtime UI's and the Godot output
PikeLogger.Log(LogTarget.All, $"Hello world!");

// Prints only to debug runtime UI's.
PikeLogger.Log(LogTarget.Debug, $"We are a debug environment!");
```

---

### LogError
**Signature**: `public static void LogError(
		LogTarget logTarget,
		[InterpolatedStringHandlerArgument("logTarget")] ref LogInterpolatedStringHandler handler,
		bool forceLog = false,
		string[] tags = null,
		bool includePath = true,
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0,
		[CallerMemberName] string memberName = "")`

/// details | Parameter details (Click to expand)  
[`LogTarget`](LogTarget.md) : `logTarget`
: The target environment for this log.

`interpolated string` : `handler`
: The message to send with this log.
	/// warning | Must be an interpolated string!  
	Example: `$"Hello World!"`
	///

`bool` : `forceLog`
: Boolean flag applied to log telling other systems not to throttle it.

`string[]` : `tags`
: META-tags for the log. This may be used for anything from styling to data processing. 

`bool` : `includePath`
: Boolean flag used to decide if the path should be concatenated for this log.  
If this is set to `false` the [LogEvent](LogEvent.md)'s sourcepath will remain empty.

`[CallerFilePath] string` : `includePath`
: Compile time variable used for diagnostic data. DO NOT SET.

`[CallerFilePath] int` : `lineNumber`
: Compile time variable used for diagnostic data. DO NOT SET.

`[CallerFilePath] string` : `memberName`
: Compile time variable used for diagnostic data. DO NOT SET.
///

**Description**:  
Send a log to the specified target environment with the severity of `Error`. This log includes the source path by default. The built-in PikeConsole UI will automatically attatch a header for this severity.  

If the environment is invalid, such as using `LogTarget.Debug` when running a release build, the log will not process **nor build the string**. Saving performance.  

Example(s):  

```csharp
// Prints to all runtime UI's and the Godot output
PikeLogger.Log(LogTarget.All, $"Hello world!");

// Prints only to debug runtime UI's.
PikeLogger.Log(LogTarget.Debug, $"We are a debug environment!");
```

---