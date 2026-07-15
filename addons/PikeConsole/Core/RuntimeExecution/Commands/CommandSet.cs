using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.Utilities;
using Godot;

#nullable enable

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Commands;

// Disable obsolete warnings in this script. 
// This is because we use the obsolete attribute to scare of people from using "new Command()"
#pragma warning disable CS0618

public abstract partial class CommandSet : Node
{
	/// <summary>
	/// All commands within this CommandSet.
	/// Initialized only once at node instantiation.
	/// </summary>
	ImmutableArray<Command> _commands;

	// ----- ----- SIGNATURE SHORTHAND ----- -----
	/// <summary>
	/// Optional prefix that automatically spplies to strings using the <c>Signature(string signature)</c> shorthand.
	/// </summary>
	protected virtual string Prefix => string.Empty;
	/// <summary>
	/// Shorthand for creating a prefixed signature.
	/// </summary>
	/// <remarks>
	/// To create a signature without a prefix, or using another prefix than the Prefix override, apply a raw string instead. <br />
	/// Signature("foo_bar") -> "prefix_foo_bar". To get "foo_bar" you pass the actual string literal.
	/// </remarks>
	/// <param name="signature">The signature to apply the <c>Prefix</c> override to.</param>
	/// <returns></returns>
	protected string Signature(string signature) => (string.IsNullOrWhiteSpace(Prefix) || signature.StartsWith($"{Prefix}_")) ? signature : $"{Prefix}_{signature}";

	// ----- ----- GODOT API WRAPPER ----- -----
	// Wrapper virtual API for Godot methods.
	// This is so that it is exceptionally clear that the original methods are off limits.
	protected virtual void OnEnterTree() { }
	protected virtual void OnReady() { }
	protected virtual void OnExitTree() { }

	// ----- ----- PIKECONSOLE API WRAPPER ----- -----
	/// <summary>
	/// Method override that can be used to reset states or otherwise react to when cheatmode is toggled.
	/// Eg: if player is noclipping and cheatmode is turned off, disable the noclip.
	/// </summary>
	protected virtual void OnCheatModeChanged(bool newState) { }

	// ----- ----- SELF DIAGNOSTIC DEPENDENCIES ----- -----

	/* 
		Q: Why use DerivedScriptPath?
		A:
			Because if a child node deriving from CommandSet triggers an error,
			the compiler attributes will point to the root CommandSet.cs file (this file)
			instead of the child that actually messed up.

			Using DerivedScriptPath makes the diagnostics more accurate,
			and since it is lazy-initialized it uses no processing power in a healthy system.
	*/
	string? _derivedScriptPath = null;
	string DerivedScriptPath => _derivedScriptPath ??= GetScript().As<Script>()?.ResourcePath ?? "Unknown Script";

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
		if (_commands.IsDefault || _commands.IsEmpty)
			InitializeCommandsInternal();

		PikeConsoleConfig.CheatMode.ValueChanged += OnCheatModeChangedInternal;
		OnEnterTree();

		RegisterCommandsInternal();
	}

	// Technically this override is useless now, but I am keeping it as-is for compatibility.
	// If we make use of it in the future, stuff wont break for anyone wanting to update their version.
	public sealed override void _Ready()
	{
		OnReady();
	}

	public sealed override void _ExitTree()
	{
		RuntimeExecutableRegistry.Unregister([.. _commands]);
		PikeConsoleConfig.CheatMode.ValueChanged -= OnCheatModeChangedInternal;
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
	static protected Command Command(
		string signature,
		string shortDesc,
		string longDesc,
		string usage,
		bool isCheat,
		Func<string[], Response<ExecutionResponseStatus>> action,
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0)
	{
		return new Command(signature, shortDesc, longDesc, [$"{usage}"], isCheat, action, new CustomStackTrace(filePath, lineNumber));
	}
	static protected Command Command(
		string signature,
		string shortDesc,
		string longDesc,
		string[] usages,
		bool isCheat,
		Func<string[], Response<ExecutionResponseStatus>> action,
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0)
	{
		return new Command(signature, shortDesc, longDesc, usages, isCheat, action, new CustomStackTrace(filePath, lineNumber));
	}

	/// <summary>
	/// Shorthand for creating undocumented commands.
	/// This will automatically inject a custom stack trace to the caller CommandSet 
	/// (compile time, no overhead) to the created command.
	/// </summary>
	/// <remarks>
	/// NOTE: It is recommended to use the documented version of this shorthand instead.
	/// If you absolutely do not care for documentation, use this override and set <c>PikeConsoleConfig.SUPPRESS_DOCUMENTATION_WARNINGS</c> to true.
	/// </remarks>
	/// <param name="signature">
	/// Command signature to run this executable, EG: "r_cleardecals" or "env_gc"<br/>
	/// A signature has no set naming rules, but it is convention to name it after scope separated using underscores.
	/// </param>
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
	static protected Command Command(
		string signature,
		bool isCheat,
		Func<string[], Response<ExecutionResponseStatus>> action,
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0)
	{
		return new Command(signature, null, null, null, isCheat, action, new CustomStackTrace(filePath, lineNumber));
	}


	// ----- ----- INTERNAL API ----- -----

	void OnCheatModeChangedInternal(bool b)
	{
		OnCheatModeChanged(b);
	}

	// We inject the filepath and linenumber so that we can reference the actual child that messed up instead of this base class.
	// This class is self-diagnostic by design. This ensures we fail fast and catch collisions early.
	void InitializeCommandsInternal()
	{
		// TODO: Add scriptpath lazy init from godot and use it to say the CommandSet childs name before the response.
		// This wil get around the filepath leading to the root CommandSet...
		try
		{
			_commands = ImmutableArray.Create(InstantiateCommands() ?? []);
		}
		catch (Exception ex)
		{
			_commands = [];
			PikeLogger.LogError(LogTarget.All, $"UNEXPECTED ERROR: Failed to instantiate commands Node: [({Name}){this}] - {ex}", forceLog: true, filePath: DerivedScriptPath, lineNumber: -1);
		}

		if (_commands.Length <= 0)
			PikeLogger.LogWarning(LogTarget.All, $"Commands failed to register from Node [({Name}){this}]. Make sure InstantiateCommands return a valid array!", forceLog: true, filePath: DerivedScriptPath, lineNumber: -1);
	}

	void RegisterCommandsInternal()
	{
		Response<RegisterExecutableResponseStatus>[] responses = RuntimeExecutableRegistry.Register([.. _commands]);

		// Self diagnose by checking all commands.
		foreach (Response<RegisterExecutableResponseStatus> response in responses)
		{
			switch (response.Status)
			{
				case RegisterExecutableResponseStatus.Success:
#if TOOLS
					// Stripped in compiled build so we don't even have to make the conditional check.
					if (PikeConsoleConfig.LogCommandOnRegister)
						PikeLogger.Log(LogTarget.Editor, $"{response.Message}", forceLog: true, filePath: DerivedScriptPath, lineNumber: -1);
#endif
					break;
				case RegisterExecutableResponseStatus.ReplacedAlias:
					PikeLogger.LogWarning(LogTarget.All, $"{response.Message}", forceLog: true, filePath: DerivedScriptPath, lineNumber: -1);
					break;
				case RegisterExecutableResponseStatus.AlreadyExists:
					PikeLogger.LogError(LogTarget.All, $"{response.Message}", forceLog: true, filePath: DerivedScriptPath, lineNumber: -1);
					break;
				// Unexpected error.
				default:
					PikeLogger.LogError(LogTarget.All, $"{response.Message}", forceLog: true, filePath: DerivedScriptPath, lineNumber: -1);
					break;
			}
		}
	}
}
