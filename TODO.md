# Ideas and TODOs

This is an unsorted list of TODOs and cool ideas. This is not a definitive list of upcoming features, nor is it
a promise to implement mentioned features. it serves more like a whiteboard / notepad.

## List

## TODO: Save logs to file

Add a button for "save logs to file" that automatically saves all current logs from the console in a human readable text document.  
Integrate with the native filesystem so that non-.technical players / end users can save it to an easy accessible place (like the desktop).  
Since all logs already live in the .NET environment this should be SUPER EASY. It's more or less just a for loop, stringbuilder and `File.WriteAllText()`

### TODO: Diagnostic CommandSet

Add a designated commandset for runtime diagnostic measuring, like: `diag_mem_start` and `diag_mem_stop`.  
Whilst active, measure diagnostics and add a flag for "is diagnosing" (maybe CVar) so that we can give user feedback for diagnostic mode.  
On stop, compile results to usefull information and print. Maybe an option for saving to file. (Or just use the built in "save logs to file" button)
