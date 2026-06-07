using FractalPike.PikeConsole.Core.RuntimeExecution;
using Godot;

public abstract partial class CommandSet : Node
{

	// TODO: IMPORTANT NOTE - DO NOT FORGET TO SEND COMPILER INJECTION ATTRIBUTES FOR FILEPATH TO COMMAND.
	// Since this is a helper, we must send the creator of the command to the logger for error diagnostics.
	// Look over the Unity based framework before doing anything.

	public IRuntimeExecutable[] Commands { get; private set; } = [];

	// _EnterTree is like Awake()?
	public override void _EnterTree()
	{
		// InitializeCommandsInternalOneShot();
	}

	// _Ready is like Start()?
	public override void _Ready()
	{
		// RegisterCommands(Commands);
	}

	// _ExitTree is like OnDestroy()?
	public override void _ExitTree()
	{
		CommandRegistry.UnRegister(Commands);
	}
}
