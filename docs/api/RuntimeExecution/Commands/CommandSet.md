# CommandSet
`public abstract partial class CommandSet : Node`  

**Inherits**: [Node (External link)](https://docs.godotengine.org/en/stable/classes/class_node.html#class-node)  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution.Commands`  

## Description

Base class that automatically syncs the lifespan of registered commands to its own lifecycle.  
Internalizes all advanced setup and exposes a lightweight API that can be used by the end-developer.

## Properties  
No public properties to showcase for this class.

## Methods
### Protected
| Scope | Return | Name |
|-------|--------|------|
| `protected` | `Command[]` | [InstantiateCommands](#initializecommands) |
| `protected` | `Command[]` | [Command](#command) |
| `protected` | `void` | [OnEnterTree](#onentertree) |
| `protected` | `void` | [OnReady](#onready) |
| `protected` | `void` | [OnExitTree](#onexittree) |
| `protected` | `void` | [OnCheatModeChanged](#oncheatmodechanged) |


## Method Descriptions  

### InstantiateCommands
**Signature**: `protected abstract Command[] InstantiateCommands();`

**Description**:  
Executes at the start of `_EnterTree` to hydrate the internal command list.

**Example**:
```csharp {linenums="1"}
protected override Command[] InstantiateCommands() => [
	Command( /* Command stuff */),
	Command( /* Command stuff */),
	Command( /* Command stuff */),
];
```
---
### Command
**Overrides**
=== "Documented"
	**Signature**: `protected Command Command(
			string signature,
			string shortDesc,
			string longDesc,
			string usage,
			bool isCheat,
			Func<string[], Response<ExecutionResponseStatus>> action,
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0)`

	**Description**:  
	Declarative shorthand method for creating a command **with runtime documentation**.  
	Using this method automatically attaches the current `CommandSet`'s filepath, 
	making any errors invoked from the command self-diagnostic.  

	/// details | Parameter details (Click to expand)   
	`string` : `Signature`
	: The command signature used to call the command, eg: `my_echo`

	`string` : `ShortDesc`
	: A summary description of the command, preferably a one-liner.  

	: **Example**:  
	`Joins and echoes the arguments back to the caller`  

	`string?` : `LongDesc`
	: An optional longer (multi-line) description of the command providing more context.  

	`string` : `Usage`
	: Usage instructions for the command.

	: **Example**:  
	`my_echo [args...]`  

	`bool` : `IsCheat`
	: Defines if `cheatmode` must be active to run this command **in the console**.  

	: /// note | Internal systems can still run commands tagged with cheats
	///

	`Func<string[], Response<ExecutionResponseStatus>>` : `Action`
	: A method that takes in a string array and returns a Response. 
		
	: /// note | All action methods **must** return a response.
	///  
	///
		
	/// warning
	Do not manually set the `filePath` or `lineNumber` properties!  
	Doing so will break the self-diagnostic nature of the command 
	and defeat the purpose of the shorthand.
	///

	**Example**:
	```csharp {linenums="1"}
	protected override Command[] InstantiateCommands() => [
		Command(
			"my_echo",
			"Joins and echoes the arguments back to the caller",
			null,
			"my_echo [args...]",
			false,
			(args) =>
				new(ExecutionResponseStatus.Success, $"{args.Join(" ")}")
		),
	];
	```
=== "Quick"
	**Signature**: `protected Command Command(
			string signature,
			bool isCheat,
			Func<string[], Response<ExecutionResponseStatus>> action,
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0)`

	/// tip
	The quick shorthand will generate warnings in debug mode (debug builds and the Editor playtest).  
	To disable this warning go to: `Project Settings (General)` > `Fractal Pike` > `PikeConsole`  
	And turn on: `Suppress Documentation Warnings`
	///

	**Description**:  
	Declarative shorthand method for creating a command with **without runtime documentation**.  
	Using this method automatically attaches the current `CommandSet`'s filepath and linenumber, making any errors invoked from the command self-diagnostic.  

	/// details | Parameter details (Click to expand)  
	`string` : `Signature`
	: The command signature used to call the command, eg: `my_echo`

	`bool` : `IsCheat`
	: Defines if `cheatmode` must be active to run this command **in the console**.  

	: /// note | Internal systems can still run commands tagged with cheats
	///

	`Func<string[], Response<ExecutionResponseStatus>>` : `Action`
	: A method that takes in a string array and returns a Response. 
		
	: /// note | All action methods **must** return a response.
	///  
	///

	/// warning
	Do not manually set the `filePath` or `lineNumber` properties!  
	Doing so will break the self-diagnostic nature of the command 
	and defeat the purpose of the shorthand.
	///

	**Example**:
	```csharp {linenums="1"}
	protected override Command[] InstantiateCommands() => [
		Command(
			"my_echo",
			false,
			(args) =>
				new(ExecutionResponseStatus.Success, $"{args.Join(" ")}")
		),
	];
	```
---

### OnEnterTree
**Signature**: `protected virtual void OnEnterTree()`

**Description**:  
Wrapper that executes at the end of the Nodes original `_EnterTree`.  
Since internal tooling relies on the native `_EnterTree` method, 
this wrapper must be used when speaking to the API.  

/// note
This method runs after [InstantiateCommands](#initializecommands), meaning it is safe to assume commands are instantiated by this point.

They are however, not registered to the global RuntimeExecution registry.
///

**Example**:
```csharp {linenums="1"}
protected override void OnEnterTree()
{
	PikeLogger.Log(LogTarget.All, $"Tree entered!");
}
```
---

### OnReady
**Signature**: `protected virtual void OnReady()`

**Description**:  
Wrapper that executes at the end of the Nodes original `_Ready`.  
Since internal tooling relies on the native `_Ready` method, 
this wrapper must be used when speaking to the API.  

/// note
This method runs after the commands have been registered to the RuntimeExecution registry. It is safe to assume that commands are fully integrated at this point.
///

**Example**:
```csharp {linenums="1"}
protected override void OnReady()
{
	PikeLogger.Log(LogTarget.All, $"Node ready!");
}
```
---

### OnExitTree
**Signature**: `protected virtual void OnExitTree()`

**Description**:  
Wrapper that executes at the end of the Nodes original `_ExitTree`.  
Since internal tooling relies on the native `_ExitTree` method, 
this wrapper must be used when speaking to the API.  

/// note
This method runs after the commands have been **un**registered to the RuntimeExecution registry. At this point, the registry no longer knows about the commands.
///

**Example**:
```csharp {linenums="1"}
protected override void OnExitTree()
{
	PikeLogger.Log(LogTarget.All, $"Tree exited!");
}
```
---

### OnCheatModeChanged
**Signature**: `protected virtual void OnCheatModeChanged(bool newState)`

**Description**:  
Executes when `PikeConsoleConfig.CheatMode` is toggled.  
This is the recommended location to reset gamestate data when access to cheats is revoked. 

**Example**:
```csharp {linenums="1"}
protected override void OnCheatModeChanged(bool newState)
{
	if (newState == false)
	{
		PikeLogger.Log(LogTarget.All, $"Force removing noclip...");
	}
}
```