// using System;
using System;
using FractalPike.PikeConsole.Core.Logging.Models;

namespace FractalPike.PikeConsole.Core.Logging;

/*
	This class routes logs to log receivers. Nothing more.
	Logs are not throttled or queued. They are sent to receivers or the in-engine GD.Print immedietly.
	It's then up to each individual subscriber to handle filtering.
	This class does, however, execute logs by target and make sure to no-op logs that are not for the current environment.

	Note: 
	All log calls use [CallerFilePath] and [CallerLineNumber] to allow for easier throttling and debugging by other systems.
	To my understanding this is compiler magic and not reflection, so it should be good. Do not spam logs in the hot path anyway.
*/

public static class PikeLogger
{
	/// <summary>
	/// Universal log emitter event. The in-game console subscribes to this event.
	/// </summary>
	/// <remarks>
	/// Godot engine logs are NOT routed through this logger. This logger 
	/// only serves to centralize and route developer made logs. Engine logs 
	/// are handled by a separate wrapper.
	/// </remarks>
	public static event Action<LogEvent> LogEmitted;
}
