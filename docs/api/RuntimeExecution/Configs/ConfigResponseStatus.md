# ConfigResponseStatus
`public enum ConfigResponseStatus`  

**Inherits**: None  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution.Confi`  

## Description

Defines the response status for when trying to do any type of CRUD action on a config or user config file.  

/// note | Values  
`None` : `0`
: Error fallback for if the enum is set using the `default` keyword. 
**This is not intended to be used as a return status.**  

`Success` : `1`
: Executed expectedly.  

`InvalidArgs` : `2`
: Denied because of bad arguments.  

`NotFound` : `3`
: A required file was not found.  

`FileConflict` : `4`
: A conflicting file exists that prevents the action.  

`Failed` : `5`
: Failed expectedly (IE: Through gameplay context, or internal flags). 

`Error` : `6`
: Failed unexpectedly. 
