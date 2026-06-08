TODO: Document the file system  
Should touch:

- File system
-   - Reading files
-   - Writing to files
- Boot order
-   - `user://pike_console/user_settings.cfg` is internally handled first
-   - File initializer array, top-bottom handled after that
-   - All files in `config/` must be manually executed using the `exec [filename]` command
- Player facing config system
-   - Setting up files to execute automatically from editor (Initializer resource)
-   -   -   - Default home for the settings file (`user://pike_console/user_settings.cfg`)
-   - How players setup their files (`user://pike_console/config/`)
-   - anything in root config can be called using `exec filename`
-   - Good for giving players or QA testers pre-defined sets of commands, like: `map_4_boss_lowhp.cfg`
-   - Players can edit and remove these at will, so no dependencies
- Cvars in the filesystem
-   - Players can save all current persistent CVars (make a custom user profile) with command `create_profile [name] [force?]`
-   -   - This creates a file: `user://pike_console/config/profile_name` (collions are denied and self diagnosed with feedback "file already exists" or "rewrote file name")
-   -   - Useful for: Creating a settings profile on a shared PC. EG: 1 has sensitivity 1 the other sensitivity 5.
-   - Players can save all non-default CVars with command `write_config [name] [force?]`
-   -   - This creates a file: `user://pike_console/config/name` (collions are denied and self diagnosed with feedback "file already exists" or "rewrote file name")
-   -   - Useful for: QA testing and reducing runtime boilerplate without having to edit the raw cfg files
