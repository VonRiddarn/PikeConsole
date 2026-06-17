TODO: Explain how the PikeLogger works, what events are emitted and how performance is saved by not allocating on the heap.  
Explain that the PikeLogger is meant to be used as a REPLACEMENT for GD.Print, not a supplement - mention interop and performance. 

Benefits
- Clickable links in editor
- Log Routing
-   - Zero allocation
-   - Debug vs Runtime
-   - Interop bridge stripping
-   - Runtime killswitch

Talk about interpolated strings and why they are goated for performance.  
Adress the forceful convention and why that choice was made.  

EG: "Arguments are evaluated before method execution.
The custom interpolated string handler prevents allocations
when the `LogTarget` is disabled."
