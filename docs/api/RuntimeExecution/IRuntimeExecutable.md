# IRuntimeExecutable

`public interface IRuntimeExecutable`  

**Inherits**: None  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution`  

## Description

Base contract for all runtime executables. This is used to execute both commands and CVars. 

///tip | Commands are `IRuntimeExecutable`
Commands are direct decendents of `IRuntimeExecutable`.  
When dealing with a command directly, treat it exactly as you would any other executable.
///

## Properties  
| Scope | Return | Name |
|-------|--------|------|
| `public` | `string` | [DisplayType](#displaytype) |
| `public` | `string` | [Signature](#signature) |
| `public` | `string` | [ShortDesc](#shortfesc) |
| `public` | `string` | [LongDesc](#longdesc) |
| `public` | `string[]` | [Usages](#usages) |
| `public` | `bool` | [IsCheat](#ischeat) |
| `public` | `bool` | [HideInRelease](#hideinrelease) |
| `public` | `string` | [SourceLocation](#sourcelocation) |

## Methods
| Scope | Return | Name |
|-------|--------|------|
| `public` | `bool` | [Execute](#execute) |
| `public` | `bool` | [GetHelp](#gethelp) |

## Property Descriptions

### DisplayType

**Signature**: `public string DisplayType { get; }`

**Description**:  
The type of executable in human-readable format.  

**Example(s)** 

_Excerpt from `Command.cs`_
```csharp
DisplayType = "Command";
```

_Excerpt from `CvarFloat.cs`_
```csharp
public override string DisplayType => "CVar_Float";
```

---

### Signature

**Signature**: `	public string Signature { get; }`

**Description**:  
The runtime executables signature, eg: `echo` or `m_sensitivity`.

---
### ShortDesc

**Signature**: `public string ShortDesc { get; }`

**Description**:  
A (preferably) single-line description of the executables function.

---
### LongDesc

**Signature**: `public string LongDesc { get; }`

**Description**:  
A thorough description of the command and it's application.
---
### Usages

**Signature**: `public string[] Usages { get; }`

**Description**:  
The different ways one may run this executable, like different argument types or count.  

**Example(s)**:

__Excerpt from `CVarColor.cs`
```csharp
public override string[] Usages => 
[
	$"{Signature} [hex value]", 
	$"{Signature} [Red 0-255] [Green 0-255] [Blue 0-255] [Alpha? 0-255]"
];
```

---
### IsCheat

**Signature**: `public bool IsCheat { get; }`

**Description**:  
Boolean flag to mark the executable as a cheat. Executables with this set to true cannot be executed by the end user (Player) without `cheatmode` enabled.  

///note
The system (game) can run executables even if they are cheat protected.
///

---
### HideInRelease

**Signature**: `public bool HideInRelease { get; }`

**Description**:  
Boolean flag used to hide the executable in release builds of the game.  
If the flag is set to true, the runtime exeutable registry automatically filters it out if the current build is a release build.  

This is usefull for commands or CVars which are _only_ relevant during development. Such as commands for setting the state of achivements etc.  

---
### SourceLocation

**Signature**: `public string SourceLocation { get; }`

**Description**:  
Static full path to the factual executable. For commands, this is compile-time and for CVars, this is set at initialization using ResourcePath.  

Used by the `whereis` command as well as in self-diagnostic errors.

---

## Method Descriptions  

### Execute
**Signature**: `public Response<ExecutionResponseStatus> Execute(ExecutionSource source, string[] args)`

/// details | Parameter details (Click to expand)  
[ExecutionSource](../RuntimeExecution/ExecutionSource.md) : `source`
: The entity wanting to execute this statement. Used to determine cheat override authority.

`string[]` : `args`
: The arguments to pass into the execution method.

///

**Description**:  
Execute the logic stored within this runtime executable.  
This could be anything from teleporting the player, echoing in the console, writing to a file or changing the value of a CVar.

---

### GetHelp
**Signature**: `public string GetHelp()`

/// note | Parameter details  
No parameters for this method.
///

**Description**:  
Gets in-depth usage information about this executable.  
By default this is routed through the `ConsoleFormatter` in order to provide a 
normalized response for all runtime executables.