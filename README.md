# PikeConsole

Proprietary Godot CVar and Command console system for Fractal Pike.

## TODO:

- Refine readme
-   - Check spelling and formatting
-   - Add quickstart guide
-   - Add eaxamples at each context
- Map out system using diagrams
- Add branding folder
- Add benchmarks
-   - Raw allocation
-   - Allocation + Interop bridge cross (Most realistic)
-   - Killswitch on / off scenarios

_NOTE TO SELF: Make sure to add the benchmarks AFTER the UI subscriber console is added._
_This makes the benchmark more honest in terms of what plug-and-play users can expect._  
_For powerusers, add 2 tables comparing Synthetic (raw logger, no UI or listener) vs End-to-end (logger + UI overhead)._

## Features

### Runtime console

A runtime console accessible through a keyboard shortcut **Default: `en:semicolon` `se:ö`**

### Routed logs

Logs can be routed to `Debug`, `Runtime` or `All`.

#### High efficiency architecture

##### Log as struct references

All logs sent from log event are sent as references and consumed using the `in`keyword.  
Meaning a near infinite number of subscribers can exist on the `LogEmitted` event with minimal overhead.

#### AOT (Ahead of time) compilation

The system does not make use of reflection to register commands or resources.

##### No alloc, no-op logger

The system is structured to be included in runtime builds.  
Therefore if logs are called on non-target systems they are non-alloc and no-op.

##### Killswitch

Should the player not wish to see logs, or want to save the small overhead of the console there exists a runtime killswitch.

#### Domain filtering

Logs can be tagged with a domain for easy filtering in subscribing systems.

### CVars (Console variables)

CVars can be created with a simple right-click menu inside the Godot editor.  
Once a CVar is created they are loaded at runtime (without reflection!) and can be accessed at O(1) complexity.

### Commands

Commands use a minimal-overhead, beginner friendly context based command-set architecture API.  
Sets are registered by adding a CommandSet node to the object and inheriting from the root class.  
Commands are managed at runtime and update at O(1) complexity with no reflection!

### Clickable source paths (Godot editor)

When using `PikeLogger.LogWarning(...)` or `PikeLogger.Error(...)` the log is automatically prefixed with a clickable link.  
Clicking this will open up the callers file and line in your default IDE (engine settings).

## Requrements

- **Godot 4.6 or later**
- **C# 10.0 or later**

## Privacy notice!

This Logger uses compile-time directory baking to display certain paths in logs.  
This results in super fast, interned strings - but it bakes your folder structure into the game!  
If you care about not leaking your file system structure, edit your `.csproj` with the `<PathMap>` element:

```xml
<Project Sdk="Godot.NET.Sdk/4.6.3">
  <PropertyGroup>
	. . .
	<PathMap>$(MSBuildProjectDirectory)=/PikeConsole</PathMap> // Add this
  </PropertyGroup>
</Project>
```

This replaces your project origin `\users\vonriddarn\Godot\Projects\MyProject` (example) with the new defined filepath `/PikeConsole` (example).

### Important note if you edit your PathMap!

If you update the PathMap to something else, like `/PikeConsole` or whatever, you MUST update `PikeConsole/PikeConsoleConfig.cs` to include this alias!  
Like so:

```csharp
	public const string PATH_MAP_ALIAS = "/PikeConsole";
```

If you fail to do this, your log paths will **no longer be clickable**.

## Solving library collisions

Many utilities use standarized names. Therefore there is a slight collision chance with other libraries.  
To avoid collisions, use a using-alias:

```csharp
using FractalPike.PikeConsole.Core.Logging;

// Using alias that tells the compiler "Use PikeConsoles utility for LogTarget"
using LogTarget = FractalPike.PikeConsole.Core.Logging.LogTarget;

// Wow, it works!
PikeLogger.Log(LogTarget.Runtime, $"Hello world!");
```

## Why are interpolated strings enforced for the logger?

Because the system uses a custom stringbuilder to enforce as close to zero allocation as possible.  
If the current environment does not match the target environment, the interpolated string will not be built and its parts not read.

Since there is no real way (that I know of) to efficiently separate concatenated string from string literals,
I made the decision to just not allow them at all. This is because even though a string literal is safe and will trigger string interning,
a concantenated string is not and will allocate and execute.

**Example**

```csharp

// THIS IS WHAT YOU USE
// This will use the custom LogInterpolatedStringHandler and thus only allocate and compile on Debug builds.
// EVEN THOUGH we are using dynamic data.
PikeLogger.Log(LogTarget.Debug, $"Hello, {Player.Name}!");

// THIS WILL NOT COMPILE
// This is a static string literal. This is safe and will compile to an interned string reference.
PikeLogger.Log(LogTarget.Debug, "Hello, VonRiddarn!");

// THIS WILL NOT COMPILE
// THIS WILL NOT! This will allocate and potentially even run on undesired environments.
// We could solve this with a Func<string> override, but that too would need some allocation and require developers to actually use it.
PikeLogger.Log(LogTarget.Debug, "Hello, " + Player.Name + "!");
```

In the example above, since it is (again, to my knowledge) not feasible to differ the last two statements from eachother efficiently, I allow neither.
