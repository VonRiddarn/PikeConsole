# PikeConsole

Proprietary Godot CVar and Command console system for Fractal Pike.

## IDE Warnings

Due to the namespace configuration some IDEs will complain about not including the addons folder.  
Since we are using Domain Driven Design it is safe to just disable the warning for this project.  
`.csproj`

```xml
<Project Sdk="Godot.NET.Sdk/4.6.3">
  <PropertyGroup>
	. . .
	<NoWarn>$(NoWarn);IDE0130</NoWarn> // Add this!
  </PropertyGroup>
</Project>
```

You can also disable it throug `.editorconfig`

```cfg
dotnet_diagnostic.IDE0130.severity = none
```

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
