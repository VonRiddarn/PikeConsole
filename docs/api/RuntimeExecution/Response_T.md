# Response&lt;T&gt;
`public readonly struct Response<T>(T status, string message = "") where T : Enum`  

**Inherits**: None  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution`  

## Description

Holds a status return type alongside an optional message.  
Should the message be `null`, it will fallback safely to `string.Empty`.

Used to normalize predictable responses across the system.  

/// note | Parameters  
`T` : `Status`
: The status of the response. Can use any enum.

`string` : `Message`
: Optional response message. By default, most internal systems will log non-empty messages and ignore the rest.
...