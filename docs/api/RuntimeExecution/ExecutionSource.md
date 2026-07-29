# ExecutionSource
`public enum ExecutionSource`  

**Inherits**: None  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution`  

## Description

Identifies the caller of a command. Mainly used for bypassing cheatprotection.

/// note | Values  
`Standard` : `1`
: Tells the execution method that the player or any other untrusted system has passed the command through the GUI, CLI or a file.  
If the executable is cheat protected, it will be blocked from running.

`System` : `2`
: Tells the execution method that the system has passed the command through protected means, like gameplay systems or code. If the executable is cheat protected it will run anyway.