using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.RuntimeExecution;
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
using FractalPike.PikeConsole.Core.RuntimeExecution.Config;
using FractalPike.PikeConsole.Core.Utilities;
using Godot;

namespace FractalPike.PikeConsole.Core.Autoloading;

public partial class ConfigCommandSet : CommandSet
{
	protected override Command[] InstantiateCommands() => [
		Command(
			"exec",
			$"Executes one or more config files with the \"{PikeConsoleConfig.ConfigDirectory}\" directory as the root.",
			"User facing command that forces the path root to be within",
			$"userdir [no args]",
			false,
			static (args) => {
				var response = ConfigIO.ExecuteFromConfig(ExecutionSource.Standard, PikeConsoleConfig.ConfigDirectory + "/" + args[0]);

				if(response.Status != ConfigResponseStatus.Success)
					return new(ExecutionResponseStatus.Failed, response.Message, response.Flags);

				return new(ExecutionResponseStatus.Success, response.Message, response.Flags);
			}
		),
		Command(
			$"userdir",
			"Opens the actual \"user://\" directory using the native file system and full system path.",
			null,
			$"userdir [no args]",
			false,
			static (_) => {
				Error err = OS.ShellOpen(FileSystemHelper.UserDirectory.Globalized());

				if (err != Error.Ok)
					return new(ExecutionResponseStatus.Error, $"Failed to open the user directory. OS Error: {err}");

				return new(ExecutionResponseStatus.Success, $"Opened the user directoty at: {FileSystemHelper.UserDirectory.Globalized()}");
			}
		),
		Command(
			"reset",
			"Reset the value of a CVar.",
			"Reset the value of a CVar and remove persistance overrides from the player settings config.",
			"reset [signature]",
			false,
			static (args) => {
				if(!ArgumentParser.ValidateCount(args, 1, out string error))
					return new(ExecutionResponseStatus.InvalidArgs, error);

				string signature = args[0];

				if(RuntimeExecutableRegistry.TryGetExecutable(signature, out var rte))
				{
					if(rte is ICVar cvar)
					{
						if(!cvar.ResetValue(ExecutionSource.Standard))
							return new(ExecutionResponseStatus.DeniedCheat, $"Failed to reset value of \"{cvar.Signature}\". CVar is cheat protected.");

						return new(ExecutionResponseStatus.Success, $"\"{cvar.Signature}\" has been reset.");
					}

					return new(ExecutionResponseStatus.Failed, $"\"{rte.Signature}\" is not a CVar.");
				}

				return new(ExecutionResponseStatus.Failed, $"Unknown signature \"{signature}\".");
			}
		),
	];
}
