using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;
using Godot;

/*
 * Internal note:
 * This is 6 scripts from the old unity based framework mashed into one.
 * (ConsoleController.cs, ConsoleState.cs, QueueManager.cs, ThrottleManager.cs, QueueSettings.cs, ThrottleSettings.cs)
 * Basically everything under the legacy /Console/Controller is here.
 * Some functions are dropped, like "dropStrategy" others have new meaning, 
 * like the consolestate which is now managed by PikeConsoleAPI.
*/

namespace FractalPike.PikeConsole.Frontend.Controllers;

public partial class LogDispatcher : Node
{

	public event Action<LogEvent[]> DispatchLogBatch;

	// Using a lock instead of concurrent queue so that we can guarantee that logs are dequeued correctly on limit.
	readonly Queue<LogEvent> _logQueue = new();
	readonly object _queueLock = new();

	// Throttle chace. Store key (location hash) and timestamp for when the throttle should end.
	readonly ConcurrentDictionary<int, ulong> _throttleCache = new();

	[Export] int _maxQueueSize = 512;
	[Export] int _maxDequeuesPerTick = 64;
	[Export] ulong _throttleLifespanMs = 1000;

	CVarInt _consoleUpdateRate;
	double _tickRate = 0;

	public override void _EnterTree()
	{
		PikeConsoleStates.ConsoleUpdateRate.ValueChanged += OnUpdateRateChanged;
	}

	public override void _ExitTree()
	{
		PikeConsoleStates.ConsoleUpdateRate.ValueChanged -= OnUpdateRateChanged;
	}

	void OnUpdateRateChanged(int newRate)
	{
		_tickRate = 1.0 / newRate;
	}

	public override void _Ready()
	{
		OnUpdateRateChanged(PikeConsoleStates.ConsoleUpdateRate.Value);
		PikeLogger.LogEmitted += OnLogEmitted;
	}

	void OnLogEmitted(in LogEvent logEvent)
	{
		// Early return if the throttle wont let us through and we're not forcing the log.
		if (!logEvent.ForceLog && TryThrottle(logEvent.CallerKeyHash))
			return;

		// Locking so that we can drop logs without race conditions.
		// This creates a small bottleneck, but if we are bottlenecked here, the lock isn't the problem. We're logging too much.
		lock (_queueLock)
		{
			// If the log queue is full we drop the oldest log to make room for the newest.
			// Note to self: Do not reimplement "dropStrategy"! It was literally useless.
			if (_logQueue.Count >= _maxQueueSize)
				_logQueue.Dequeue();

			_logQueue.Enqueue(logEvent);
		}
	}

	bool TryThrottle(int key)
	{
		ulong now = Time.GetTicksMsec();
		ulong expiry = now + _throttleLifespanMs;

		// Using concurrent, so this is safe.
		if (_throttleCache.TryGetValue(key, out ulong oldExpiry) && oldExpiry > now)
			return true;

		// If we're not throttled, update the registry.
		_throttleCache[key] = expiry;
		return false;
	}

	double _timeSinceLastTick = 0.0;
	public override void _Process(double delta)
	{
		_timeSinceLastTick += delta;

		if (_timeSinceLastTick < _tickRate)
			return;

		_timeSinceLastTick -= _tickRate;
		CleanupThrottleCache();
		DispatchQueueBatch();
	}

	void DispatchQueueBatch()
	{
		List<LogEvent> batch = [];

		lock (_queueLock)
		{
			for (int i = 0; i < _maxDequeuesPerTick; i++)
			{
				if (_logQueue.TryDequeue(out LogEvent entry))
					batch.Add(entry);
				else
					break;
			}
		}

		if (batch.Count > 0)
		{
			DispatchLogBatch?.Invoke([.. batch]);
		}
	}

	void CleanupThrottleCache()
	{
		ulong now = Time.GetTicksMsec();

		// Using concurrent, so this is safe.
		foreach (var kvp in _throttleCache)
		{
			if (kvp.Value <= now)
				_throttleCache.TryRemove(kvp.Key, out _);
		}
	}
}
