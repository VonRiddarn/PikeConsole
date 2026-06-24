# PikeConsole - The modern GoldSrc-inspired console framework!  

<p align="center">
	<img src="_media/proprietary/pike-console-banner.png" alt="PikeConsole branding and text"/>
</p>

Welcome to the official documentation for PikeConsole!  
This is a production-ready framework that provides high-performance, 
zero-allocation logging and command execution for Godot 4.6+!  

It is 100% AOT-compilation safe and provides everything from commands 
and CVars to user configs right out of the box! Whether you're looking for 
a runtime developer console, easy-to-use global state variables or just a more efficient logging 
system than the default `GD.Print` this is the tool for you!

/// note | Quick reminder
PikeConsole is provided "_as-is_".  
All versioning, upkeep and issue tracking are done when time allows for me to do so.  
///

## The docs are curated!  
Please note that these docs are curated to provide as smooth and frictionless of an experience as possible. 
Thus, some systems are occluded from the API reference or documentation in its entirety. This desn't mean the systems 
aren't doing anything, it simply means they weren't intended to be interacted with.  

PikeConsoles biggest strength is it's ease-of-use and out of the box magic. 
Connecting to certain parts of the API (like instancing raw commands) would bypass what makes PikeConsole great, 
and potentially tarnish your experience.  

The [API Reference](./api/index.md) exposes most things that any power-user might want to explore, 
but might leave out internal details. To hack into those details one must read the source code. 

The magic of PikeConsole comes from its ease-of-use, and the docs job is to reflect that as much as possible.

## Where to begin?
At the top of the page you've got tabs for any and all available root-pages at all times.  
If you're on mobile, these are inside the hamburger icon in the top left.  

The **Architecture & Framework** tab provides benchmarks and explanations regarding the different 
design decisions that went into the making of the framework. 
If you're looking for a more curated experience, you might want to start with:  

- **[Getting Started](guides/getting_started.md)**: Get the console setup and run your first command in less than 2 minutes!  
- **[Logging](guides/logging.md)**: Learn how to replace `GD.Print` with PikeLogger in just one line of code!
- **[Commands](guides/commands.md)**: Learn the inner workings of commands and create your own.  
- **[CVars](guides/cvars.md)**: Lean about CVars, how to use them and how to make your own. 

## Requirements  
* **Godot 4.6** _or later_.
* **.NET 8** _or later_.