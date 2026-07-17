using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution;
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
using FractalPike.PikeConsole.Frontend.Controllers;
using Godot;

namespace FractalPike.PikeConsole.Frontend;

public partial class ConsoleCommandSet : CommandSet
{
	[ExportGroup("Dependencies")]
	[Export] OutputController _outputController;

	protected override void OnEnterTree()
	{
		if (_outputController == null)
			PikeLogger.LogError(LogTarget.Editor, $"Output Controller has not been through the editor in \"{Name}\".");
	}

	protected override Command[] InstantiateCommands() => [
		Command(
			"clear",
			"Clears the runtime console of logs.",
			null,
			"clear",
			false,
			(_) => {
				if(_outputController == null)
					return new(ExecutionResponseStatus.Error, "Missing dependency for output controller.");

				_outputController.Clear();
				return new(ExecutionResponseStatus.Success, null);
			}
		),
	];
}
