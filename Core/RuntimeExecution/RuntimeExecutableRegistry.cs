using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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
			return new(RegisterExecutableResponseStatus.AlreadyExists, $"A {rteType} already exists for signature \"{signature}\"");
		}

		if (AliasRegistry.TryGetAlias(signature, out string _))
		{
			string type = executable is ICVar ? "cvar" : "command";
			AliasRegistry.Unregister(signature);
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

	/// <summary>
	/// LINQ "SQL-style" lookup.
	/// Gets all executables containing the term with an optional type filter.
	/// </summary>
	/// <remarks>For games with very large sets (thousands) of commands / CVars this might cause some overhead.
	/// Though executable querying is not part of the hot-path, so this should cover most usecases.</remarks>
	/// <param name="term">The search term to pass.</param>
	/// <param name="filterType">The type to filter by, EG: <code>typeof(CVar&lt; int &gt;)</code></param>
	/// <param name="rankByPrefix">Place the results in order, prioritizing those who start with the term.</param>
	/// <param name="emptyMeansAll">If term is empty, return all matching the other parameters.</param>
	/// <returns>An array of IRuntimeExecutable[] (The results)</returns>
	public static IRuntimeExecutable[] Search(
			string term,
			SearchMode mode = SearchMode.Contains,
			Type? filterType = null,
			bool rankByPrefix = false)
	{
		var filtered = filterType != null
			? _executables.Values.Where(filterType.IsInstanceOfType)
			: _executables.Values;

		if (string.IsNullOrWhiteSpace(term))
			return [.. filtered.OrderBy(c => c.Signature)];

		var comp = StringComparison.OrdinalIgnoreCase;
		filtered = mode switch
		{
			SearchMode.StartsWith => filtered.Where(c => c.Signature.StartsWith(term, comp)),
			SearchMode.Exact => filtered.Where(c => c.Signature.Equals(term, comp)),
			_ => filtered.Where(c => c.Signature.Contains(term, comp))
		};

		return rankByPrefix && mode != SearchMode.StartsWith
			? [.. filtered.OrderBy(c => c.Signature.StartsWith(term, comp) ? 0 : 1).ThenBy(c => c.Signature)]
			: [.. filtered.OrderBy(c => c.Signature)];
	}
}
