# PersistentCVarRegistry

///warning | PLEASE NOTE
The included user config system already listens to this class.  
Unless you are intending to create your own custom save system, you **will not need this**.  

_Note, methods meant for internal use are not documented._
///

`public static class PikeLogger`  

**Inherits**: None  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution`  

## Description

The configuration save router. When a persistent CVar is changed, it will call the update event and send itself as an argument.

## Events  
| Scope | Delegate | Name |
|-------|--------|------|
| `public` | `Action<ICVar>` | [ValueUpdated](#valueupdated) |

## Methods
| Scope | Return | Name |
|-------|--------|------|
| `public` | `void` | [ResetAll](#resetall) |
| `public` | `ImmutableDictionary<string, ICVar>` | [GetSnapshot](#getsnapshot) |

## Event Descriptions  

### ValueUpdated
Called when the update method has been called with a valid CVar (persistent).  
Will pass the updated CVar as an argument (note that CVars are reference types).

## Method Descriptions  

### ResetAll
**Signature**: `public static void ResetAll(bool ramOnly = false)`

/// details | Parameter details (Click to expand)  
`bool` : `ramOnly`
: Flag that dictates if the variables affected by a reset will trigger [`ValueUpdated`](#valueupdated) or not.
///

**Description**:  
Registers a CVar to the registry under its own signature.  

---

### GetSnapshot
**Signature**: `public static ImmutableDictionary<string, ICVar> GetSnapshot()`

/// Note | No parameters
///

**Description**:  
Collects an immutable snapshot of the internal persistent CVar dictionary.  

/// note
The CVars within the dictionary are passed as reference types. The snapshot is is just a snapshot of all currently registered CVars.
///

**Returns**:  
An `ImmutableDictionary<string, ICVar>` where the key is the CVars siganture.

---