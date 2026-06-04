# PikeConsole

Proprietary Godot CVar and Command console system for Fractal Pike.

## IDE Warnings

Due to the namespace configuration some IDEs will complain about not including the addons folder.  
Since we are using Domain Driven Design it is safe to just disable the warning for this project.  
`.csproj`

```
<Project Sdk="Godot.NET.Sdk/4.6.3">
  <PropertyGroup>
	. . .
	<NoWarn>$(NoWarn);IDE0130</NoWarn> // Add this!
  </PropertyGroup>
</Project>
```
