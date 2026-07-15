using System;
using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.Utilities;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Commands;

public sealed class Command : IRuntimeExecutable
{
	public string DisplayType { get; }
	public string Signature { get; }
	public string ShortDesc { get; }
	public string LongDesc { get; }
	public string[] Usages { get; }
	public bool IsCheat { get; }
	public string SourceLocation { get; }

	readonly Func<string[], Response<ExecutionResponseStatus>> _action;

	// Centralized fallback management. 
	// Keeping this static prevents per-object allocation.
	static int _nextFallbackIndex = 0;
	static Response<ExecutionResponseStatus> FallbackAction(string[] args) => new(ExecutionResponseStatus.Failed, "No action registered!");

	// Onsolete attribute hack that allows us to yell when someone tries to instantiate a command directly
	[Obsolete("Instantiating Commands directly bypasses Godot lifecycle safety-net! Inherit from CommandSet and use the Command() shorthand instead.")]
	public Command(
		string commandSignature,
		string shortDesc,
		string longDesc,
		string[] usages,
		bool isCheat,
		Func<string[], Response<ExecutionResponseStatus>> action,
		// We're sending the filePath and lineNumber from the CommandSet using compiler injection attributes.
		// This makes us able to track the exact file that is faulty.
		CustomStackTrace customStackTrace)
	{
		var (filePath, lineNumber) = customStackTrace;

		DisplayType = "Command";
		Signature = ConsoleFormatter.ToSignature(commandSignature);
		ShortDesc = string.IsNullOrWhiteSpace(shortDesc) ? "No description available." : shortDesc;
		LongDesc = string.IsNullOrWhiteSpace(longDesc) ? "No long description available." : longDesc;
		Usages = usages;
		IsCheat = isCheat;
		SourceLocation = $"{filePath}:{lineNumber}";

		_action = action;

		// Self diagnose errors to console and apply safe fallbacks...
		if (string.IsNullOrWhiteSpace(Signature))
		{
			Signature = $"FALLBACK_SIGNATURE_{_nextFallbackIndex++}";
			PikeLogger.LogError(LogTarget.All, $"A command has been created without a signature! Emergency-fallback: {Signature}", filePath: filePath, lineNumber: lineNumber, forceLog: true);
		}
		if (_action == null)
		{
			PikeLogger.LogError(LogTarget.All, $"Command \"{Signature}\" is being registered with no callback! This is safe due to fallbacks, but very bad!", filePath: filePath, lineNumber: lineNumber, forceLog: true);
			_action = FallbackAction;
		}

		if (!PikeConsoleConfig.SuppressDocumentationWarnings)
		{
			if (string.IsNullOrWhiteSpace(shortDesc))
				PikeLogger.LogWarning(LogTarget.Debug, $"Command \"{Signature}\" is being registered with no short description. This is safe but unadvised.", filePath: filePath, lineNumber: lineNumber, forceLog: true);
			if (usages is null or [] or [""] or [null])
				PikeLogger.LogWarning(LogTarget.Debug, $"Command \"{Signature}\" is being registered with no usage instructions. This is safe but unadvised.", filePath: filePath, lineNumber: lineNumber, forceLog: true);
		}
	}

	public Response<ExecutionResponseStatus> Execute(ExecutionSource executionSource, string[] args)
	{
		// Note: Actions can still return their own exception error messages.
		// We just wrap it so that if the API consumer does not catch their own error, we do it here.
		try
		{
			// If this is a cheat AND we are not the system AND cheatmode is off. Fail the execution.
			// The system passes this check though, so we can still pass map specific overrides and cool stuff.
			if (IsCheat && executionSource is not ExecutionSource.System && !PikeConsoleConfig.CheatMode.Value)
				return new(ExecutionResponseStatus.DeniedCheat, $"{Signature} is cheat protected!");

			return _action.Invoke(args);
		}
		catch (Exception e)
		{
			return new(ExecutionResponseStatus.Error, $"Uncaught exception caused by \"{Signature}\"\nin {SourceLocation}:\n{e.Message}");
		}
	}

	public string GetHelp() => ConsoleFormatter.FormatHelp(this);
}
