using FractalPike.PikeConsole.Core.Logging;

namespace FractalPike.PikeConsole.Core.Utilities;

public interface IStartupLogConsumer
{
	/// <summary>
	/// Used by the FrontendInitializer to push startup logs to the frontend.
	/// </summary>
	/// <remarks>
	/// This is how the UI can display logs that were made BEFORE its lifecycle. <br />
	/// These logs may contain important errors, warnings or diagnostics.
	/// </remarks>
	/// <param name="logEvents">All logevents that was missed by the frontend.</param>
	public void ConsumeStartupLogs(LogEvent[] logEvents);
}