using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution;
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
using Godot;
using System;


namespace FractalPike.PikeConsole.Examples;

public partial class DebugCommandSets : CommandSet
{
	protected override Command[] InstantiateCommands() =>
	[
		Command(
			"echo_target",
			"Send a message to a specific LogTarget.",
			"Combines all arguments after the first into a string and logs the concatenated result to a specific environment. Used to test killswitches.",
			"echo_target [runtime | debug | editor | all] [..args]",
			false,
			static (args) => {
				if (args.Length < 2)
					return new(ExecutionResponseStatus.InvalidArgs, "Usage: echo [target] [message]");

				LogTarget target = args[0].ToLower() switch {
					"runtime" => LogTarget.Runtime,
					"editor" => LogTarget.Editor,
					"debug" => LogTarget.Debug,
					"all" => LogTarget.All,
					_ => LogTarget.Runtime,
				};

				PikeLogger.Log(target, $"{string.Join(' ', args[1..])}", forceLog: true);
				return new(ExecutionResponseStatus.Success, null);
			}
		),
				Command(
			"push_warning",
			"Push a warning to the Godot engine using GD.PushWarning. Used to test interop logger.",
			"Combines all arguments into a string and pushes it to the Godot engine as a warning.",
			"push_warning [..args]",
			false,
			static (args) => {
				GD.PushWarning(string.Join(' ', args));
				return new(ExecutionResponseStatus.Success, null);
			}
		),
		Command(
			"push_error",
			"Push a warning to the Godot engine using GD.PushError. Used to test interop logger.",
			"Combines all arguments into a string and pushes it to the Godot engine as an error.",
			"push_error [..args]",
			false,
			static (args) => {
				GD.PushError(string.Join(' ', args));
				return new(ExecutionResponseStatus.Success, null);
			}
		),
		Command(
			"throw",
			"Throw a generic, unhandled exception to be caught by the StatementExecutor.",
			"Combines all arguments into a message and throws a generic error in the .NET runtime environment. Used for testing the try-catch, PathMap and UI formatting.",
			"throw [..args]",
			false,
			static (args) => {
				throw new System.Exception(string.Join(' ', args));
			}
		),
	];
}
