# SearchMode
`public enum SearchMode`  

**Inherits**: None  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution`  

## Description

Defines what search mode to use when searching for an `IRuntimeExecutable` or alias. Note that contains and startswith uses `O(N log N)` complexity when used with the [RegistryBrowser](./RegistryBrowser.md) due to how the registry is filtered. This is known, but not considered an issue at this time, as all current usecases are cold-path and shows decent speeds in test environments with 10 000 entries.

/// note | Values  
`Contains` : `0`
: Loose search that simply checks if the query string is present anywhere in the signature.

`StartsWith` : `1`
: Checks if the signature starts with the query string.

`Exact` : `2`
: Checks if the signature exactly (case insensitive) matches the query string. This results in O(1) lookup when using the [RegistryBrowser](./RegistryBrowser.md)
///

---