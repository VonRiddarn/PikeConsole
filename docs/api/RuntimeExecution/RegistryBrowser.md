# RegistryBrowser
`public static class RegistryBrowser`  

**Inherits**: None  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution`  

## Description

Wrapper method for searching through the Runtime and Alias registries.  
Contains helper methods that automatically filters commands and cvars from the shared runtime execution registry.  

/// note
It is recommended to always use the RegistryBrowser rather than contacting the registries direcctly to query for certain executables or signatures. 
///

## Properties  
No public properties to showcase for this class.

## Methods
| Scope | Return | Name |
|-------|--------|------|
| `public` | [`IRuntimeExecutable[]`](../RuntimeExecution/IRuntimeExecutable.md) | [FindExecutables](#findexecutables) |
| `public` | [`Command[]`](../RuntimeExecution/IRuntimeExecutable.md) | [FindCommands](#findcommands) |
| `public` | [`ICVar[]`](../RuntimeExecution/ICvar.md) | [Execute](#execute) |
| `public` | `void` | [Execute](#execute) |
| `public` | `void` | [Execute](#execute) |
| `public` | `void` | [Execute](#execute) |


## Method Descriptions  

### FindExecutables

**Signature**: `public static IRuntimeExecutable[] FindExecutables(string term, SearchMode searchMode, bool rankByPrefix)`

/// details | Parameter details (Click to expand)  
`string` : `term`
: The search term to use for the query.

[SearchMode](../RuntimeExecution/SearchMode.md) : `searchMode`
: The search mode to use when filtering the results. 

`bool` : `rankByPrefix`
: If set to true, commands will be ordered depending on if they start with the term. In short: Prefix matches get priority over generic matches.

///

**Description**:  
Uses the search term to find matching runtime executables.

**Returns**  
An array of [IRuntimeExecutable](../RuntimeExecution/IRuntimeExecutable.md) objects that are the results of the query.


**Example(s)**:

_Excerpt showing the implementation of the `find` command, which depends on the RegistryBrowser._
```csharp
Command(
	"find",
	"Lists all comands and CVars with an optional search term.",
	null,
	"find [term?]",
	false,
	static (args) => {
		string term = string.Join(' ', args);
		var rtes = RegistryBrowser.FindExecutables(term, SearchMode.Contains, true);
		return FormatAndLogResults(rtes, term, "results");
	}
),
```
---

### FindCommands

**Signature**: `public static Command[] FindCommands(string term, SearchMode searchMode, bool rankByPrefix)`

/// details | Parameter details (Click to expand)  
`string` : `term`
: The search term to use for the query.

[SearchMode](../RuntimeExecution/SearchMode.md) : `searchMode`
: The search mode to use when filtering the results. 

`bool` : `rankByPrefix`
: If set to true, commands will be ordered depending on if they start with the term. In short: Prefix matches get priority over generic matches.

///

**Description**:  
Uses the search term to find matching commands.

**Returns**  
An array of [Command](../RuntimeExecution/IRuntimeExecutable.md) objects that are the results of the query.


**Example(s)**:

_Excerpt showing the implementation of the `find_command` command, which depends on the RegistryBrowser._
```csharp
Command(
	"find_command",
	"Lists all comands with an optional search term.",
	null,
	"find_command [term?]",
	false,
	static (args) => {
		string term = string.Join(' ', args);
		var rtes = RegistryBrowser.FindCommands(term, SearchMode.Contains, true);
		return FormatAndLogResults(rtes, term, "commands");
	}
),
```
---

### FindCVars

**Signature**: `public static ICVar[] FindCVars(string term, SearchMode searchMode, bool rankByPrefix)`

/// details | Parameter details (Click to expand)  
`string` : `term`
: The search term to use for the query.

[SearchMode](../RuntimeExecution/SearchMode.md) : `searchMode`
: The search mode to use when filtering the results. 

`bool` : `rankByPrefix`
: If set to true, commands will be ordered depending on if they start with the term. In short: Prefix matches get priority over generic matches.

///

**Description**:  
Uses the search term to find matching CVars.

**Returns**  
An array of [ICvar](../RuntimeExecution/ICvar.md) objects that are the results of the query.


**Example(s)**:

_Excerpt showing the implementation of the `find_cvar` command, which depends on the RegistryBrowser._
```csharp
Command(
	"find_cvar",
	"Lists all CVars with an optional search term.",
	null,
	"find_cvar [term?]",
	false,
	static (args) => {
		string term = string.Join(' ', args);
		var rtes = RegistryBrowser.FindCVars(term, SearchMode.Contains, true);
		return FormatAndLogResults(rtes, term, "cvars");
	}
),
```
---

### FindAliases

**Signature**: `public static Alias[] FindAliases(string term, SearchMode searchMode, bool rankByPrefix)`

/// details | Parameter details (Click to expand)  
`string` : `term`
: The search term to use for the query.

[SearchMode](../RuntimeExecution/SearchMode.md) : `searchMode`
: The search mode to use when filtering the results. 

`bool` : `rankByPrefix`
: If set to true, aliases will be ordered depending on if they start with the term. In short: Prefix matches get priority over generic matches.

///

**Description**:  
Uses the search term to find matching aliases.

**Returns**  
An array of `Alias` structs that are the results of the query.  
The `Alias` struct contains only a `signature` and a `statement`.  
`Statements` can be passed to the [StatementExecutor](../RuntimeExecution/StatementParser.md) for further actions.


**Example(s)**:

_Excerpt showing the implementation of the `alias_list` command, which depends on the RegistryBrowser._
```csharp
Command(
	Signature("list"),
	"Lists all aliases with an optional search term.",
	null,
	$"{Signature("list")} [..term?]",
	false,
	static (args) => {
		string term = string.Join(' ', args);
		var aliases = RegistryBrowser.FindAliases(term, SearchMode.Contains, true);

		if (aliases.Length < 1)
			return new(ExecutionResponseStatus.Success, string.IsNullOrWhiteSpace(term) ? $"No aliases found." : $"No aliases found matching \"{term}\".");

		string header = string.IsNullOrWhiteSpace(term) ? $"Showing all aliases..." : $"Showing aliases matching \"{term}\"...";

		StringBuilder sb = new(header);

		foreach (var alias in aliases)
			sb.Append($"\n\n[Alias] {alias.Signature}\n\t\"{alias.Statement}\"");

		PikeLogger.Log(LogTarget.Runtime, $"{sb.ToString()}");

		return new(ExecutionResponseStatus.Success, null);
	}
),
```
---
