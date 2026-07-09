# Ideas and TODOs

This is an unsorted list of TODOs and cool ideas. This is not a definitive list of upcoming features, nor is it
a promise to implement mentioned features. it serves more like a whiteboard / notepad.

## List

### TODO: Save logs to file

Add a button for "save logs to file" that automatically saves all current logs from the console in a human readable text document.  
Integrate with the native filesystem so that non-.technical players / end users can save it to an easy accessible place (like the desktop).  
Since all logs already live in the .NET environment this should be SUPER EASY. It's more or less just a for loop, stringbuilder and `File.WriteAllText()`

Alternative solution: Have a button and for opening the `user://` folder.  
This solves the issue game-wide where the player can just browse their user files.

### TODO: Diagnostic CommandSet

Add a designated commandset for runtime diagnostic measuring, like: `diag_mem_start` and `diag_mem_stop`.  
Whilst active, measure diagnostics and add a flag for "is diagnosing" (maybe CVar) so that we can give user feedback for diagnostic mode.  
On stop, compile results to usefull information and print. Maybe an option for saving to file. (Or just use the built in "save logs to file" button)

### TODO: String extentions

Quick shorthands for forcing strings to start or end with certain things

- EndWith -> If not end with, make end with
- NoEndWith -> If end with, make no end with
- StartWtih -> If not start with, make start with
- NoStartWith -> If start with, make no start with

### Network ramble for future future future endavors

"The thing is I can still add multiplayer to this system.
You just make a multiplayer wrapper for it and have an IO point to the network. If the IO point receives a command from the server to change a CVar it can just change that CVar interally using "system" as the source (since the server is the final authority). If a player changes an internal CVar and that is tracked by the NetIO system they send a command to the server that they want to change something, the server checks if they are RCON authorized and sends back the change to all clients if they pass. (With the autorization set to Server)."

Alternatively add a "IsNetworked" flag on each CVar through the CVarBase and have them automatically register themselves to the NetworkIO system on initialize.  
Just like how persistent varaibles register to the persistence registry.

We would probably have to add a guard clause for variables that are persistent AND networked - even though that is a logical fallacy (user errors).  
Edit: Actually, it's not a logical fallacy, because technically we could pass cl_name or cl_skin or cl_crosshaircolor etc to the server when we join or change some.  
This would make the server able to display our crosshair on other clients, or temporarily force a server-sided crosshair / skin / name etc without overriding the local memory.

The server could also execute commands on the clients - that way we can sync events etc. Something like "rcon_execute [statement]".  
This is why we HAVE to add a third authorization though! Otherwise a bad admin could run shitty commands like "rcon_execute exit" or "rcon_execute resetprofile".  
Having an authorization clause that prevents this is neat. We could also make it backwards compatible by having the parameter in the Command shorthand set "rconCallable" to false by default.
