using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution;
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
using System.Text;

namespace FractalPike.PikeConsole.Examples;

public partial class WelcomeCommandSet : CommandSet
{
	protected override void OnReady()
	{
		PrintWelcomeMessage();
	}

	protected override Command[] InstantiateCommands() => [
		Command(
			"welcome",
			"Prints the welcome message to the console.",
			null,
			"welcome [no args]",
			false,
			(_) => {
				PrintWelcomeMessage();
				return new(ExecutionResponseStatus.Success, null);
			}
		)
	];

	static void PrintWelcomeMessage()
	{
		StringBuilder welcomeSb = new("Welcome to the PikeConsole FPS demo!");
		welcomeSb.AppendLine("To get started, type \"find\" for a list of all available commands.");
		welcomeSb.AppendLine("To see all CVars affecting the crosshair, type \"find ch_\".");
		welcomeSb.AppendLine("To see all CVars affecting the player, type \"find pl_\".");
		welcomeSb.AppendLine("To see all CVars affecting the console settings, type \"find console_\".");
		welcomeSb.AppendLine("To see all commands affecting the user config system, type \"find user_\".");
		welcomeSb.AppendLine();
		welcomeSb.AppendLine("To get help with a command or CVar, type \"help command_name\".");
		welcomeSb.AppendLine("To find the disc location of a command or CVar, type \"whereis command_name\".");
		welcomeSb.AppendLine();
		welcomeSb.AppendLine("A note on CVars:");
		welcomeSb.AppendLine("typing the CVar with no arguments will print its current and default value.");
		welcomeSb.AppendLine();
		welcomeSb.AppendLine("If you're familiar with the GoldSrc console, you should feel right at home.");
		welcomeSb.AppendLine("If not, you're in luck. Because it's one of the easiest and intuitive frameworks to learn!");
		welcomeSb.AppendLine();
		welcomeSb.AppendLine("Use \"cheatmode 1\" or \"cheatmode true\" to enable cheat mode.");
		welcomeSb.AppendLine();
		welcomeSb.AppendLine("Use the command \"welcome\" to see this message again!");
		welcomeSb.Append("Good luck!");

		StatementExecutor.Execute(ExecutionSource.System, "clear", [], silent: true);
		PikeLogger.Log(LogTarget.Runtime, $"{welcomeSb.ToString()}");
	}
}
