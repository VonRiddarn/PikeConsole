namespace FractalPike.PikeConsole.Core.Logging.Models;

/// <summary>
/// A struct used for outwards communications from the PikeLogger.
/// </summary>
public readonly struct LogEvent(int callerKeyHash, LogLevel logLevel, string message, bool forceLog, string domain)
{
	/// <summary>
	/// Unique key built using the callers filepath and linenumber. Used by listeners for throttling.
	/// </summary>
	public readonly int CallerKeyHash = callerKeyHash;

	/// <summary>
	/// The "severity" or "category" of the log. (EG: LogLevel.Info | LogLevel.Error)
	/// </summary>
	public readonly LogLevel LogLevel = logLevel;
	public readonly string Message = message;

	/// <summary>
	/// Flag for listeners. Used to bypass throttling.
	/// </summary>
	public readonly bool ForceLog = forceLog;

	/// <summary>
	/// An optional string attached to the log event that allows advanced listeners to filter logs.
	/// Internal systems separate their domains using dots (EG: "FractalPike.Entities.AI").
	/// </summary>
	/// <remarks>
	/// Performance Note: Using string literals (EG: "Game.Combat") makes the compiler use string interning. 
	/// This makes the execution zero-allocation. Dynamic domains (EG: $"Game.Player.{playerName}") 
	/// will allocate heap memory. This shouldn't become a bottleneck, but worth mentioning.
	/// </remarks>
	public readonly string Domain = domain;
}
