# Best Practices

-   - Best practices
-   -   - Do not place in hot path (even if the framework is very optimized; Guardrail, not free-card)
-   -   - Place killswitch as a setting in the settings menu under "enable console"
-   -   - Should replace GD.print all together
-   -   -   - Interop bridge (C# -> C++ interop overhead, PikeConsole is fully C# and non-alloc with)
-   -   -   - Use LogTarget.Debug for internal use that strips interop bridge in builds
-   -   -   - Hard set killswitch in config and do not add a setting to exclude from release builds. Not recommended.