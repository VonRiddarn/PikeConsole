using FractalPike.PikeConsole.Core.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;


namespace FractalPike.PikeConsole.Core.RuntimeExecution;
public static class PersistentCVarRegistry
{
	static readonly Dictionary<string, ICVar> _persistentCvars = [];
	public static event Action<ICVar> ValueUpdated;

	/// <summary>
	/// Used to update the dictionary without triggering a save event.
	/// This is used on startup (cvar initialize internal) and when cvars are set using "ram_only" as the last parameter.
	/// </summary>
	/// <param name="signature">CVar signature (resource file name)</param>
	/// <param name="cvar">ICVar interface object</param>
	public static void Write(string signature, ICVar cvar) => _persistentCvars[signature] = cvar;

	/// <summary>
	/// Router to trigger the ValueUpdated event on the registry.
	/// Sends a reference to the ICVar that was updated. 
	/// The interface already contains anything we need to check and save the value.
	/// </summary>
	/// <remarks>
	/// Note: We are passing the CVar that WAS UPDATED. That means we cannot see a from-to comparison. Just the new value.
	/// </remarks>
	/// <param name="cvar">Reference to the CVar that was updated.</param>
	public static void Update(ICVar cvar)
	{
		if (_persistentCvars.ContainsKey(cvar.Signature))
		{
			ValueUpdated?.Invoke(cvar);
		}
		else
			PikeLogger.LogWarning(LogTarget.All, $"{cvar.Signature} is not in the persistent CVar registry, but tried to access it.");
	}

	/// <summary>
	/// Takes a snapshot of the registry in its current state.
	/// This allocates memory and should be used only when actually writing to file.
	/// </summary>
	public static ImmutableDictionary<string, ICVar> GetSnapshot() => _persistentCvars.ToImmutableDictionary();
}
