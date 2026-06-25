# ArgumentParser
`public static class ArgumentParser`  

**Inherits**: None  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution`  

## Description

Takes arguments and performs parsing actions on them.  
This is a suplement to regular parsing methods, not a replacement. 
Regular parsing, such as for a singular int, should still be handled 
with the native .NET parser (Example: `int.TryParse`).

## Properties  
No public properties to showcase for this class.

## Methods
### Protected
| Scope | Return | Name |
|-------|--------|------|
| `public` | `bool` | [ValidateCount](#validatecount) |
| `public` | `bool` | [TryParseBool](#tryparsebool) |
| `public` | `bool` | [TryParseEnum](#tryparseenum) |
| `public` | `bool` | [TryParseManyInt](#tryparsemanyint) |
| `public` | `bool` | [TryParseManyFloat](#tryparsemanyfloat) |
| `public` | `bool` | [TryParseManyDouble](#tryparsemanydouble) |
| `public` | `bool` | [TryParseManyBool](#tryparsemanybool) |
| `public` | `bool` | [TryParseManyEnum](#tryparsemanyenum) |

## Method Descriptions  

### ValidateCount

=== "Exact"  
	**Signature**: `public static bool ValidateCount(ReadOnlySpan<string> args, int count, out string error)`

	/// details | Parameter details (Click to expand)  
	`ReadOnlySpan<string>` : `args`
	: The argument string array. Passed as `ReadOnlySpan<string>` for performance and mutability safety.  
	**Example**: `["Hello", "world!"]`

	`int` : `count`
	: The exact amount of arguments allowed.

	`string` : **out** `error`
	: An error message that is filled if the arguments are not valid.  
	_`string.Empty` if the validation passes._

	: **Example**:  
	`"Too many arguments. Argument count must be exactly 1."`
	///
	
	**Description**:  
	Takes an arguments array and a count, then returns if the array length is within 


	**Examples**:

	_Usage within a CVar._
	```csharp
	// If there is not EXACTLY 1 argument, return an error.
	if (!ArgumentParser.ValidateCount(args, 1, out string error))
		return new(CvarSetResponseStatus.InvalidArgs, error);
	```

=== "Exact (Multiple)"  
	**Signature**: `public static bool ValidateCount(ReadOnlySpan<string> args, int count, out string error)`

	/// details | Parameter details (Click to expand)  
	`ReadOnlySpan<string>` : `args`
	: The argument string array. Passed as `ReadOnlySpan<string>` for performance and mutability safety.  
	**Example**: `["Hello", "world!"]`

	`ReadOnlySpan<int>` : `counts`
	: An array of ints where each element is an allowed exact count of arguments.

	`int` : **out** `count`
	: The amount of arguments if the check passed. Can be easily used with a switch statement.  
	_`-1` if the validation fails._

	: **Example**:  
	`-1` or `1` or `3`...

	`string` : **out** `error`
	: An error message that is filled if the arguments are not valid.  
	_`string.Empty` if the validation passes._

	: **Example**:  
	`"Invalid argument count. Expected one of: 1, 3"`
	///

	**Description**:  
	Takes an arguments array and a counts array, then compares the length of the arguments array to the values in the counts array. 
	If the length matches any of the values exactly, the method returns true and assigns the length to the `out int count` parameter.


	**Examples**:

	_Using the method with a switch statement._
	```csharp
	// If there is not EXACTLY 1 or 3 arguments, return an error.
	if (!ArgumentParser.ValidateCount(args, [1, 3], out int count, out string error))
		return new(CvarSetResponseStatus.InvalidArgs, error);

	// Count is guaranteed to be 1 or 3 here.
	switch(count)
	{
		case 1: 
			ExecuteWithOne();
			break;
		case 3:
			ExecuteWithThree();
			break;
		
		// The default state is not needed, but can stil be included for good hygiene.  
		// It catches cases where the developer messes up the cases.
		default:
			return new(CvarSetResponseStatus.InvalidArgs, error);
	}
	```

=== "Range"  
	**Signature**: `public static bool ValidateCount(ReadOnlySpan<string> args, int count, out string error)`

	/// details | Parameter details (Click to expand)  
	`ReadOnlySpan<string>` : `args`
	: The argument string array. Passed as `ReadOnlySpan<string>` for performance and mutability safety.  
	**Example**: `["Hello", "world!"]`

	`int` : `min`
	: The minimum amount of arguments allowed.
	
	`int` : `max`
	: The maximum amount of arguments allowed.

	`string` : **out** `error`
	: An error message that is filled if the arguments are not valid.  
	_`string.Empty` if the validation passes._

	: **Example**:  
	`"Too many arguments."` or `"Not enough arguments."`
	///
	
	**Description**:  
	Takes an arguments array and a min / max inclusive value , then returns if the array length is within bounds.  


	**Examples**:

	_Usage within a CVar._
	```csharp
	// If there arent BETWEEN 1 and 3 arguments, return an error.
	if (!ArgumentParser.ValidateCount(args, 1, 3, out string error))
		return new(CvarSetResponseStatus.InvalidArgs, error);
	```
---

### TryParseBool

**Signature**: `public static bool TryParseBool(ReadOnlySpan<char> input, out bool value)`

/// details | Parameter details (Click to expand)  
`ReadOnlySpan<char>` : `input`
: Raw input that should be parsed into a boolean value.  
Accepts `1`, `0`, `true` and `false`. Case insensitive.  

: **Example**:  
`TRUE` or `fAlSe` or `1` or `0`

`bool` : **out** `value`
: The input value parsed and converted to a strictly typed boolean. 

///

**Description**:  
Takes an input and returns a strictly typed boolean from it.  


**Examples**:

_Excerpt from [CVarBool](../CVars/CVarBool.md)._
```csharp
if (!ArgumentParser.TryParseBool(args[0], out bool value))
	return new(CvarSetResponseStatus.Failed, $"Could not parse {args[0]} into type bool.");
// . . .
Value = value;
```

### TryParseEnum

**Signature**: `public static bool TryParseEnum(ReadOnlySpan<char> input, ReadOnlySpan<string> options, out int index, out string error)`

/// details | Parameter details (Click to expand)  
`ReadOnlySpan<char>` : `input`
: Raw input that should be parsed into the enum value.  
Accepts the index as an `int` or the named enum value as a `string`. Case insensitive.  

: **Example**:  
`1` or `medium` or `meDiUm`

`ReadOnlySpan<string>` : `options`
: An array of strings that represent the options for the enum.  

: **Example**:  
`["easy", "medium", "hard", "extreme"]`

`int` : **out** `index`
: A strictly typed `int` that represents the index of the option.  

: **Example** _(using above context)_:  
`medium` will yield `1`.  
`extreme` will yield `3`.

`string` : **out** `error`
: An error message that is filled if the input is not valid.  
_`string.Empty` if the enum parses correctly._

///

**Description**:  
Takes an input and an array of options, then locates the input 
value within the options array and returns the index.  

/// note | This is not a strong enum
This is used mainly for the [CVarEnum](../CVars/CVarEnum.md) resource.  
It is tailored around limitations in the Godot editor and 
strong typing.  
We are basically creating a weak reference using 
a string array instead of a strict enum.  
///


**Examples**:

_Excerpt from [CVarEnum](../CVars/CVarEnum.md)._
```csharp
if (!ArgumentParser.TryParseEnum(args[0], _options, out int index, out error))
	return new(CvarSetResponseStatus.Failed, error);
// . . .
Value = index;
```

### TryParseManyInt

**Signature**: `public static bool TryParseManyInt(ReadOnlySpan<string> args, out int[] values, out string error)`

/// details | Parameter details (Click to expand)  
`ReadOnlySpan<string>` : `args`
: Raw input arguments that should be parsed into a `int`.    

: **Example**:  
`["1", "1337", "2"]`

`int[]` : **out** `values`
: A strictly typed array of all inputs parsed to `int`s.  

: **Example** _(using above context)_:  
`[1, 1337, 2]`

`string` : **out** `error`
: An error message that is filled if ANY of the input are not valid.  
_`string.Empty` if the input is parsed correctly._

///

**Description**:  
Takes an array of raw input arguments, 
then parses them into a strictly typed array of `int`s.  

All arguments must pass for the method to return successfull.

**Examples**:

_Hypothetical usage within a command that sets the players position in a 2d grid._
```csharp
// . . . 
if (!ArgumentParser.TryParseManyInt(args.AsSpan(), out int[] coords, out error))
	return new Response<ExecutionResponseStatus>(ExecutionResponseStatus.InvalidArgs, $"Invalid grid input: {error}");
// . . .
Player.GridPosition = new Vector2(coords[0], coords[1]);
```

/// tip | Continuous vs Non-continuous arguments
If the parameters you want to parse are non-continuous you can pass a collection expression instead of an array span.  
Below are 3 common examples of how we might want to pass arguments.

```csharp
// Args index 0, 2 and 4. Skip all others
ArgumentParser.TryParseManyInt([args[0], args[2], args[4]], out int[] values, out error)

// All arguments
ArgumentParser.TryParseManyInt(args.AsSpan(), out int[] values, out error)

// Arguments from index 0 to index 2
ArgumentParser.TryParseManyInt(args.AsSpan(0, 2), out int[] values, out error)
```
///

### TryParseManyFloat

**Signature**: `public static bool TryParseManyFloat(ReadOnlySpan<string> args, out float[] values, out string error)`

/// details | Parameter details (Click to expand)  
`ReadOnlySpan<string>` : `args`
: Raw input arguments that should be parsed into a `float`.  
Internal systems automatically handles culture invariances, 
forcing all users to use periods to separate decimals.

: **Example**:  
`["1.1", "77.7", "2"]`

`float[]` : **out** `values`
: A strictly typed array of all inputs parsed to `float`s.  

: **Example** _(using above context)_:  
`[1.1f, 77.7f, 2f]`

`string` : **out** `error`
: An error message that is filled if ANY of the input are not valid.  
_`string.Empty` if the input is parsed correctly._

///

**Description**:  
Takes an array of raw input arguments, 
then parses them into a strictly typed array of `floats`s.  

All arguments must pass for the method to return successfull.

**Examples**:

_Hypothetical usage within a command that sets the players position in 3d world space._
```csharp
// . . . 
if (!ArgumentParser.TryParseManyFloat(args.AsSpan(), out float[] newPos, out error))
	return new Response<ExecutionResponseStatus>(ExecutionResponseStatus.InvalidArgs, $"Invalid position input: {error}");
// . . .
Player.Position = new Vector3(newPos[0], newPos[1], newPos[2]);
```

/// tip | Continuous vs Non-continuous arguments
If the parameters you want to parse are non-continuous you can pass a collection expression instead of an array span.  
Below are 3 common examples of how we might want to pass arguments.

```csharp
// Args index 0, 2 and 4. Skip all others
ArgumentParser.TryParseManyFloat([args[0], args[2], args[4]], out float[] values, out error)

// All arguments
ArgumentParser.TryParseManyFloat(args.AsSpan(), out float[] values, out error)

// Arguments from index 0 to index 2
ArgumentParser.TryParseManyFloat(args.AsSpan(0, 2), out float[] values, out error)
```
///

### TryParseManyDouble

**Signature**: `public static bool TryParseManyDouble(ReadOnlySpan<string> args, out double[] values, out string error)`

/// details | Parameter details (Click to expand)  
`ReadOnlySpan<string>` : `args`
: Raw input arguments that should be parsed into a `double`.  
Internal systems automatically handles culture invariances, 
forcing all users to use periods to separate decimals.

: **Example**:  
`["1.1", "77.7", "2"]`

`double[]` : **out** `values`
: A strictly typed array of all inputs parsed to `double`s.  

: **Example** _(using above context)_:  
`[1.1d, 77.7d, 2d]`

`string` : **out** `error`
: An error message that is filled if ANY of the input are not valid.  
_`string.Empty` if the input is parsed correctly._

///

**Description**:  
Takes an array of raw input arguments, 
then parses them into a strictly typed array of `double`s.  

All arguments must pass for the method to return successfull.

**Examples**:

_Hypothetical usage within a command that sets the world origin in 3d large world space._  
_In reality, this would never be done through commands._
```csharp
// . . . 
if (!ArgumentParser.TryParseManyDouble(args.AsSpan(), out double[] newOrigin, out error))
	return new Response<ExecutionResponseStatus>(ExecutionResponseStatus.InvalidArgs, $"Invalid large position input: {error}");
// . . .
WorldManager.SetOrigin(newOrigin[0], newOrigin[1], newOrigin[2]);
```

/// tip | Continuous vs Non-continuous arguments
If the parameters you want to parse are non-continuous you can pass a collection expression instead of an array span.  
Below are 3 common examples of how we might want to pass arguments.

```csharp
// Args index 0, 2 and 4. Skip all others
ArgumentParser.TryParseManyDouble([args[0], args[2], args[4]], out double[] values, out error)

// All arguments
ArgumentParser.TryParseManyDouble(args.AsSpan(), out double[] values, out error)

// Arguments from index 0 to index 2
ArgumentParser.TryParseManyDouble(args.AsSpan(0, 2), out double[] values, out error)
```
///

### TryParseManyBool

**Signature**: `public static bool TryParseManyBool(ReadOnlySpan<string> args, out bool[] values, out string error)`

/// details | Parameter details (Click to expand)  
`ReadOnlySpan<string>` : `args`
: Raw input arguments that should be parsed into a `bool`.  

: **Example**:  
`["true", "1", "false", "0"]`

`bool[]` : **out** `values`
: A strictly typed array of all inputs parsed to `bool`s.  

: **Example** _(using above context)_:  
`[true, true, false, false]`

`string` : **out** `error`
: An error message that is filled if ANY of the input are not valid.  
_`string.Empty` if the input is parsed correctly._

///

**Description**:  
Takes an array of raw input arguments, 
then parses them into a strictly typed array of `bool`s.  

All arguments must pass for the method to return successfull.

**Examples**:

_Hypothetical usage within a command that has several boolean flags scattered in the argument signature._  
_In this case, they are implicitly paired with positional and rotational arguments._
```csharp
// . . . 
if (!ArgumentParser.TryParseManyBool([args[1], args[3]], out bool[] flags, out error))
	return new Response<ExecutionResponseStatus>(ExecutionResponseStatus.InvalidArgs, $"Invalid boolean input: {error}");
// . . .
useLocalPosition = flags[0];
useLocalRotation = flags[1];
```

/// tip | Continuous vs Non-continuous arguments
If the parameters you want to parse are non-continuous you can pass a collection expression instead of an array span.  
Below are 3 common examples of how we might want to pass arguments.

```csharp
// Args index 0, 2 and 4. Skip all others
ArgumentParser.TryParseManyBool([args[0], args[2], args[4]], out bool[] values, out error)

// All arguments
ArgumentParser.TryParseManyBool(args.AsSpan(), out bool[] values, out error)

// Arguments from index 0 to index 2
ArgumentParser.TryParseManyBool(args.AsSpan(0, 2), out bool[] values, out error)
```
///

### TryParseManyEnum

**Signature**: `public static bool TryParseManyEnum(ReadOnlySpan<string> args, string[] options, out int[] values, out string error)`

/// details | Parameter details (Click to expand)  
`ReadOnlySpan<string>` : `args`
: Raw input arguments that should be parsed into a `int`.  
Note that the int will represent an index for a certain collection of options.  

: **Example**:  
`["command", "starts_with"]`

`int[]` : **out** `values`
: A strictly typed array of all inputs parsed to `int`s (indexes).  

: **Example** _(using above context)_:  
`[0, 1]`

`string` : **out** `error`
: An error message that is filled if ANY of the input are not valid.  
_`string.Empty` if the input is parsed correctly._

///

**Description**:  
Takes an array of raw input arguments, 
then parses them into a strictly typed array of `int`s that represent option indexes. These can be mapped using a string array.  

All arguments must pass for the method to return successfull.

**Examples**:

_Hypothetical usage within a command that has several enums scattered in the argument signature._  
_In this case, they are implicitly paired with a command that searches for other commands._
```csharp
// . . . 
if (!ArgumentParser.TryParseManyEnum([args[0], args[2]], out int[] flags, out error))
	return new Response<ExecutionResponseStatus>(ExecutionResponseStatus.InvalidArgs, $"Invalid boolean input: {error}");
// . . .
findOfType = ExecutionTypes[flags[0]];
searchMode = SearchModes[flags[1]];
```

/// tip | Continuous vs Non-continuous arguments
If the parameters you want to parse are non-continuous you can pass a collection expression instead of an array span.  
Below are 3 common examples of how we might want to pass arguments.

```csharp
// Args index 0, 2 and 4. Skip all others
ArgumentParser.TryParseManyEnum([args[0], args[2], args[4]], out int[] values, out error)

// All arguments
ArgumentParser.TryParseManyEnum(args.AsSpan(), out int[] values, out error)

// Arguments from index 0 to index 2
ArgumentParser.TryParseManyEnum(args.AsSpan(0, 2), out int[] values, out error)
```
///