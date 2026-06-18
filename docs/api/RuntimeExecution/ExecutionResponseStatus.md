# ExecutionResponseStatus
`public enum ExecutionResponseStatus`  

**Inherits**: None  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution`  

## Description

Defines the response status for an executed runtime executable.  
Used by [StatementExecutor](../RuntimeExecution/StatementExecutor.md) to decide upon logging severity.  

/// note | Values  
`None` : `0`
: Error fallback for if the enum is set using the `default` keyword. **This is not intended to be used as a return status.**  

`Success` : `1`
: Execution succeeded with no hickups.

`DeniedCheat` : `2`
: Execution was aborted because the command is cheat protected.  

/// warning | _This is managed automatically, and internally. Regular use of the API should not lead to the usage of `DeniedCheat`._
///

`InvalidArgs` : `3`
: Execution was aborted because the arguments were invalid. This can be anything from a type conversion error, to the amount not matching the expected signature.

`Failed` : `4`
: The execution failed expectedly, most likely through gameplay means.

: **Example**:  
The player tries to run the command `entity_kill 1337` but when the _EnemyManager_ looks for an enemy of that ID it cannot find it.  
This is an expected failure.

`Error` : `5`
: The execution failed unexpectedly, most likely from an exception in the .NET environment.

: **Example**:  
A developer has made a mistake when fetching an item inside an array and triggers an out of bounds error.

/// note
When using the provided [StatementExecutor](../RuntimeExecution/StatementExecutor.md) all executions are wrapped at the execution layer.  

This means it is safe to leave commands without a try-catch block, though it removes a layer of log-control.
///