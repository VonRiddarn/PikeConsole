# CVarBase&lt;T&gt;
`public abstract partial class CVarBase<T> : Resource, ICVar`  

**Inherits**: [`Resource`](https://docs.godotengine.org/en/stable/classes/class_resource.html#resource)⿻, `ICvar`  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution.Cvars.Extensions`  

## Description

Root class for **ALL** CVars that manages initialization, execution, 
session persistance, cheat protection, registry indexing and formatting.  

It serves both as the entry point for anyone wanting to expand the API with more CVar types, and as a contract to ensure consistent interactions between CVars.  

For a tutorial on how to create a custom CVar type, see [the cvar guide](../../../../guides/cvars.md).

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
| `public` | `string` | [CurrentValueDisplay](#currentvaluedisplay) |
| `public` | `string` | [DefaultValueDisplay](#defaultvaluedisplay) |
| `public` | `string` | [Description](#description_1) |
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
| `protected virtual` | `void` | [InitializeInternal](#initializeinternal) |
| `public` | `void` | [ResetValue](#resetvalue) |
| `public` | `Response<ExecutionResponseStatus>` | [Execute](#execute) |
| `protected abstract` | `Response<CvarSetResponseStatus>` | [SetValue](#setvalue) |
| `public` | `string` | [GetHelp](#gethelp) |
| `public virtual` | `string` | [DisplayValue](#displayvalue) |

## Event Descriptions  

### ValueChanged
Called when the value has been changed.  
Passes the new value as an argument to the consumer method.

_This is useful for updating information using the observer pattern._  

**Example(s)**:
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

**Example(s)**:
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

---

### IsModified
A shorthand for checking if the current value is not the original, expected value.  
This can be used by saving systems to occlude default CVars, which 
allows for delta-configurations (only save what has been changed).

_Behind the scenes, this is the full signature._
```csharp
public bool IsModified => !EqualityComparer<T>.Default.Equals(_value, _defaultValue);
```

---

### FormattedValue
Shorthand that displays the formatted value of a CVar.  
It does so by utilizing the `virtual` method [DisplayValue](#displayvalue).  

---

### Persist
This is a _Editor facing_ flag that decides whether or not the CVar 
should **persist between sessions**.  

By default this only allows the CVar to be saved in the [PersistentCVarRegistry](../../PersistentCVarRegistry.md). 
From there, one may opt-in for the built in userconfig `.cfg` system, 
or build their own using the `PersistentCVarRegistry` api.

---

### IsCheat
This is a _Editor facing_ flag that decides whether or not the CVar **is considered a cheat**.  

CVars marked as cheats may only be 
edited by the system. Players are unable to edit them without entering cheatmode.  

**See also**: [ExecutionSource](../../RuntimeExecution/ExecutionSource.md)

---
### CurrentValueDisplay
Shorthand property for getting the user-facing (UI-friendly) format of the current value. Used by the `ConsoleFormatter` to structure help messages.  

/// caution
CurrentValueDisplay shoud never be overridden directly by CVar variants.  
Instead, they override the [DisplayValue](#displayvalue) method provided by.
///

---
### DefaultValueDisplay
Shorthand property for getting the user-facing (UI-friendly) format of the current value. Used by the `ConsoleFormatter` to structure help messages.  

/// caution
DefaultValueDisplay shoud never be overridden directly by CVar variants.  
Instead, they override the [DisplayValue](#displayvalue) method provided by.
///

---

### Description
This is a _Editor facing_ flag that **sets the description** of the CVar.  

This description is used by the [LongDesc](#longdesc) prefaces the [DescriptionInternal](#descriptioninternal).  
It can be arbitrarily long or short and serves as a description for the specific **CVar resource**, rather than the _CVar type_.  

**Example(s)**:
```
Crosshair length variable. 
Used by the CrosshairManager when rendering the crosshair on screen.
```

---

### Signature
Fully automatic _"command"_ signature for the CVar.  
Each CVar automatically registers themselves to the `RuntimeExecutableRegistry` as an executable with the resource filename as the signature.  

/// note
Signatures are automatically parsed at runtime to ensure no spaces or trailing whitespaces exists.  
Trailing whitespaces are trimmed, and spaces are replaced with underscores.
///

---

### ShortDesc
Handled automatically by the root class (this).  
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

Details about what the command does comes from the help formatter and [long description](#longdesc).
///

---

### LongDesc
Handled automatically by the root class (this).  
`LongDesc` is the canonical long description of any executable and 
is hardwired for CVars to return both the [resource description](#description_1) and 
the [internal description](#descriptioninternal).  


_Behind the scenes, this is the full signature._
```csharp
public string LongDesc => $"{Description}\n{DescriptionInternal}";
```

---

### DisplayType
This is the UI-friendly name of the type and is used to display the resource type in the console and logs. It is not designed to be parseable. However, all default CVars included in PikeConsole follow the strict naming pattern: `CVar_Type`

**Example(s)**:  
```csharp
// Excerpt from CVarEnum.cs
public override string DisplayType => "CVar_Enum";

// Excerpt from CVarBool.cs
public override string DisplayType => "CVar_Bool";

// Excerpt from CVarInt.cs
public override string DisplayType => "CVar_Int";

// . . .
```

---

### Usage
This is the UI-friendly, human readable usage instructions for the CVar.  
Ideally it is kept short and concise for better parsing with the help command, but there is no arbitrary length limit.  

By default it's formatted as `$"{Signature} [new value]"` for all CVars, but is overrideable for advanced extentions.

_Behind the scenes, this is the full signature._
```csharp
public virtual string Usage => $"{Signature} [new value]";
```

---

### _defaultValue
The de-facto default value of the CVar resource instance.  
This is set in the Godot editor by the designer.  

CVar resources are automatically set to their default value when initialized or reset using the [ResetValue](#resetvalue) method.  

/// note
If you are using the user configuration system the values are first set to their default values, _and then_ overridden by the user profile initializer at startup.
///

---

### _value
The de-facto current value of the CVar resource instance.  
Unless the [Persist](#persist) flag is set (and the user configuration system is active), this value is per-session scoped. Closing and opening the runtime will reset the value to its default automatically.

---

### DescriptionInternal
The META description for the CVar type.  
The structure of this is fully arbitrary and just serves as an entry point for advanced extentions to list critical typ information.  

**Example(s)**:  

_excerpt from `CVarEnum.cs`._

```csharp
protected override string DescriptionInternal => _cachedHelpLst;

protected override void InitializeInternal()
{
	// . . .
	StringBuilder sb = new("OPTIONS:\n");
	for (int i = 0; i < _options.Length; i++)
		sb.Append($"\t{i} = {_options[i]}\n");

	_cachedHelpLst = sb.ToString();
}

```

---

## Method Descriptions  

### Initialize

**Signature**: `public void Initialize()`

/// note | Takes no parameters 
///

**Description**:  
Method used to add the CVar into the `RuntimeExecutableRegistry` and `PersistentCVarRegistry`. This is called automatically by the `CVarCrawler` inside the addons autoloader.  

This method cannot be overridden. To extend the initializsation logic, see [InitializeInternal](#initializeinternal).   

/// warning
If CVars are used as intended, this method never has to be called by any user of this framework. It is called autoamtically by the `CVarCrawler` for all CVar resources in the designated CVar directory and all of its child directories.  

The only exception is for internal system initializations where CVars are defined outside of the designated CVar folder, as is the case with PikeConsoles cheatmode CVar and others located at `addons/PikeConsole/Config/variables`.
///

**Example**:

_Excerpt from `CVarCrawler.cs`._
```csharp
// . . .
Resource loadedResource = ResourceLoader.Load(fullPath);

if (loadedResource is ICVar cvar)
{
	cvar.Initialize();
}
```
---

### InitializeInternal

**Signature**: `protected virtual void InitializeInternal()`

/// note | Takes no parameters 
///

**Description**:  
Method used to extend initialization logic and apply things like internal caching or fetching. This is called automatically at the end of [Initialize](#initialize), which itself is automatically called by the `CVarCrawler`autoload at runtime initialization. 

**Example**:

_Excerpt from `CVarEnum.cs`._
```csharp
// At CVar initialization, pre-cache all options to 
// improve runtime performance and text processing speed
protected override void InitializeInternal()
{
	// . . .
	StringBuilder sb = new("OPTIONS:\n");
	for (int i = 0; i < _options.Length; i++)
		sb.Append($"\t{i} = {_options[i]}\n");

	_cachedHelpLst = sb.ToString();
}
```
---

### ResetValue

**Signature**: `public void ResetValue()`

/// note | Takes no parameters 
///

**Description**:  
Resets the [value](#_value) of the CVar back to its [default value](#_defaultvalue).  
If the value is a persistent value and the new value is not equal to the old, 
the `PersistentCVarRegistry` sends an update event that is consumed by the user configuration storage (if it's enabled). 

**Example**:

_Hypothetical method that resets all user settings in a GUI within an optional scope._
```csharp
public void ResetAllSettings(string scope = string.Empty)
{
	foreach(ICVar setting in allSettings)
	{
		if(string.IsNullOrWhitespace(scope))
			setting.ResetValue();
		else if(setting.Signature.StartsWith(scope))
			setting.ResetValue();
	}
}
```

---

### Execute

**Signature**: `public Response<ExecutionResponseStatus> Execute(ExecutionSource executionSource, string[] args)`

/// warning
CVars should not be set through code using the execution method unless it is necessary and used with intention. This method is used to map the CVar value to the `RuntimeExecutionSystem` and is mainly invoked through the means of a console or config file.  

To set a CVar from code you should always manage the [Value](#value) property directly. 

When making a custom runtime console [StatementExecutor](../../RuntimeExecution/StatementExecutor.md) should be used as the definitive entry point!

**Example**:  
```csharp
// This automatically runs like a system user and runs potential persistance events.
_gravityModifierCVar.Value = 800f;

```
///

/// details | Parameter details (Click to expand)  
[ExecutionSource](../../RuntimeExecution/ExecutionSource.md) : `executionSource`
: The definite caller of this execution. Could be a player through the console / player config file, or the game itself through internal systems. 
**Example**: `ExecutionSource.Player`

`string[]` : `args`
: The unparsed arguments passed to the CVar. If they end with the ram only flag, the flag is read and stripped before passing the arguments into the [SetValue](#setvalue) method.

: **Example**:  
`["800", "ram_only"]`
_In this example SetValue will be called using: `SetValue(["800"])`._  
_Since `ram_only` was used the value will not persist._
///

**Description**:  
API entry point that makes CVars agnostically executable from outside systems.  
This is mainly used by the [StatementExecutor](../../RuntimeExecution/StatementExecutor.md) and should not be regularly used by users of this framework. 

**Example**:

_Excerpt from `StatementExecutor.cs`._  
```csharp
// . . .
if (RuntimeExecutableRegistry.TryGetExecutable(signature, out IRuntimeExecutable executable))
{
	Response<ExecutionResponseStatus> response = executable.Execute(executionSource, args);
	// . . . 
}
```
---

### SetValue

**Signature**: `protected abstract Response<CvarSetResponseStatus> SetValue(ReadOnlySpan<string> args);`

/// details | Parameter details (Click to expand)  
`ReadOnlySpan<string>` : `args`
: The argument string array. Passed as `ReadOnlySpan<string>` for performance and mutability safety.  

**Example**: `["800"]`

///

**Description**:  
This method handles the actual parsing and setting of data when calling the [Execute](#execute) method.  
It is overridden within each CVar resource type and serves as the definite ruleset for what arguments are parsable.  

/// tip 
This method is best combined with the [ArgumentParser](../../RuntimeExecution/ArgumentParser.md) for 
the best results and developer experience.
///


**Example(s)**:

_Excerpt from `CVarEnum.cs`._  
_Note that the value is set using the [Value](#value) property. The value MUST be set within the method._

```csharp
public override Response<CvarSetResponseStatus> SetValue(ReadOnlySpan<string> args)
{
	if (!ArgumentParser.ValidateCount(args, 1, out string error))
		return new(CvarSetResponseStatus.InvalidArgs, error);

	if (!ArgumentParser.TryParseEnum(args[0], _options, out int index, out error))
		return new(CvarSetResponseStatus.Failed, error);

	if (Value == index)
		return new(CvarSetResponseStatus.NoChange, null);

	Value = index;
	return new(CvarSetResponseStatus.Success, null);
}
```

---

### GetHelp

**Signature**: `public string GetHelp()`

/// note | Takes no parameters 
///

**Description**:  
Returns a human readable string with any help regarding the command by 
automatically interpolating all information about the `IRuntimeExecutable`.  

The primary consumer of this method is the [default command `help`](../../../guides/default_commands.md#help), which prints information about runtime executables into the runtime console.

/// details | Extended knowledge
	type: tip
All `IRuntimeExecutables` get their help formatted from the `ConsoleFormatter`, like so:  
```csharp
	public string GetHelp() => ConsoleFormatter.FormatHelp(this);
```

This is an agressively inlined method that standarizes the text and outputs it the same no matter the caller.  
Behind the scenes, this is the actual method being called:  

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static string FormatHelp(IRuntimeExecutable rte)
{
	return $"Signature: {rte.Signature}\nIs cheat: {rte.IsCheat}\nDescription: {rte.ShortDesc}\nType: {rte.DisplayType.ToUpper()}\nUsage: {rte.Usage}\n{rte.LongDesc}";
}
```
///

**Example(s)**:

_Usage within a CVar._
```csharp
[Export] CVarFloat _gravityModifierCVar;

protected override void _EnterTree() => 
	PikeConsole.Log(LogTarget.All, $"{_gravityModifierCVar.GetHelp()}");
```

---

### DisplayValue

**Signature**: `public virtual string DisplayValue(T value)`

/// details | Parameter details (Click to expand)  
`T` : `value`
: Any value of type T (the same as this CVar resource instance).  
**Example**: `["400"]`

///

**Description**:  
Takes any value of type `T` and returns a human readable / UI friendly representation for that value.  

This method is used to parse othervise complex or unreadable data into something that would make sense when reading the value back. Such as the case with enums or vectors.


**Example(s)**:

_Excerpt from `CVarEnum.cs`._
```csharp
public override string DisplayValue(int value) => 
	$"{value} ({_options[value]})";
```

This results in an output like: 
```
1 (Medium)
```

_Excerpt from `CVarColor.cs`._
```csharp
public override string DisplayValue(Color value) 
	=> $"({value.R8}, {value.G8}, {value.B8}, {value.A8}) | #{value.ToHtml()}";
```

For the color green, this results in an output like: 
```
(0, 255, 0, 255) | #00ff00ff
```

---