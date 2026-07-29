# LogLevel
`public enum LogLevel`  

**Namespace**: `FractalPike.PikeConsole.Core.Logging`  

## Description

List of log severities. Used by other systems for filtering, styling and priority logging.  

/// note | Values  
`Info` : `0`
: Default message.

`Success` : `1`
: Response to an action that was successfull.

`Warning` : `2`
: Report of non-fatal flaw.

`Error` : `3`
: Error report.

`Engine_Warning` : `4`
: Non-fatal flaw invoked by the engine layer (c++).

`Engine_Error` : `5`
: Error report invoked by the engine layer (c++).

///

---