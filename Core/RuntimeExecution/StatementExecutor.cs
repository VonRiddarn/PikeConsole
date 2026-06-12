using System;
using System.Collections.Generic;
using System.Linq;
using FractalPike.PikeConsole.Core.Logging;

namespace FractalPike.PikeConsole.Core.RuntimeExecution;

public static class StatementExecutor
{
	// New feature: Capped recursion depth prevents infinite loops even without exact signature matches
	// <= 0 is infinite
	// TODO: Turn this into a project setting later! (This goes under /runtime with UI logs)
	const int MAX_ALIAS_DEPTH = 128;

	/// <summary>
	/// Try to execute a command or alias matching the signature with the passed arguments.
	/// </summary>
	/// <param name="signature">The command or alias to execute.</param>
	/// <param name="args">Arguments to pass with to the command or alias.</param>
	public static void Execute(string signature, string[] args)
	{
		// We use a private internal method so recursion tracking is hidden from the public API
		ExecuteInternal(signature, args, null);
	}

	static void ExecuteInternal(string signature, string[] args, Stack<string> callStack)
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
			Response<ExecutionResponseStatus> response = executable.Execute(args);

			// Log messages if the command actually returned one
			if (!string.IsNullOrWhiteSpace(response.Message))
			{
				switch (response.Status)
				{
					// Using fallthrough cases so that we can map certain statuses to different log severities.
					case ExecutionResponseStatus.Success:
						PikeLogger.LogSuccess(LogTarget.AnyRuntime, $"{response.Message}", forceLog: true);
						break;
					case ExecutionResponseStatus.InvalidArgs:
					case ExecutionResponseStatus.Failed:
					case ExecutionResponseStatus.DeniedPermission:
					case ExecutionResponseStatus.DeniedCheat:
						PikeLogger.LogError(LogTarget.AnyRuntime, $"{response.Message}", forceLog: true, includePath: false);
						break;
					default:
					case ExecutionResponseStatus.Error:
						PikeLogger.LogError(LogTarget.AnyRuntime, $"{response.Message}", forceLog: true, includePath: true);
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

				ExecuteInternal(statements[i].Signature, targetArgs, callStack);
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