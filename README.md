# PikeConsole - GoldSrc-inspired console framework for Godot!

**PikeConsole** is a _plug-and-play_ runtime execution framework that gives you a production-ready, professional-grade debug and command environment.  
The framework is built with **runtime optimization**, **extensibility** and **ease of use** in mind, which means everything is pre-configured to work great out of the box, but open enough to extend / customize its features!

<p align="center">
	<img src="/docs/_media/proprietary/pike-console-banner.png" alt="PikeConsole Banner"/>
</p>

## 🧰 Requirements

- **C# 12.0+ (.NET 8 or later)**
- **Godot 4.6+**

## 🐇💨 Getting started

If you want to start by toying around you can check out the ~~**[⏳ QUICK START GUIDE](#)**~~ which will have you running your first command in just **3 minutes**!
For visual learners there is an official **[▶️ YOUTUBE PLAYLIST](#)** that goes through the consoles concepts in a video format.

There is also a quite extensive ~~**[📑 DOCUMENTATION](#)**~~ where you can find anything from gudes and breakdowns, to a public API reference.

NOTE:  
The documentation and quick start guide is underway. To peek at the content, check out the `docs` folder in this repository, but be adviced that it is not fully realised.  
If you just want to get started with PikeConsole right now, check out the YouTube playlist!

## 🔥 Features

- **Drag and drop installation**  
  Install the framework by just dragging and dropping the addon folder into your project.  
  All project settings and features will connect automatically. (Some features requires you to build the project once)
- **Stupid easy (plug-and-play)**  
  The default UI that ships with PikeConsole comes with QOL features such as: Automatic command / alias suggestions, an alias system, diagnostic commands, runtime-documentation using the `help` command. All packed into a sleek, modern UI. You can just drag it in, play with the theme and just get going.
- **Zero-alloc routed logging**  
  The `PikeLogger` utility shipped with PikeConsole is a direct replacement for `GD.Print`.  
  It uses a interpolation manager to make logs truly no-op on non-targeted environments,
  which means you can just leave debug logs in without tanking performance in release!
- **AOT-safe compilation**  
  Everything in the system is built to be ahead-of-time compatible, which is a MUST if you're porting to console / mobile.
- **Thread safety out of the box**  
  Call logs on whichever thread you want. All logs are automatically managed on the main thread by the built in UI.
- **Automatic throttling**  
  All logs are throttled through their automatic compile-time location hash. This means a broken system will not deadlock the console and make it unreadable.
- **Context-aware commands**  
  Declarative command sets that know _who_ and _what_ triggered them. Natively mapped to the SceneTree's lifecycle with no reflection which makes it easy to reference in-world nodes and objects. Commands are created with a few lines of code.
- **No-code CVar registry**  
  Console variables based on native Godot resources allows for self-contained data with O(1) lookup!  
  Creating a CVar is as easy as right clicking in the editor and checking a few boxes.
- **Preconfigured commands and settings**  
  PikeConsole comes pre-configured with commands and CVars for basic debugging, user config management and console configuration.
- **Opt-in user config manager**  
  Save user configurations and settings with custom `.ecfg` files. The active profile run automatically at startup and seamlessly saves all user (CVar) configurations on change. Use as a single source of truth, or alongside an already integrated save system.
- **Clickable logs**  
  Links in the editor output window that takes you to the caller when clicked!
- **Automatic engine errors**  
  Errors coming from the C++ side of the engine are automatically routed back and displayed in the console. This makes it easy for QA testers and players to make error reports This system also catches `GD.PushError` calls.
- **Built for release**  
  The framework is intended to ship with the released version of the game. This means editor-related code is automatically stripped at compile time, and all active systems are optimized for minimal overhead. The system also comes with a full, 100% killswitch that no-ops both the logger and engine interop bridge.  
  _When the logger is disabled, the strings in calls to `PikeLogger` **will not build** meaning log calls scattered in the code draws no processing power._
- **And more!**  
  There are literally so many features that I know I'm forgetting cool things to list!

<p align="center">
	<img src="/docs/_media/showcase-animation.gif"/>
</p>

## 📊 Memory footprint

The memory footprint has been tested for 3 distinct levels of use.  
These are given the arbitrary names "indie", "AA" and "AAA" based on real life examples.
Note that all footprints are measured from the console itself within a running game instance, which automatically also measures the UI overhead.

### Indie

<img src="/docs/_media/showcase-mem-500.png"/>

This example is based on Valve's Half-Life 1, whcih used about 800 combined commands and console variables.  
For small teams or people not using an excessive amount of custom tooling, 500 is a very realistic upper benchmark.

### AA

<img src="/docs/_media/showcase-mem-5000.png"/>

This example is based on Valve's Half-Life 2, whcih used about 3000 combined commands and console variables.  
Medium sized teams with deep engine knowledge could potentially reach this kind of usage, and as the numbers suggest the framework is ready for it.

### AAA

<img src="/docs/_media/showcase-mem-10000.png"/>

This example is based on Valve's Counter-Strike: Global offensive, whcih used about 8000+ combined commands and console variables.
It is unlikely to reach these kinds of numbers without several large dedicated teams working on custom engine extentions and toolings.
The console does however hold at this level and retains its O(1) lookup capabilities.

## 🖼️ Screenshots

**Custom aliases with automatic recursion protection!**

<p align="center">
	<img src="/docs/_media/showcase-alias.png"/>
</p>

**Smart suggestions that shows Commands, CVars and runtime aliases!**

<p align="center">
	<img src="/docs/_media/showcase-suggestions.png"/>
</p>

**Modular and easy to understand project settings!**

<p align="center">
	<img src="/docs/_media/showcase-project-settings.png"/> <br/>
	<i>(Typos has been fixed since the capture of this screenshot)</i>
</p>

## ⚠️ Privacy notice!

_For a video tutorial on how to edit the path map, check out the official **[▶️ YOUTUBE PLAYLIST](#)**_.

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
