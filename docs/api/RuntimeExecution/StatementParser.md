# StatementParser
`public static class StatementParser`  

**Inherits**: None  
**Namespace**: `FractalPike.PikeConsole.Core.RuntimeExecution`  

## Description

Takes unparsed input of any kind and parses it to valid command statements. 
All valid command statements are not necessarily valid commands, just statements that will be accepted 
by the [StatementExecutor](./StatementExecutor.md).  

The parsing is done in a single pass and only allocates when necessary.  

/// warning | Please note!
The `StatementParser` is mostly for internal use. 
Most users can simply rely on the [StatementExecutor](./StatementExecutor.md) instead!  

**No execution happens in the `StatementParser`. Only parsing!**
///

The parser allows:  

- Multiple statements separated by semicolon `;`.  
- Arguments encapsulated within double quotes `"`.  
_Arguments within double qoutes are passed as one argument, even if it contains spaces_  
- Breakout logic using backslash `\`  
- - _Example: `"\"Hello world!\""` will result in `"Hello world!"`._  
- - _Example 2: `"Remember that you can use the escape \\ character"` will result in `Remember that you can use the escape \ character`._  
- - _Note: Statement separators can be escaped. `"Hello \; world!"` will result in `Hello ; world!`._

## Properties  
No public properties to showcase for this class.

## Methods
### Protected
| Scope | Return | Name |
|-------|--------|------|
| `public` | [ParsedStatement](./ParsedStatement.md) | [ParseLine](#parseline) |


## Method Descriptions  

### ParseLine

**Signature**: `public static ParsedStatement[] ParseLine(string input)`

/// note | Parameter details  
`string` : `input`
: The input string to parse into a list of statements.  
This could come from a console line, a weak-link system or a config file.  

///

**Description**:  
Takes a raw input and parses it into valid statement profiles.  
These statement profiles are returned as an array.  

**Example**:  
`echo Hello World!; echo "Hello back at you!"; ; ; ; count "one argument" two "three and not four"`

_Results in_:
```csharp
[
	{command: "echo",	args: ["Hello", "world!"]},
	{command: "echo",	args: ["Hello back at you!"]},
	{command: "count",	args: ["one argument", "two", "three and not four"]}
]
```

/// tip | Handling Separators
The parser is designed to be resilient. Multiple consecutive semicolons (; ; ;) or leading/trailing whitespace are automatically ignored and do not produce empty commands or arguments!
///
---
