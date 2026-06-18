# ParsedStatement
`public readonly struct ParsedStatement(string signature, string[] arguments)`  

**Inherits**: None  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution`  

## Description

Holds the data for 1 statement.  
Mainly returned by the [StatementParser](../RuntimeExecution/StatementParser.md) and consumed by the [StatementExecutor](../RuntimeExecution/StatementExecutor.md).  

/// note | Parameters  
`string` : `Signature`
: Command signature.

: **Example**:  
`echo`

`string[]` : `Arguments`
: Command arguments.

: **Example**:  
`["Hello", "world!"]`
...