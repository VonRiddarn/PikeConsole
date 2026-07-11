using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FractalPike.PikeConsole.Core.RuntimeExecution.Aliases;
using FractalPike.PikeConsole.Core.Utilities;

/* 
	2026-07-02
	Important note to self (and others)
		
		This is a thread safe class by implicit usage. Yes, we can technically instantiate a Node class on another thread using "new",
		but all de-facto in-game Nodes in the SceneTree are living on the main thread. This means
		there is no way to break the command system as long as the CommandSet Node is used as intended.

		TL;DR:
		The command is always registered on the main thread by implicit, 
		as _EnterTree is always fired on the main thread.

		NOTE TO SELF AGAIN (FOR CLARITY): 
			
			STOP COMING BACK HERE EVERY OTHER DAY WASTING TIME MAKING IT THREAD SAFE!! 
			IT DOESN'T NEED TO BE AND DOESN'T NEED A CONCURRENT DICTIONARY!
*/

#nullable enable
namespace FractalPike.PikeConsole.Core.RuntimeExecution;

public static class RuntimeExecutableRegistry
{
	public static IReadOnlyDictionary<string, IRuntimeExecutable> Executables => _executables;
	static readonly Dictionary<string, IRuntimeExecutable> _executables = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Protective and pragmatic wrapper for <c>_executables.TryGetValue(signature, out executable)</c>
	/// </summary>
	public static bool TryGetExecutable(string signature, out IRuntimeExecutable? executable)
	{
		return _executables.TryGetValue(signature, out executable);
	}

	public static Response<RegisterExecutableResponseStatus> Register(IRuntimeExecutable executable)
	{
		// Cache in stack since we reuse it like 4 times.
		string signature = executable.Signature;

		if (_executables.TryGetValue(signature, out var rte))
		{
			string rteType = rte is ICVar ? "cvar" : "command";
			return new(RegisterExecutableResponseStatus.AlreadyExists, $"A {rteType} already exists for signature \"{signature}\".\nConflict found at: {rte.SourceLocation}");
		}

		if (AliasRegistry.TryGetAlias(signature, out string _))
		{
			string type = executable is ICVar ? "cvar" : "command";
			AliasRegistry.Unregister(signature);
			_executables[signature] = executable;
			return new(RegisterExecutableResponseStatus.ReplacedAlias, $"Alias \"{signature}\" has been overridden by a {type} with the same signature!");
		}

		_executables[signature] = executable;
		return new(RegisterExecutableResponseStatus.Success, $"Registered command: \"{signature}\"");
	}


	/// <summary>
	/// Register executables to the registry in bulk.
	/// Used internally by the CommandSet class.
	/// </summary>
	/// <returns>An array of <c>Response&lt; RegisterExecutableResponseStatus &gt;[]</c></returns>
	public static Response<RegisterExecutableResponseStatus>[] Register(IRuntimeExecutable[] executables)
	{
		var responses = new Response<RegisterExecutableResponseStatus>[executables.Length];

		for (int i = 0; i < executables.Length; i++)
			responses[i] = Register(executables[i]);

		return responses;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Unregister(IRuntimeExecutable executable)
	{
		_executables.Remove(executable.Signature);
	}

	/// <summary>
	/// Unregister executable from the registry in bulk.
	/// Used internally by the CommandSet class.
	/// </summary>
	public static void Unregister(IRuntimeExecutable[] executables)
	{
		foreach (IRuntimeExecutable executable in executables)
			Unregister(executable);
	}

	// TODO: TEST RUNTIME EXECUTABLE SEARCH : REMOVE THIS TODO WHEN TESTED!!!!


	// Note: 
	// The search methods are still kind of a mess since the Unity framework.
	// It uses quite heavy allocation and O(N log N) lookup for commands + cvars.
	// There is a lot of room for optimization, but since this is only ever used by 
	// QA testers, developers and cheaters it's okay to waste a few ms for now. It's a cold path lookup.

	/// <summary>
	/// LINQ "SQL-style" lookup.
	/// Gets all executables containing the term with an optional type filter.
	/// </summary>
	/// <remarks>For games with very large sets (thousands) of commands / CVars this might cause some overhead.
	/// Though executable querying is not part of the hot-path, so this should cover most usecases.</remarks>
	/// <param name="term">The search term to pass.</param>
	/// <param name="mode">Search mode for filtering.</param>
	/// <param name="rankByPrefix">Place the results in order, prioritizing those who start with the term.</param>
	/// <returns>An array of IRuntimeExecutable[] (The results)</returns>
	public static T[] Search<T>(
		string term,
		SearchMode mode = SearchMode.Contains,
		bool rankByPrefix = false) where T : IRuntimeExecutable
	{
		// IMPORTANT PERFORMANCE BOOST!
		// If the mode is set to "exact", do a raw O(1) check and early return.
		if (mode == SearchMode.Exact)
		{
			if (!TryGetExecutable(term, out var rte) || rte is not T typedRte)
				return [];

			return [typedRte];
		}

		var filtered = _executables.Values.OfType<T>();

		if (string.IsNullOrWhiteSpace(term))
			return [.. filtered.OrderBy(c => c.Signature)];

		var comp = StringComparison.OrdinalIgnoreCase;

		filtered = mode switch
		{
			SearchMode.StartsWith => filtered.Where(c => c.Signature.StartsWith(term, comp)),
			_ => filtered.Where(c => c.Signature.Contains(term, comp))
		};

		return rankByPrefix && mode != SearchMode.StartsWith
			? [.. filtered.OrderBy(c => c.Signature.StartsWith(term, comp) ? 0 : 1).ThenBy(c => c.Signature)]
			: [.. filtered.OrderBy(c => c.Signature)];
	}

	/// <summary>
	/// LINQ "SQL-style" lookup.
	/// Gets all SIGNATURES of the executables containing the term with an optional type filter.
	/// </summary>
	/// <remarks>For games with very large sets (thousands) of commands / CVars this might cause some overhead.
	/// Though executable querying is not part of the hot-path, so this should cover most usecases.</remarks>
	/// <param name="term">The search term to pass.</param>
	/// <param name="mode">Search mode for filtering.</param>
	/// <param name="rankByPrefix">Place the results in order, prioritizing those who start with the term.</param>
	/// <returns>An array of IRuntimeExecutable[] (The results)</returns>
	public static string[] SearchSignatures<T>(
	string term,
	SearchMode mode = SearchMode.Contains,
	bool rankByPrefix = false) where T : IRuntimeExecutable
	{
		// IMPORTANT PERFORMANCE BOOST!
		// If the mode is set to "exact", do a raw O(1) check and early return.
		if (mode == SearchMode.Exact)
		{
			if (!TryGetExecutable(term, out var rte) || rte is not T typedRte)
				return [];

			return [typedRte.Signature];
		}

		var filtered = _executables.Values.OfType<T>();

		if (string.IsNullOrWhiteSpace(term))
			return [.. filtered.Select(c => c.Signature).OrderBy(s => s)];

		var comp = StringComparison.OrdinalIgnoreCase;

		filtered = mode switch
		{
			SearchMode.StartsWith => filtered.Where(c => c.Signature.StartsWith(term, comp)),
			_ => filtered.Where(c => c.Signature.Contains(term, comp))
		};

		return rankByPrefix && mode != SearchMode.StartsWith
			? [.. filtered.OrderBy(c => c.Signature.StartsWith(term, comp) ? 0 : 1).ThenBy(c => c.Signature).Select(c => c.Signature)]
			: [.. filtered.OrderBy(c => c.Signature).Select(c => c.Signature)];
	}
}
