# LogTarget
`[Flags]`
`public enum LogTarget`  

**Namespace**: `FractalPike.PikeConsole.Core.Logging`  

## Description

Flags used when logging to determine the target environment.  
Note that [PikeLogger](PikeLogger.md) will not even process strings for non-target environments, which is a huge performance benefit. 

/// note | Values  
`Debug` : `1 << 0` (`1`)
: Show in compiled debug builds.

`Runtime` : `1 << 1` (`2`)
: Show in compiled release builds.  
 _Will also show in debug builds, as expected._

`Editor` : `1 << 2` (`4`)
: Show in the Godot output window.

`All` : `Debug | Runtime | Editor`
: Show in all compiled builds and the Godot output window.
///

## Descriptive table  

| LogTarget | UI - Debug | UI - Release | Output  | Logfile |
| --------- | ---------- | ------------ | ------- | ------- |
| Debug | YES | NO | NO | NO |
| Runtime | YES | YES | NO | NO |
| Editor | NO | NO | YES | NO |  
| All | YES | YES | YES | NO |  

**In short**:  
Use `Debug` for logs that should show when debugging / QA testing.  
Use `Runtime` for logs that should be seen by players.  
Use `Editor` for logs that should only show in the Godot output window.

///tip
Using `Runtime` or `All` for errors and warnings will allow players to send more detailed bug reports.
///

---