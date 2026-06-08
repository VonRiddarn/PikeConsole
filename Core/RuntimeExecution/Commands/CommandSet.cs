using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution;
using Godot;

public abstract partial class CommandSet : Node
{
	/// <summary>
	/// All commands within this CommandSet.
	/// Initialized only once at runtime.
	/// </summary>
	public ImmutableArray<Command> Commands { get; private set; }

	// ----- ----- GODOT API WRAPPER ----- -----
	// Wrapper virtual API for Godot methods.
	// This is so that it is exceptionally clear that the original methods are off limits.
	protected virtual void OnEnterTree() { }
	protected virtual void OnReady() { }
	protected virtual void OnExitTree() { }

	/// <summary>
	/// Obligatory registration method that must return an array of Commands.
	/// This is used by internal systems to register commands to the registry.
	/// Example usage:
	/// <code>
	/// protected override Command[] InstantiateCommands() => [
	/// Command(...),
	/// Command(...)
	/// ];
	/// </code>
	/// </summary>
	/// <remarks>
	/// Use with the shorthand method <c>Command()</c> to get automatic stack trace injection.
	/// This makes debugging easier!
	/// </remarks>
	protected abstract Command[] InstantiateCommands();


	// ----- ----- GODOT API ----- -----
	// Sealing important initialization methods so that nobody accidentally 
	// overrides them without calling base..().
	public sealed override void _EnterTree()
	{
		if (Commands.IsDefault || Commands.IsEmpty)
			InitializeCommandsInternal();

		OnEnterTree();
	}

	public sealed override void _Ready()
	{
		RegisterCommandsInternal();
		OnReady();
	}

	public sealed override void _ExitTree()
	{
		CommandRegistry.Unregister(Commands);
		OnExitTree();
	}

	// ----- ----- INHERITED API ----- -----
	/// <summary>
	/// Shorthand for creating commands.
	/// This will automatically inject a custom stack trace to the caller CommandSet 
	/// (compile time, no overhead) to the created command.
	/// </summary>
	/// <param name="signature">
	/// Command signature to run this executable, EG: "r_cleardecals" or "env_gc"<br/>
	/// A signature has no set naming rules, but it is convention to name it after scope separated using underscores.
	/// </param>
	/// <param name="shortDesc">The short description (1 line) for this executable. EG: "Force-activates the garbage collector"</param>
	/// <param name="longDesc">The long description for this executable. Go wild.</param>
	/// <param name="usage">Usage example of this executable, EG: "em_find [enemy id | enemy type]" or "env_gc [no args]"</param>
	/// <param name="isCheat">If set to true CheatMode must be active to run this command.</param>
	/// <param name="action">
	/// The method to run when this command is called. Example of a simple echo command:
	/// <code>
	/// (args) => {
	///     PikeLogger.Log(LogTarget.Runtime, $"{string.Join(' ', args)}");
	///     return new Response&lt;ExecutionResponseStatus&gt;(ExecutionResponseStatus.Success);
	/// }
	/// </code>
	/// </param>
	/// <param name="filePath">COMPILER INJECTED ARGUMENT, DO NOT SET.</param>
	/// <param name="lineNumber">COMPILER INJECTED ARGUMENT, DO NOT SET.</param>
	/// <returns>A command with an automatic compile-time custom stacktrace.</returns>
	protected Command Command(
		string signature,
		string shortDesc,
		string longDesc,
		string usage,
		bool isCheat,
		Func<string[], Response<ExecutionResponseStatus>> action,
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0)
	{
		return new Command(signature, shortDesc, longDesc, usage, isCheat, action, new CustomStackTrace(filePath, lineNumber));
	}


	// ----- ----- INTERNAL API ----- -----

	// We inject the filepath and linenumber so that we can reference the actual child that messed up instead of this base class.
	// This class is self-diagnostic by design. This ensures we fail fast and catch collisions early.
	void InitializeCommandsInternal(
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0
	)
	{
		try
		{
			Commands = ImmutableArray.Create(InstantiateCommands() ?? []);
		}
		catch (Exception ex)
		{
			Commands = [];
			PikeLogger.LogError(LogTarget.All, $"UNEXPECTED ERROR: Failed to instantiate commands in {filePath}:{lineNumber} [ Node: {this}] - {ex}");
		}

		if (Commands.Length <= 0)
			PikeLogger.LogWarning(LogTarget.All, $"Commands failed to register from {filePath}:{lineNumber} [Node: {this}]. Make sure InstantiateCommands return a valid array!");
	}

	void RegisterCommandsInternal()
	{
		Response<RegisterCommandResponseStatus>[] responses = CommandRegistry.Register(Commands);

		// Self diagnose by checking all commands.
		foreach (Response<RegisterCommandResponseStatus> response in responses)
		{
			switch (response.Status)
			{
				case RegisterCommandResponseStatus.Success:
					break;
				case RegisterCommandResponseStatus.ReplacedAlias:
					PikeLogger.LogWarning(LogTarget.All, $"{response.Message}", forceLog: true);
					break;
				case RegisterCommandResponseStatus.AlreadyExists:
					PikeLogger.LogError(LogTarget.All, $"{response.Message}", forceLog: true);
					break;
				// Unexpected error.
				default:
					PikeLogger.LogError(LogTarget.All, $"{response.Message}", forceLog: true);
					break;
			}
		}
	}
}
