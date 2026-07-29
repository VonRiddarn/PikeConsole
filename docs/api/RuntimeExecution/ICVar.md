# ICVar

`public interface ICVar : IRuntimeExecutable`  

**Inherits**: [IRuntimeExecutable](../RuntimeExecution/IRuntimeExecutable.md)  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution`  

## Description

The definitive contract that decides what a CVar should contain. Please note that this is **not** meant to be used on each individual CVar type. Instead, the [CVarBase&lt;T&gt;](../CVars/Extensions/CVarBase_T.md) manages all internal logic, and individual CVars just inherit from that.  

In short: This is used by PikeConsole to determine something is in fact a CVar, but it is not applied explicitly to each CVar, it is inherited.

## Properties  
| Scope | Return | Name |
|-------|--------|------|
| `public` | `bool` | [Persist](#persist) |
| `public` | `bool` | [IsModified](#ismodified) |
| `public` | `string` | [CurrentValueDisplay](#currentvaluedisplay) |
| `public` | `string` | [DefaultValueDisplay](#defaultvaluedisplay) |
| `public` | `string` | [FormattedValue](#formattedvalue) |

## Methods
| Scope | Return | Name |
|-------|--------|------|
| `public` | `void` | [Initialize](#initialize) |
| `public` | `void` | [ResetValue](#resetvalue) |

## Property Descriptions

### Persist

**Signature**: `public bool Persist { get; }`

**Description**:  
Boolean flag used to mark cvars that should persist between sessions.  
Flagging a CVar as persistent automatically tracks it in the [PersistentCVarRegistry](../CVars/PersistentCVarRegistry.md).  

In order to actually save the values the user must either use the built in PikeConsole user-config system (recomended), or create their own save system for the CVars.  

///tip
To activate the PikeConsole user configurations systsem, go to:  
`Project` > `Project settings (General tab)` > `FractalPike` > `PikeConsole`  
And enable the setting `Use user configs`.  

_Note: If you can't find the settings, make sure advaced settings is turned on._  
///

---

### IsModified

**Signature**: `public bool IsModified { get; }`

**Description**:  
Shorthand property to check if the current value of the CVar `is not` the default value. By default this is used by the user config save system for delta-settings. Meaning values that are unmodified does not get added to the player settings.

---
### CurrentValueDisplay

**Signature**: `public string CurrentValueDisplay { get; }`

**Description**:  
Shorthand property for getting the user-facing (UI-friendly) format of the current value. Used by the `ConsoleFormatter` to structure help messages.  

/// caution
CurrentValueDisplay shoud never be overridden directly by CVar variants.  
Instead, they override the `DisplayValue` method provided by [CVarBase&lt;T&gt;](../CVars/Extensions/CVarBase_T.md).
///

---
### DefaultValueDisplay

**Signature**: `public string DefaultValueDisplay { get; }`

**Description**:  
Shorthand property for getting the user-facing (UI-friendly) format of the current value. Used by the `ConsoleFormatter` to structure help messages.  

/// caution
DefaultValueDisplay shoud never be overridden directly by CVar variants.  
Instead, they override the `DisplayValue` method provided by [CVarBase&lt;T&gt;](../CVars/Extensions/CVarBase_T.md).
///

---
### FormattedValue

**Signature**: `public string FormattedValue { get; }`

**Description**:  
The value of the CVar in it's formatted state.  
This value **MUST** be passable to the `SetValue` method within [CVarBase&lt;T&gt;](../CVars/Extensions/CVarBase_T.md). Spaces in the formatted value will become separate arguments.

**Example(s)**:  

_Excerpt from `CVarVector2.cs`._
```csharp
public override string FormattedValue => string.Format(
	System.Globalization.CultureInfo.InvariantCulture,
	"{0} {1}",
	_value.X, _value.Y);
```

_Excerpt from `CVarColor.cs`._
```csharp
public override string FormattedValue => $"#{_value.ToHtml()}";
```

---

## Method Descriptions  

### Initialize
**Signature**: `public void Initialize()`

/// note | Parameter details  
No parameters for this method.
///

**Description**:  
This method is for internal and advanced use only.  
It is used by the CVarCrawler in otder to initialize the CVars into the runtime and persistent registries.  

It is also used by internal systems to initialize CVars from within the addon directory (outside the designated CVar folder).  

///warning
It is highly recommended to __ignore this method__ and only initialize CVars using the designated CVar directory.
///

### ResetValue
**Signature**: `public bool ResetValue(ExecutionSource executionSource, bool ramOnly = false)`

/// details | Parameter details (Click to expand)  
[ExecutionSource](../RuntimeExecution/ExecutionSource.md) : `source`
: The entity wanting to execute this statement. Used to determine cheat override authority.

`bool` : `ramOnly`
: If set to true, the cvar will update without triggering an update from the persistent CVar registry.

///

**Description**:  
Resets the value of the CVar back to whatever is set as the default value.  
If the CVar is persistent, and the command is not run in "ramonly" mode, the CVar will be cleared from the user config.