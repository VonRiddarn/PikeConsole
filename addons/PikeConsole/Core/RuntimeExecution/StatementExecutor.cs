using System;
using System.Collections.Generic;
using System.Linq;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution.Aliases;

namespace FractalPike.PikeConsole.Core.RuntimeExecution;

public static class StatementExecutor
{
	// New feature: Capped recursion depth prevents infinite loops even without exact signature matches
	// <= 0 is infinite
	// TODO: Turn this into a CVAr later! Could be cool! console_max_alias_execution_depth
	const int MAX_ALIAS_DEPTH = 128;

	/// <summary>
	/// Try to execute a command or alias matching the signature with the passed arguments.
	/// </summary>
	/// <param name="source">The entitty that wants to execute the command (Player or System)</param>
	/// <param name="signature">The command or alias to execute.</param>
	/// <param name="args">Arguments to pass with to the command or alias.</param>
	/// <param name="silent">Supress "success" logs.</param>
	public static void Execute(ExecutionSource executionSource, string signature, string[] args, bool silent = false)
	{
		// We use a private internal method so recursion tracking is hidden from the public API
		ExecuteInternal(executionSource, signature, args, silent, null);
	}

	/// <summary>
	/// Execute raw statement (previously called "ExecuteLine" in Unity. <br />
	/// This makes us able to execute arbitrary statements without using the ConfigIO file.
	/// </summary>
	/// <param name="source">The entitty that wants to execute the command (Player or System)</param>
	/// <param name="rawInput">A raw input line to parse and execute.</param>
	/// <param name="silent">Supress "success" logs.</param>
	public static void Execute(ExecutionSource source, string rawInput, bool silent = false)
	{
		ParsedStatement[] statements = StatementParser.ParseLine(rawInput);
		foreach (var s in statements)
		{
			ExecuteInternal(source, s.Signature, s.Arguments, silent, null);
		}
	}

	/// <summary>
	/// Execute statements already parsed by the StatementParser.
	/// </summary>
	/// <param name="source">The entitty that wants to execute the command (Player or System)</param>
	/// <param name="parsedStatements">A list of pre-parsed statement structs.</param>
	/// <param name="silent">Supress "success" logs.</param>
	public static void Execute(ExecutionSource source, ParsedStatement[] parsedStatements, bool silent = false)
	{
		foreach (var s in parsedStatements)
		{
			ExecuteInternal(source, s.Signature, s.Arguments, silent, null);
		}
	}

	static void ExecuteInternal(ExecutionSource executionSource, string signature, string[] args, bool silent, Stack<string> callStack)
	{
		if (callStack != null)
		{
			if (MAX_ALIAS_DEPTH > 0 && callStack.Count >= MAX_ALIAS_DEPTH)
			{
				PikeLogger.LogError(LogTarget.All, $"Alias max recursion depth reached ({MAX_ALIAS_DEPTH}). Aborting...", forceLog: true, includePath: false);
				return;
			}

			if (callStack.Contains(signature, StringComparer.OrdinalIgnoreCase))
			{
				PikeLogger.LogError(LogTarget.All, $"Alias recursion detected: {string.Join(" -> ", callStack.Reverse())} -> {signature}. Aborting...", forceLog: true, includePath: false);
				return;
			}
		}

		// ----- ----- COMMAND EXECUTION ----- -----
		if (RuntimeExecutableRegistry.TryGetExecutable(signature, out IRuntimeExecutable executable))
		{
			Response<ExecutionResponseStatus> response = executable.Execute(executionSource, args);

			// Log messages if the command actually returned one
			if (!string.IsNullOrWhiteSpace(response.Message))
			{
				switch (response.Status)
				{
					case ExecutionResponseStatus.Success:
						if (!silent)
							PikeLogger.LogSuccess(LogTarget.Runtime, $"{response.Message}", forceLog: true, includePath: false, tags: [.. response.Flags]);
						break;

					case ExecutionResponseStatus.InvalidArgs:
						PikeLogger.LogWarning(LogTarget.Runtime, $"{response.Message}", forceLog: true, includePath: false, tags: [.. response.Flags, RuntimeExecutionLogTags.InvalidArgs]);
						break;

					case ExecutionResponseStatus.Failed:
						PikeLogger.LogWarning(LogTarget.Runtime, $"{response.Message}", forceLog: true, includePath: false, tags: [.. response.Flags, RuntimeExecutionLogTags.Failed]);
						break;

					case ExecutionResponseStatus.DeniedCheat:
						PikeLogger.LogWarning(LogTarget.Runtime, $"{response.Message}", forceLog: true, includePath: false, tags: [.. response.Flags, RuntimeExecutionLogTags.DeniedCheat]);
						break;

					case ExecutionResponseStatus.Error:
					default:
						PikeLogger.LogError(LogTarget.Runtime, $"{response.Message}", forceLog: true, includePath: true, tags: [.. response.Flags]);
						break;
				}
			}

			return;
		}

		// ----- ----- ALIAS EXECUTION ----- -----
		// Else if is technically not needed, but we want to be SURE this NEVER runs if a command is found.
		else if (AliasRegistry.TryGetAlias(signature, out string aliasStatement))
		{
			callStack ??= [];
			callStack.Push(signature);

			ParsedStatement[] statements = StatementParser.ParseLine(aliasStatement);

			for (int i = 0; i < statements.Length; i++)
			{
				string[] targetArgs = statements[i].Arguments;

				// OLD (bad): foreach (ParsedStatement ps in StatementParser.ParseLine($"{input} {string.Join(' ', args)}"))
				// Biggest upgrade from Unity framework!!
				// Instead of force-passing the external arguments using interpolation we 
				// Just append it using spread operatiors before executing the last alias!

				if (i == statements.Length - 1 && args.Length > 0)
				{
					targetArgs = [.. targetArgs, .. args];
				}

				ExecuteInternal(executionSource, statements[i].Signature, targetArgs, silent, callStack);
			}

			callStack.Pop();
			return;
		}
		else // Once again, else is not needed, but if we ever accidentally remove a return we are safe.
		{
			// ----- ----- NOT FOUND ----- -----
			PikeLogger.Log(LogTarget.All, $"Unknown command: \"{signature}\"", forceLog: true);
			return;
		}

		/*
		 * NOTE TO SELF:
		 * 		The TryExecuteFromFile method has been removed from this script.
		 * 		DO NOT RE-ADD IT! It is a violation of concerns.
		 * 		It should be part of the RuntimeExecution.Config namespace instead!
		*/
	}
}