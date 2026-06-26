# CVarBase&lt;T&gt;
`public abstract partial class CVarBase<T> : Resource, ICVar`  

**Inherits**: [Resource (External link)](https://docs.godotengine.org/en/stable/classes/class_resource.html#resource), `ICvar`  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution.Cvars.Extensions`  

## Description

Root class for **ALL** CVars that manages initialization, execution, 
session persistance, cheat protection, registry indexing and formatting.  

It serves both as the entry point for anyone wanting to expand the API with more CVar types, and as a contract to ensure consistent interactions between CVars.  

For a tutorial on how to create a custom CVar type, see [the cvar guide](../../../guides/cvars.md).

## Events  
| Scope | Delegate | Name |
|-------|--------|------|
| `public` | `Action<T>?` | [ValueChanged](#valuechanged) |
| `public` | `Action?` | [ValueInvalidated](#valueinvalidated) |

## Properties  
| Scope | Return | Name |
|-------|--------|------|
| `public` | `T` | [Value](#value) |
| `public` | `bool` | [IsModified](#ismodified) |
| `public` | `string` | [FormattedValue](#formattedvalue) |
| `public` | `bool` | [Persist](#persist) |
| `public` | `bool` | [IsCheat](#ischeat) |
| `public` | `string` | [Description](#description) |
| `public` | `string` | [Signature](#signature) |
| `public` | `string` | [ShortDesc](#shortdesc) |
| `public` | `string` | [LongDesc](#longdesc) |
| `public abstract` | `string` | [DisplayType](#displaytype) |
| `public virtual` | `string` | [Usage](#usage) |
| `protected abstract` | `T` | [_defaultValue](#_defaultvalue) |
| `protected abstract` | `T` | [_value](#_value) |
| `protected virtual` | `string` | [DescriptionInternal](#descriptioninternal) |

## Methods
| Scope | Return | Name |
|-------|--------|------|
| `public` | `void` | [Initialize](#initialize) |
| `protected virtual` | `void` | [InitializeInternal](#initializeInternal) |
| `public` | `void` | [ResetValue](#resetvalue) |
| `public` | `Response<ExecutionResponseStatus>` | [Execute](#execute) |
| `public abstract` | `Response<CvarSetResponseStatus>` | [SetValue](#setvalue) |
| `public` | `string` | [GetHelp](#gethelp) |
| `public virtual` | `string` | [DisplayValue](#displayvalue) |

## Event Descriptions  

### ValueChanged
Called when the value has been changed.  
Passes the new value as an argument to the consumer method.

_This is useful for updating information using the observer pattern._  
**Example**:
```csharp

DifficultyCVar.ValueChanged += OnDifficultyChanged;

void OnDifficultyChanged(int newDifficulty)
{
	GameManager.Difficulty = newDifficulty;
}

```

### ValueInvalidated
Called when the value has been changed.  
Does NOT pass a value to the consumer method.  

_This is useful when several CVars share execution method._  
**Example**:
```csharp

CrosshairLengthCVar.ValueInvalidated += OnCrosshairChanged;
CrosshairThicknessCVar.ValueInvalidated += OnCrosshairChanged;
CrosshairColorCVar.ValueInvalidated += OnCrosshairChanged;

void OnCrosshairChanged()
{
	CrosshairManager.ReRender(
		CrosshairLengthCVar.Value,
		CrosshairThicknessCVar.Value,
		CrosshairColorCVar.Value
	);
}

```


## Property Descriptions  

### Value
The current value.  
Automatically handles value comparison and event triggering.  
[ValueChanged](#valuechanged) and [ValueInvalidated](#valueinvalidated) are only 
called if the new value is not equal to the old value.  

This saves performance on event invokation overhead.

### IsModified
A shorthand for checking if the current value is not the original, expected value.  
This can be used by saving systems to occlude default CVars, which 
allows for delta-configurations (only save what has been changed).

_Behind the scenes, this is the full signature._
```csharp
public bool IsModified => !EqualityComparer<T>.Default.Equals(_value, _defaultValue);
```

### FormattedValue
Shorthand that displays the formatted value of a CVar.  
It does so by utilizing the `virtual` method [DisplayValue](#displayvalue).  

### Persist
This is a _Editor facing_ flag that decides whether or not the CVar 
should **persist between sessions**.  

By default this only allows the CVar to be saved in the [PersistentCVarRegistry](../PersistentCVarRegistry.md). 
From there, one may opt-in for the built in userconfig `.cfg` system, 
or build their own using the `PersistentCVarRegistry` api.

### IsCheat
This is a _Editor facing_ flag that decides whether or not the CVar **is considered a cheat**.  

CVars marked as cheats may only be 
edited by the system. Players are unable to edit them without entering cheatmode.  

**See also**: [ExecutionSource](../../RuntimeExecution/ExecutionSource.md)

### Description
This is a _Editor facing_ flag that **sets the description** of the CVar.  

This description is used by the [LongDesc](#longdesc) prefaces the [DescriptionInternal](#descriptioninternal).  
It can be arbitrarily long or short and serves as a description for the specific **CVar resource**, rather than the _CVar type_.  

**Example**:
```
Crosshair length variable. 
Used by the CrosshairManager when rendering the crosshair on screen.
```

### Signature
Fully automatic _"command"_ signature for the CVar.  
Each CVar automatically registers themselves to the `RuntimeExecutableRegistry` as an executable with the resource filename as the signature.  

/// note
Signatures are automatically parsed at runtime to ensure no spaces or trailing whitespaces exists.  
Trailing whitespaces are trimmed, and spaces are replaced with underscores.
///

### ShortDesc
Handled automatically by the root class.  
`ShortDesc` is a required property for all `IRuntimeExecutable`s and 
is used by help formatters and executable lists.  

_Behind the scenes, this is the full signature._
```csharp
public string ShortDesc => $"View or set the value of {Signature}";
```

/// note
Not having `ShortDesc` being virtually scoped is intentional.  
When showing help for the executable signature it should be 
explicitly clear that it is a CVar shortcut, and that the command 
only serves and invokes the CVars `Execute` method. 

Details about what the command does comes from the help formatter and long description.
///

### LongDesc


### DisplayType


### Usage


### _defaultValue


### _value


### DescriptionInternal


## Method Descriptions  

### DisplayValue
Displayvalue is a method that converts the value to a readable string. 
This is used by many internal systems to provide a clean and 
consistent experience across CVars.

_A good example of how to use the DisplayValue override is this_ 
_excerpt from `CVarEnum.cs`._

```csharp
public override string DisplayValue(int value) => $"{value} ({_options[value]})";
```

### ValidateCount

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