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
	public bool IsLocal { get; }
	public bool IsCheat { get; }

	readonly Func<string[], Response<ExecutionResponseStatus>> _action;

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

		// Self diagnose errors to console...
		if (string.IsNullOrWhiteSpace(Signature))
			PikeLogger.LogError(LogTarget.All, $"FATAL RISK DETECTED: A command has been created without a signature!", filePath: filePath, lineNumber: lineNumber, forceLog: true);
		if (action == null)
			PikeLogger.LogError(LogTarget.All, $"Command \"{Signature}\" is being registered with no callback! This is safe due to fallbacks, but very bad!", filePath: filePath, lineNumber: lineNumber, forceLog: true);
		if (string.IsNullOrWhiteSpace(shortDesc))
			PikeLogger.LogWarning(LogTarget.Debug, $"Command \"{Signature}\" is being registered with no short description. This is safe but unadviced.", filePath: filePath, lineNumber: lineNumber, forceLog: true);
		if (string.IsNullOrWhiteSpace(usage))
			PikeLogger.LogWarning(LogTarget.Debug, $"Command \"{Signature}\" is being registered with no usage instructions. This is safe but unadviced.", filePath: filePath, lineNumber: lineNumber, forceLog: true);
	}


	// TODO: CONTINUE WITH THIS!!!

	public Response<ExecutionResponseStatus> Execute(string[] args)
	{
		throw new System.NotImplementedException();
	}

	public string GetHelp()
	{
		throw new System.NotImplementedException();
	}
}
