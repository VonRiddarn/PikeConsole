# StatementExecutor
`public static class StatementExecutor`  

**Inherits**: None  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution`  

## Description

Takes execution statements and tries to execute them.  
An execution statement can be both raw or pre-parsed.

When executing statements the `StatementExecutor` will first look for `IRuntimeExecutable`s and then aliases. Should the executor find a `IRuntimeExecutable` it will not check the alias registry.  

## Properties  
No public properties to showcase for this class.

## Methods
### Protected
| Scope | Return | Name |
|-------|--------|------|
| `public` | `void` | [Execute](#execute) |


## Method Descriptions  

# TODO: CONTINUE FROM HERE!!!!
# CommandSet.md contains the "style guide".

### Execute
**Signature**: `public static void Execute(ExecutionSource executionSource, string signature, string[] args, bool silent = false)`

/// details | Parameters  
`Signature` : `string`
: The command signature used to call the command, eg: `my_echo`

`ShortDesc` : `string`
: A summary description of the command, preferably a one-liner.  

: **Example**:  
`Joins and echoes the arguments back to the caller`  

`LongDesc` : `string?`
: An optional longer (multi-line) description of the command providing more context.  

`Usage` : `string`
: Usage instructions for the command.

: **Example**:  
`my_echo [args...]`  

`IsCheat` : `bool`
: Defines if `cheatmode` must be active to run this command **in the console**.  

: /// note | Internal systems can still run commands tagged with cheats
///

`Action` : `Func<string[], Response<ExecutionResponseStatus>>`
: A method that takes in a string array and returns a Response. 
	
: /// note | All action methods **must** return a response.
///  
///
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
