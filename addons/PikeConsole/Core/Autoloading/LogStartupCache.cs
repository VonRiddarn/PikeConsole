using System.Collections.Generic;
using FractalPike.PikeConsole.Core.Logging;
using Godot;

namespace FractalPike.PikeConsole.Core.Autoloading;

public partial class LogStartupCache : Node
{
	List<LogEvent> _logs = [];
	bool _dead = false;

	public override void _EnterTree()
	{
		PikeLogger.LogEmitted += OnLogEmitted;
	}
	public override void _ExitTree()
	{
		Kill();
	}

	void OnLogEmitted(in LogEvent logEvent)
	{
		_logs?.Add(logEvent);
	}

	public LogEvent[] Consume()
	{
		LogEvent[] logs = [.. _logs];
		Kill();
		return logs;
	}

	public void Kill()
	{
		if (_dead)
			return;

		PikeLogger.LogEmitted -= OnLogEmitted;
		_logs = null;
		_dead = true;
	}
}
