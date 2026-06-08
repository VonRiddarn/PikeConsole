TODO: Document logging  
Should touch:

- Clickable links in editor
- Log Routing
-   - Zero allocation
-   - Debug vs Runtime
-   - Interop bridge stripping
-   - Runtime killswitch
- Writing a log
-   - Syntax
-   - Interpolated string (for safety reasons... arguments run before the method etc...)
-   - Parameters

EG: "Arguments are evaluated before method execution.
The custom interpolated string handler prevents allocations
when the `LogTarget` is disabled."
