using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution;
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;

namespace FractalPike.PikeConsole.Autoloading;

public partial class DiagnosticCommandSet : CommandSet
{
	protected override Command[] InstantiateCommands() => [
		Command(
			"echo",
			"Send a message to the console.",
			"Combines all arguments into a string and returns the concatenated result.",
			"echo [..args]",
			false,
			static (args) => {
				PikeLogger.Log(LogTarget.Runtime, $"{string.Join(' ', args)}", forceLog: true);
				return new(ExecutionResponseStatus.Success, null);
			}
		),
		Command(
			"count",
			"Count all passed arguments.",
			"Counts all arguments and logs an integer of the count.",
			"count [..args]",
			false,
			static (args) => {
				PikeLogger.Log(LogTarget.Runtime, $"{args.Length.ToString()}", forceLog: true);
				return new(ExecutionResponseStatus.Success, null);
			}
		),
	];
}
