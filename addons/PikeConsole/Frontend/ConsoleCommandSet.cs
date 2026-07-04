using FractalPike.PikeConsole.Core.RuntimeExecution;
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;

namespace FractalPike.PikeConsole.Frontend;

public partial class ConsoleCommandSet : CommandSet
{
	PikeConsoleUI _pikeConsoleUi = null;

	protected override void OnEnterTree()
	{
		_pikeConsoleUi = Owner as PikeConsoleUI;
	}

	protected override Command[] InstantiateCommands() => [
		Command(
			"clear",
			"Clears the runtime console of logs.",
			null,
			"clear [no args]",
			false,
			(_) => {
				if(_pikeConsoleUi == null)
					return new(ExecutionResponseStatus.Failed, "No console frontend is setup in the Godot editor.");

				_pikeConsoleUi.Clear();
				return new(ExecutionResponseStatus.Success, null);
			}
		),
	];
}
