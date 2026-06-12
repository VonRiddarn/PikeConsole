TODO: Document CVars

Should touch:

- What are CVars
-   - Cvars are global, self-contained data
-   - CVars are also COMMANDS... kinda
-   -   - How CVars register as commands on startup when the CVar crawler parses `res://cvars`
- Create CVars
-   - Basic creation (right click flow)
-   - Note about the persistent flag
-   - For persistent settings only - player facing
-   - Saved to `user://pike_console/user_settings.cfg`
- Consume CVars
-   - Subscription to ValueChanged (new value) used for EG: Health, Speed...
-   - Subscription to ValueInvalidated (passes nothing) used for EG: Crosshair (collection of CVars using the same subscription method)
-   - PREVENT MEMORY LEAKS - UNSUBSCRIBE!
- How to create custom CVar types
- See how CVars can be saved and written to file: link to `docs/file-system.md` (Cvars in the filesystem)
-   - Players can save all current persistent CVars (make a custom user profile) with command `create_profile [name] [force?]`
-   -   - This creates a file: `user://pike_console/config/profile_name` (collions are denied and self diagnosed with feedback "file already exists" or "rewrote file name")
-   -   - Useful for: Creating a settings profile on a shared PC. EG: 1 has sensitivity 1 the other sensitivity 5.
-   - Players can save all non-default CVars with command `write_config [name] [force?]`
-   -   - This creates a file: `user://pike_console/config/name` (collions are denied and self diagnosed with feedback "file already exists" or "rewrote file name")
-   -   - Useful for: QA testing and reducing runtime boilerplate without having to edit the raw cfg files

NOTE TO SELF:  
Might be a good idea to explain that CVars are resources that automatically create commands at startup.  
Maybe a catchy memorable phrase like "CVars are COMMANDS... kinda." or something.
