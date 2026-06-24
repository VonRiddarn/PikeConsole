# CvarSetResponseStatus
`public enum CvarSetResponseStatus`  

**Inherits**: None  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution`  

## Description

Defines the response status for when trying to set a CVar through its execution method.  
This is most often invoked by setting a CVar from the console, or a parsed command.  

Custom CVars are required to have an implementation of the `SetValue` method. 
This method requires the CVar to return a response.  

/// note | Values  
`None` : `0`
: Error fallback for if the enum is set using the `default` keyword. 
**This is not intended to be used as a return status.**  

`NoChange` : `1`
: The current value and the new value are already the same and no change has happened.  
_Internally this allows the [CVarBase&lt;T&gt;](../CVars/Extensions/CVarBase_T.md) to return the method as successfull without invoking any further processing or events._

`Success` : `2`
: A new value has been set with no hickups.  

`InvalidArgs` : `3`
: Execution was aborted because the arguments were invalid. This can be anything from a type conversion error, to the amount not matching the expected signature.

`Failed` : `4`
: The execution failed expectedly, most likely through gameplay means.

: **Example**:  
We use a custom CVar that can reference nodes, 
but the node we are trying to find and reference does not exist.

`Error` : `5`
: The execution failed unexpectedly, most likely from an exception in the .NET environment.

: **Example**:  
A developer has made a mistake when fetching an item inside an array and triggers an out of bounds error.

/// tip
The `CVarBase&lt;T&gt;` automatically wraps the `SetValue` class in a try-catch block.  

This means it is safe to leave the method without catching any errors, though it removes a layer of log-control.
///