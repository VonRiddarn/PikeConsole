# PikeConsole - The modern GoldSrc-inspired console framework!

<p align="center">
	<img src="/docs/_media/proprietary/pike-console-banner.png" alt="PikeConsole Banner"/>
</p>

## 🔥 High performance C# ready for Godot 4.6+

**PikeConsole** is a _plug-and-play_ runtime execution framework that gives you a production-ready, professional-grade debug and command environment.  
You do not have to lift a finger.  
Just install / activate the addon and you get:

- **Zero-alloc routed logging**: Isolated log streams for both debug and runtime environments, with runtime killswitch to protect performance!
- **AOT-safe compilation**: The system is built natively for ahead-of-time compilation which is a MUST if you're porting to console / mobile.
- **Thread safety out of the box**: The backend turnstiles log events safely and the runtime UI consumes them in fast, thread-safe batches.
- **Context-aware commands**: Declarative command sets that know _who_ and _what_ triggered them. Natively mapped to the SceneTree's lifecycle with no reflection.
- **No-code CVar registry**: Console variables based on native Godot resources allows for self-contained data with O(1) lookup!  
  _(And the console variables are fetched automatically at runtime without reflection!)_
- **Clickable logs**: Links in the editor output window that takes you to the caller!
- **The pit of success**: The API is a self-diagnostic system that makes the easiest path the right path. No overcomplicated boilerplate or file bloat.
- **A Fail fast, fail safe net**: When bad data is passed, the system flags the error and falls back on a safe default to prevent runtime crashes.
- **Smart stripping**: The framework is intended to ship with the released version of the game. To prevent file-bloat the system automatically strips itself of editor-related code!

The framework is built with **runtime optimization**, **extensibility** and **ease of use** in mind.  
Thus, everything will work great out of the box - but is also open enough for any average C# developer to extend its features!

## ‼️ Disclaimer

This framework is a port from our Unity project.  
It is actively being worked on right now and currently does not have a frontend UI or the user-CFG system mentioned.  
Once this disclaimer is removed it is safe to assume that everything is functioning as intended.  
The backend (with an exception for the config manager) is fully developed and works though.

If this README reads like som maniacs cork board, it is because it is.  
Until version 1.0 this is more or less a temporary spitball file.  
Most information in here will be moved to the documentation later.

The final, estimated release date for `version 1.0` is by the **end of July, 2026**.

### 🚗 Roadmap (in order)

- [x] Create the CVar startup crawler
- [x] Refactor hardcoded configs to internal dogfeed CVars -- We are officially dogs baby!!! 🦴🐶
- [x] Create first documentation copy using MKDocs -- FINALLY!! Still a lot to be done, but core API refs are written. 🎉
- [ ] Create a tech-demo console UI
- [ ] Create base command sets (Global, Alias, Environment)
- [ ] Create the executable config IO system (.cfg readers)
- [ ] Create the UserConfigManager and add a Project Setting for opt-in
- [ ] Update the runtime UI to v1

## 🧰 Requirements

- **Godot 4.6 or later**
- **C# 12.0 (.NET 8) or later**

## ⚠️ Privacy notice!

This Logger uses compile-time directory baking to display certain paths in logs.  
This results in super fast, interned strings - but it bakes your folder structure into the game!  
If you care about not leaking your file system structure, edit your `.csproj` with the `<PathMap>` element like so:

```xml
<Project Sdk="Godot.NET.Sdk/4.6.3">
  <PropertyGroup>
	. . .
	<PathMap>$(MSBuildProjectDirectory)=/MyProjectName</PathMap> // Add this!
  </PropertyGroup>
</Project>
```

This replaces your project origin `\users\vonriddarn\Godot\Projects\MyProject` (example) with the new defined filepath `/MyProjectName` (example).

### If you edit your PathMap, do this!

If you update the PathMap to something else, like `/PikeConsole` or whatever, you MUST update the pathmap string to match.  
Go into `Project settings (General)` > `FractalPike` > `PikeConsole` and set `PathMap` to match. In this case: `/MyProjectName`.

Failing to do this will not break your project, but your log paths will **no longer be clickable**.

## 🐇 Getting started

PikeConsole has an extensive **documentation** where you can read up on whatever you're wondering.  
If you just want to get started and have the project running in less than 2 minutes, check out the **quick start guide**(link to github pages generated with MKDocs)!

## ⁉️ Questions & Answers

### Why am I forced to use interpolated strings in the logger?

_(Why does `PikeLogger.Log` give me an error?)_

#### The vision

The PikeLogger uses a custom stringbuilder to enforce as close to zero allocation as possible.  
If the current environment does not match the target environment, like if we are playing a release build, but the log is meant for the editor only, the interpolated string will not be built. This makes the system zero-allocating for untargeted environments.

#### The problem

The drawback to this is that there is no real way (that I know of) to efficiently separate a concatenated string from a string literal.  
That means we cannot differ:

```csharp
string foo = "Hello world!";
```

From:

```csharp
string bar = Hello() + World();
```

This creates a problem since string concantenation (`Hello() + World()`) is processed before sending it into the method.  
Thus, even if we block the log inside the logger, it is already too late. The system has spent processing power building the concantenated string.

#### The solution

Since we can't differ a (memory) safe string literal from an unsafe concantenated string, we allow neither.  
Instead the system forces you to use interpolated strings, which can be (and are) intercepted before they are built.

Thus, the propper syntax to use the `PikeLogger` becomes:

```csharp
// Notice the dollar sign before the string.
PikeLogger.Log(LogTarget.Debug, $"Hello world");

// Now, this becomes memory safe too!!
PikeLogger.Log(LogTarget.Debug, $"{Hello() + World()}");
```

### What difference does the LogTargets do?

The `LogTarget` tells the logger what environment this log should run in.  
Below is a table showing where logs show depending on their `LogTarget`.

| Build Environment                         | Runtime console (UI) | Output |
| :---------------------------------------- | :------------------- | :----- |
| **Playtesting in editor**                 | Debug, Runtime       | Editor |
| **Compiled DEBUG build**                  | Debug, Runtime       | -      |
| **Compiled RELEASE build** _(final game)_ | Runtime              | -      |

Notice how _PikeLogger_ does not natively route logs to file.  
If you want to route logs to a file, eg: `errors.txt` you will have to wire a system that subscribes to _PikeLoggers_ `LogEmitted` event.

### How do I stop PikeConsole from colliding with my other namespaces?

All utilities in PikeConsole use a mix of conventional and intuitive names. Therefore there is a slight collision chance with other libraries.  
To avoid collisions, use a using-alias:

```csharp
using FractalPike.PikeConsole.Core.Logging;

// Using alias that tells the compiler "Use PikeConsoles utility for LogTarget"
using LogTarget = FractalPike.PikeConsole.Core.Logging.LogTarget;

// Wow, it works!
PikeLogger.Log(LogTarget.Runtime, $"Hello world!");
```

## 🔦 Highlighted Features

#### 💻 Runtime console

Fully featured thread-safe runtime UI that receives logs, executes commands and modifies CVars. No setup required!

#### 🔀 Advanced zero-allocation log routing

Logs are dynamically routed as references to the receiver for the targeted environment. Logs can be domain tagged for easy filtering.  
The runtime killswitch also makes the entire system no-op, meaning the console produces no overhead when turned off.

#### 🔗 Clickable source paths (Godot editor)

When using `PikeLogger.LogWarning(...)` or `PikeLogger.Error(...)` the log is automatically prefixed with a clickable link.  
Clicking this will open up the file and line in your default IDE.  
_Logs also contain overrides for explicitly hiding or showing the path on demand._

#### ⚙️ No-code CVar registry

Create CVars directly in the Godot editor! Just `right click` > `New Resource` and choose the CVar you want to create.  
The CVar is automatically fetched at runtime without using reflection. Making them O(1) lookup at runtime and AOT-safe!

#### 🕹️ Context-based Commands & Aliases

The declarative API for creating custom commands is beginner friendly and efficient. You get maximum power for minimal code!  
Just inherit from the `CommandSet` node, attach it to your world and watch the magic!  
**Aliases** can be registered at runtime and come with automatic recursion protection to prevent infinite loops that could crash the game.

#### 🤓 Interop considerate

All static interop values are saved in order to prevent marshalling accross the C++ bridge.  
The entire system lives within the .NET environment and only leaves when strictly necessary.

#### 📦 AOT-Safe compilation

The entire framework is built with ahead-of-time compilation in mind.  
Therefore there is no magic reflection or dangling attributes that could cause unintended stripping.

## 🗣️ Credits and support

**PikeConsole does not require you to give credits!**  
You are free to use the framework however you'd like and never even mention it!  
If you would like to support the project though, I'd love for you to leave a ⭐ if you found it useful!

## ⚖️ License & Copyright

This project is released under the MIT open source license.  
**You may use, modify and distribute the contents of this repo however you'd like for free**.

### Branding exception

The official `logo.png` placed at the root of this repository, as well as all files in **ANY FOLDER** named `proprietary` are the exclusive property of
[Timmy "VonRiddarn" Öhman](https://github.com/VonRiddarn) / [Fractal Pike Entertainment](https://www.fractalpike.com/).  
All rights reserved.

**_Please note:_**  
_This disclaimer is just here to protect my company branding and mascot._  
_If you use my logo just to give credit, and do not say or imply that we are affiliated: I truly, utterly, do not mind ❤️._
