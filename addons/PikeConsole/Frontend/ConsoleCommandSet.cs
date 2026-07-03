using FractalPike.PikeConsole.Core.Logging;
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
		Command(
			"echo",
			"Send a message to the console.",
			"Combines all arguments into a string and returns the concatenated result.",
			"echo [..args]",
			false,
			(args) => {
				PikeLogger.Log(LogTarget.Runtime, $"{string.Join(' ', args)}", forceLog: true, domain: "PikeConsole.Frontend");
				return new(ExecutionResponseStatus.Success, null);
			}
		),
		Command(
			"count",
			"Count all passed arguments.",
			"Counts all arguments and logs an integer of the count.",
			"count [..args]",
			false,
			(args) => {
				PikeLogger.Log(LogTarget.Runtime, $"{args.Length.ToString()}", forceLog: true, domain: "PikeConsole.Frontend");
				return new(ExecutionResponseStatus.Success, null);
			}
		),
		Command(
			"help",
			"Get detailed help of any command or CVar.",
			null,
			"help [signature]",
			false,
			(args) => {
				if(!ArgumentParser.ValidateCount(args, 1, out string error))
					return new(ExecutionResponseStatus.InvalidArgs, error);

				string signature = args[0];

				if(RuntimeExecutableRegistry.TryGetExecutable(signature, out var rte))
				{
					PikeLogger.Log(LogTarget.Runtime, $"{rte.GetHelp()}", forceLog: true, domain: "PikeConsole.Frontend");
					return new(ExecutionResponseStatus.Success, null);
				}

				return new(ExecutionResponseStatus.Failed, $"Could not find runtime executable with signature \"{signature}\".");
			}
		),
		Command(
			"reset",
			"Reset the value of a CVar.",
			"Reset the value of a CVar and remove persistance overrides from the player settings config.",
			"reset [signature]",
			false,
			(args) => {
				if(!ArgumentParser.ValidateCount(args, 1, out string error))
					return new(ExecutionResponseStatus.InvalidArgs, error);

				string signature = args[0];

				if(RuntimeExecutableRegistry.TryGetExecutable(signature, out var rte))
				{
					if(rte is ICVar cvar)
					{
						if(!cvar.ResetValue(ExecutionSource.Player))
							return new(ExecutionResponseStatus.DeniedCheat, $"Failed to reset value of \"{cvar.Signature}\". CVar is cheat protected.");

							return new(ExecutionResponseStatus.Success,$"\"{cvar.Signature}\" has been reset.");
					}

					return new(ExecutionResponseStatus.Failed, $"\"{rte.Signature}\" is not a CVar.");
				}

				return new(ExecutionResponseStatus.Failed, $"Unknown signature \"{signature}\".");
			}
		),
	];
}
