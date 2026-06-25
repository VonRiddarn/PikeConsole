# ArgumentParser
`public static class ArgumentParser`  

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
| `public` | `bool` | [ValidateCount](#validatecount) |
| `public` | `bool` | [TryParseBool](#tryparsebool) |
| `public` | `bool` | [TryParseEnum](#tryparseenum) |
| `public` | `bool` | [TryParseMany](#tryparsemany) |
| `public` | `bool` | [TryParseManyFloat](#tryparsemanyfloat) |
| `public` | `bool` | [TryParseManyDouble](#tryparsemanydouble) |

# TODO: PRIO 1 - CONTINUE HERE

## Method Descriptions  

### ValidateCount

=== "RAW / Input"  
	**Signature**: `public static void Execute(ExecutionSource source, string rawInput, bool silent = false)`

	/// details | Parameter details (Click to expand)  
	[ExecutionSource](../RuntimeExecution/ExecutionSource.md) : `source`
	: The entity wanting to execute this statement. Can be `Player` or `System`.  
	_Note: Executing as `System` bypasses cheat protection._

	`string` : `rawInput`
	: Raw, unparsed input. Usually provided from a line in a config, or user input through the console.  

	: **Example**:  
	`echo Hello World!; echo "Hello back at you!"`


	`bool` : `silent` def `false`
	: If set to true, success commands are supressed.  
	Useful for supressing console spam when a system sets a large amount of variables  
	```
	ph_gravity set to 3
	pl_speed set to 25
	pl_jump_force set to 33
	...
	```

	: /// note | _Errors_ will always log.
	///
	///

	**Description**:  
	Takes a raw input and parses it into valid statement profiles.  
	These statement profiles are then executed in order.  

	Invalid statements will bounce and return "not a valid command".  
	Thus, it is safe to pass wild, untamed data into this method.


	**Examples**:

	_Execute based on user input._
	```csharp

	void OnUserPressEnter(string consoleInput)
	{
		StatementExecutor.Execute(
			ExecutionSource.System,
			consoleInput
		);
	}
	```

	_Initialize commands from a file of unknown contents._
	```csharp

	// Fetch map config settings.
	string[] lines = File.ReadAllLines("map_spooky_manor.cfg");

	// Execute map config.
	foreach(string line in lines)
	{
		StatementExecutor.Execute(
			ExecutionSource.System,
			line,
			true
		);
	}
	```

=== "Programmatic"  
	**Signature**: `public static void Execute(ExecutionSource executionSource, string signature, string[] args, bool silent = false)`

	/// details | Parameter details (Click to expand)  
	[ExecutionSource](../RuntimeExecution/ExecutionSource.md) : `source`
	: The entity wanting to execute this statement. Can be `Player` or `System`.  
	_Note: Executing as `System` bypasses cheat protection._

	`string` : `signature`
	: The signature of the command to execute.  

	: **Example**:  
	`echo`

	`string[]` : `args`
	: The arguments to pass through with the command signature.  

	: **Example**:  
	`["Hello", "world!"]`  

	`bool` : `silent` def `false`
	: If set to true, success commands are supressed.  
	Useful for supressing console spam when a system sets a large amount of variables  
	```
	ph_gravity set to 3
	pl_speed set to 25
	pl_jump_force set to 33
	...
	```

	: /// note | _Errors_ will always log.
	///
	///

	**Description**:  
	Takes the signature of a command and passes arguments into its execution method.  
	If the command returns a response, the response is logged before proceeding.

	**Examples**:

	_Print back "Hello World!"_
	```csharp

	StatementExecutor.Execute(
		ExecutionSource.System,
		"echo",
		["Hello", "World!"],
	);
	```

	_Silently set "ph_gravity" without confirmation._
	```csharp

	StatementExecutor.Execute(
		ExecutionSource.System,
		"ph_gravity",
		["3"],
		true
	);
	```

=== "Batch"  
	**Signature**: `public static void Execute(ExecutionSource source, ParsedStatement[] parsedStatements, bool silent = false)`

	/// details | Parameter details (Click to expand)  
	[ExecutionSource](../RuntimeExecution/ExecutionSource.md) : `source`
	: The entity wanting to execute this statement. Can be `Player` or `System`.  
	_Note: Executing as `System` bypasses cheat protection._

	[ParsedStatement](../RuntimeExecution/ParsedStatement.md)`[]` : `parsedStatements`
	: A list of pre-parsed statement objects.  
	Usually returned from the [StatementParser](../RuntimeExecution/StatementParser.md) class.

	: **Example**:  
	```csharp
	{
		Signature: "echo";
		Arguments = ["Hello", "world!"];
	}
	```

	`bool` : `silent` def `false`
	: If set to true, success commands are supressed.  
	Useful for supressing console spam when a system sets a large amount of variables  
	```
	ph_gravity set to 3
	pl_speed set to 25
	pl_jump_force set to 33
	...
	```

	: /// note | _Errors_ will always log.
	///
	///

	**Description**:  
	Takes a parsed statement and executes the signature with the accompanied arguments. Mainly for internal use, but could work well with a statement container-like structure.

---
