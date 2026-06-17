TODO: Write a full, beginner friendly, comprehensible getting started guide.  
Should touch:

NOTE TO SELF:  
Include screenshots and images throguhout documetation.  
Place small files (ie webp) inside `docs/media`  
Also, always link to deeper docs when applicable!!

- Project setup
-   - Installing addon
-   -   - Drag and drop
-   -   - Pull PackagedScene into scene tree
-   -   - Console is now active (semicolon)!
-   -   - First command (early dopamine!)
-   -   -   - `echo Hello world`
-	-	- Expand on first command (tease complexity!)
-   -   -   - `count argument counter that "takes quotations into" consideration`
-   -   -   - `echo Hello; echo World`
-   -   - Rebinding the console key
-   - Preferences / Configuration
-   -   - Config file
-   -   - Privacy concerns (override compiler baked location using `<PathMap>`)
- Logging (First because it is the most impressive feature imo)
-   - Automatically collects engine logs, exceptions and warnings
-   -   - Copy paste code for causing an array out of bounds exception
-   -   - Killswitch (cut engine logs which severs interop bridge completely)
-   -   - Sending a log using PikeLogger
-   -   - Interpolated strings
-   -   -   - How
-   -   -   - Why
-   -   - Different logs
-   -   -   - Info
-   -   -   - Success
-   -   -   - Warning
-   -   -   - Error
-   - Best practices
-   -   - Do not place in hot path (even if the framework is very optimized; Guardrail, not free-card)
-   -   - Place killswitch as a setting in the settings menu under "enable console"
-   -   - Should replace GD.print all together
-   -   -   - Interop bridge (C# -> C++ interop overhead, PikeConsole is fully C# and non-alloc with)
-   -   -   - Use LogTarget.Debug for internal use that strips interop bridge in builds
-   -   -   - Hard set killswitch in config and do not add a setting to exclude from release builds. Not recommended.
- CVars (Before commands, because they are easier!)
-   - CVar folder
-   - Right click creation flow
-   - Consume the CVar from a subscriber
-   - More info, like how to make your own CVar type (link to `docs/cvars.md`)
- Commands
-   - Command lifecycle (context based, lives on nodes)
-   - Create a CommandSet
-   - The `Response<>` object and how to use it
-   -   - Returning a `Response<ExecutionResponseStatus>`
-   -   - How the command executor reacts to the `Response<ExecutionResponseStatus>` object (Logs if message exists, else stays silent. Exceptions are always logged.)
-   - Best practices (Place on enemymanager instead of enemy etc)
- Aliases
-   - What are aliases
-   - Where can they be used (runtime)
-   -   - How to get around this by registering a file for execution DEV side
-   -   - Players can register non auto execution cfg files (link to `docs/file-system`)
-   - How to create an alias
-   - A note on recursion (protected)
- Youtube
-   - Link to youtube playlist
-   - List of each individual video
