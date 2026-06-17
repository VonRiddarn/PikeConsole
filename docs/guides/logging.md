TODO: Explain how to use the PikeLogger and what LogLevels and Logtargets do.  
Explain that the PikeLogger is meant to be used as a REPLACEMENT for GD.Print, not a supplement - mention interop and performance.  
  
- Writing a log
-   - Syntax
-   - Interpolated string (for safety reasons... arguments run before the method etc...)
-   - Parameters

Add a note that if they want to read in detail about why we force the interpolated string and what benefits they get from replacing GD.Print with PikeLogger,
they should check out the logger docs. Link to the architecture/pikelogger path.

EG: "Arguments are evaluated before method execution.
The custom interpolated string handler prevents allocations
when the `LogTarget` is disabled."
