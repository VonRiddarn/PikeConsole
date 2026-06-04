namespace FractalPike.PikeConsole.Core.Logging.Models;

public readonly struct LogEvent(int callerKeyHash, LogLevel logLevel, string message, bool forceLog)
{
	public readonly int CallerKeyHash = callerKeyHash;
	public readonly LogLevel LogLevel = logLevel;
	public readonly string Message = message;
	public readonly bool ForceLog = forceLog;
}
