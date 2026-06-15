namespace FractalPike.PikeConsole.Core.Logging;

/// <summary>
/// Delegate that passes a log event by reference. This allows us to have our cake (structs, which is stack alloc)
/// and eat it too (send the data without making a copy)
/// </summary>
/// <param name="logEvent">The struct containing information about the log, such as the message</param>
public delegate void LogEventHandler(in LogEvent logEvent);