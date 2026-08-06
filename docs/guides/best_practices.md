# Best Practices

## 🔝 CommandSets at top layer

When adding a [`CommandSet`](../api/RuntimeExecution/Commands/CommandSet.md) to your scene, always make sure to add it to the top most instance in each system.  
For example, if we have a [`CommandSet`](../api/RuntimeExecution/Commands/CommandSet.md) to kill, move or mutate enemies, **do not** place it on the enemy instance itself!  

Instead place it on the `enemy manager` instance, which is less volatile.  
This should come intuitively, but it is worth noting.  

## ⚙️ Cvars are for tweaks  

When using CVars, use them on values that are either mostly static, or used for tweaking gameplay / environment.  

**Good** examples of CVars are:  

- `player_speed`  
- `player_jump_height`  
- `world_daytime_speed`  
- `monster_spawn_rate`    

**Bad** examples of CVars are:  

- `player_health`
- `player_current_ammo`
- `world_current_time`

## ☝️ Make the console killswitch available  

PikeConsole comes with a built in killswitch.  
Setting `PikeConsoleStates.RuntimeConsoleEnabled.Value` to `false` will effectively kill the runtime console.  
In rare cases, such as if the game is spamming errors or the player is using exceptionally slow hardware, this can save on performance.  
It is recommended to provide a toggle for this value in your settings menu, much like how Valve did in **Half-Life 2**.  

## 🛑 Stop using `GD.Print`

[`PikeLogger`](../api/Logging/PikeLogger.md) is not a substitute for `GD.Print`, it's a replacement.  
Use [`PikeLogger`](../api/Logging/PikeLogger.md) exlusively to benefit from the smart compiler flags, self diagnostic errors and zero allocation logging.  
[`PikeLogger`](../api/Logging/PikeLogger.md) bypasses the interop bridge entirely and is an optimized .NET runtime class.  

See [the logging guide](logging.md) for more info.

## 📙 Other resources

* [Getting Started](getting_started.md) (Recommended)
* [Logging](logging.md) (Recommended)
* [Cvars](cvars.md)
* [Commands](commands.md)
* [Aliases](aliases.md)
* [User Configs](user_configs.md)
* [Video Guides](video_guides.md)

* [API REF](../api/index.md)