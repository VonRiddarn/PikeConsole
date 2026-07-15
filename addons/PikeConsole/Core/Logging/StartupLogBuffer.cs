using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FractalPike.PikeConsole.Core.Logging;

public static class StartupLogBuffer
{
	static List<LogEvent> _logs = [];
	static bool _dead = false;
	private static readonly object _lock = new();

	// Since PikeLogger is designed to be called on any thread, we neeed to thread safe this buffer too.
	// We could use a ConcurrentQueue for this, but we make the assumption that the startup impact is low.
	public static void TryBuffer(LogEvent logEvent)
	{
		if (_dead)
			return;

		lock (_lock)
		{
			// For race conditions with consume.
			if (_dead)
				return;

			_logs.Add(logEvent);
		}
	}

	// This will ALWAYS be called on the MAIN THREAD by a node ready to consume the buffer.
	// Once consimed
	public static LogEvent[] Consume([CallerFilePath] string FILEPATH = "")
	{
		LogEvent[] copy;
		bool wasAlreadyDead;

		lock (_lock)
		{
			wasAlreadyDead = _dead;

			if (!_dead)
			{
				_dead = true;
				copy = [.. _logs];

				// Clear the list before nulling to 100% makr sure the GC gets the memo for all elements
				_logs.Clear();
				_logs = null;
			}
			else
			{
				copy = [];
			}
		}

		if (wasAlreadyDead)
		{
			PikeLogger.LogWarning(LogTarget.All, $"StartupLogBuffer is dead but \"{FILEPATH}\" is still trying to consume it.", includePath: false);
			return [];
		}

		return copy;
	}
}