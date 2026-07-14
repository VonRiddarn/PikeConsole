using FractalPike.PikeConsole.Core.Logging;

namespace FractalPike.PikeConsole.Core.Utilities;

public interface IConsoleFrontend
{
	/// <summary>
	/// Used by the FrontendInitializer to push startup logs to the frontend.
	/// </summary>
	/// <param name="logEvents">All logevents that was missed by the frontend.</param>
	public void PushStartupLogs(LogEvent[] logEvents);
}