using System;
using FractalPike.PikeConsole.Core.Extensions;
using FractalPike.PikeConsole.Core.Logging;

namespace FractalPike.PikeConsole.Core.RuntimeExecution;

public sealed class Command : IRuntimeExecutable
{
	public string DisplayType { get; }
	public string Signature { get; }
	public string ShortDesc { get; }
	public string LongDesc { get; }
	public string Usage { get; }
	public bool IsCheat { get; }

	readonly Func<string[], Response<ExecutionResponseStatus>> _action;

	// Centralized fallback management. 
	// Keeping this static prevents per-object allocation.
	static int _nextFallbackIndex = 0;
	static Response<ExecutionResponseStatus> FallbackAction(string[] args) => new(ExecutionResponseStatus.Failed, "No action registered!");

	public Command(
		string commandSignature,
		string shortDesc,
		string longDesc,
		string usage,
		bool isCheat,
		Func<string[], Response<ExecutionResponseStatus>> action,
		// We're sending the filePath and lineNumber from the CommandSet using compiler injection attributes.
		// This makes us able to track the exact file that is faulty.
		CustomStackTrace customStackTrace)
	{
		DisplayType = "Command";
		Signature = commandSignature.ToSignature();
		ShortDesc = string.IsNullOrWhiteSpace(shortDesc) ? "No description available." : shortDesc;
		LongDesc = string.IsNullOrWhiteSpace(longDesc) ? "No long description available." : longDesc;
		Usage = string.IsNullOrWhiteSpace(usage) ? "No usage instructions available." : usage;
		IsCheat = isCheat;
		_action = action;

		var (filePath, lineNumber) = customStackTrace;

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

		if (!PikeConsoleConfig.SUPPRESS_DOCUMENTATION_WARNINGS)
		{
			if (string.IsNullOrWhiteSpace(shortDesc))
				PikeLogger.LogWarning(LogTarget.Debug, $"Command \"{Signature}\" is being registered with no short description. This is safe but unadvised.", filePath: filePath, lineNumber: lineNumber, forceLog: true);
			if (string.IsNullOrWhiteSpace(usage))
				PikeLogger.LogWarning(LogTarget.Debug, $"Command \"{Signature}\" is being registered with no usage instructions. This is safe but unadvised.", filePath: filePath, lineNumber: lineNumber, forceLog: true);
		}
	}


	// TODO: CONTINUE WITH THIS!!!

	public Response<ExecutionResponseStatus> Execute(string[] args)
	{
		// TODO: Add try.catch so we can send back exceptions.
		throw new System.NotImplementedException();
	}

	public string GetHelp()
	{
		throw new System.NotImplementedException();
	}
}
