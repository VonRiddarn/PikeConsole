using FractalPike.PikeConsole.Core.Logging;
using Godot;
using System;

public partial class EngineLoggerBridgeManager : Node
{
	public override void _EnterTree()
	{
		PikeLogger.Log(LogTarget.All, $"Hello world!");
	}
}
