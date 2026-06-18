# ExecutionSource
`public enum ExecutionSource`  

**Inherits**: None  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution`  

## Description

Identifies the caller of a command. Mainly used for bypassing cheatprotection.

/// note | Values  
`None` : `0`
: Error fallback for if the enum is set using the `default` keyword. **This is not intended to be used as a return status.**  

`Player` : `1`
: Tells the execution method that the player has passed the command through the GUI or CLI.  
If the executable is cheat protected, it will be blocked from running.

`System` : `2`
: Tells the execution method that the system has passed the command through gameplay, code or the file system.  
If the executable is cheat protected, it will be blocked from running.