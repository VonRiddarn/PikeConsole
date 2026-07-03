using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution;
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
using Godot;

public partial class MyCommandSet : CommandSet
{
	protected override void OnEnterTree() =>
		PikeLogger.Log(LogTarget.All, $"Tree entered!");

	protected override void OnReady() =>
		PikeLogger.Log(LogTarget.All, $"Node ready!");

	protected override void OnExitTree() =>
		PikeLogger.Log(LogTarget.All, $"Tree exited!");

	protected override void OnCheatModeChanged(bool newState)
	{
		if (!newState)
			PikeLogger.Log(LogTarget.All, $"Force removing noclip...");
	}

	protected override Command[] InstantiateCommands() => [
		Command(
				"my_echo",
				"Joins and echoes the arguments back to the caller",
				null,
				"my_echo [args...]",
				false,
				(args) =>
					new(ExecutionResponseStatus.Success, $"{args.Join(" ")}")
			),
		];
}