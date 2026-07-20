using System;
using System.Collections.Generic;
using System.Linq;
using FractalPike.PikeConsole.Core.Utilities;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Aliases;
public static class AliasRegistry
{
	static readonly Dictionary<string, string> _aliases = new(StringComparer.OrdinalIgnoreCase);


	/// <summary>
	/// Protective wrapper for <c>_aliases.TryGetValue(signature, out executable)</c>
	/// </summary>
	public static bool TryGetAlias(string signature, out string aliasStatement) => _aliases.TryGetValue(signature, out aliasStatement);
	public static void Clear() => _aliases.Clear();

	public static Response<RegisterAliasResponseStatus> Register(string signature, string input, bool replace = true)
	{
		signature = ConsoleFormatter.ToSignature(signature);

		if (RuntimeExecutableRegistry.TryGetExecutable(signature, out IRuntimeExecutable rte))
		{
			string type = rte is ICVar ? "cvar" : "command";
			return new(RegisterAliasResponseStatus.Occupied, $"Signature \"{signature}\" is occupied by a {type}!");
		}

		if (_aliases.TryGetValue(signature, out _))
		{
			if (!replace)
				return new(RegisterAliasResponseStatus.AlreadyExists, $"Alias \"{signature}\" already exists.");

			_aliases[signature] = input;
			return new(RegisterAliasResponseStatus.Replaced, $"\"{signature}\" has been overridden with a new statement.");
		}

		_aliases[signature] = input;
		return new(RegisterAliasResponseStatus.Success, $"Alias \"{signature}\" has been added.");
	}


	/// <summary>
	/// Unregister an alias from the aliases dictionary.
	/// </summary>
	/// <param name="signature">The alias (signature) to unregister.</param>
	/// <remarks>Does not return any confirmation or errors.</remarks>
	public static void Unregister(string signature) => _aliases.Remove(signature);

	/// <summary>
	/// Unregister a bunch of aliases in bulk from the aliases dictionary.
	/// </summary>
	/// <param name="signatures">The aliases (signatures) to unregister as any type of Enumerable.</param>
	/// <remarks>Does not return any confirmation or errors.</remarks>
	public static void Unregister(IEnumerable<string> signatures)
	{
		foreach (string signature in signatures)
			Unregister(signature);
	}

	// Note: 
	// The search methods are still kind of a mess since the Unity framework.
	// It uses quite heavy allocation and O(N log N) lookup.
	// There is a lot of room for optimization, but since this is only ever used by 
	// QA testers, developers and cheaters it's okay to waste a few ms for now. It's a cold path lookup.

	/// <summary>
	/// LINQ "SQL-style" lookup.
	/// Gets all aliases containing the term with an optional type filter.
	/// </summary>
	/// <param name="term">The search term to pass.</param>
	/// <param name="rankByPrefix">Place the results in order, prioritizing those who start with the term.</param>
	/// <param name="emptyMeansAll">If term is empty, return all matching the other parameters.</param>
	/// <returns>An array of IRuntimeExecutable[] (The results)</returns>
	public static string[] Search(string term, SearchMode mode = SearchMode.Contains, bool rankByPrefix = false)
	{
		// IMPORTANT PERFORMANCE BOOST!
		// If the mode is set to "exact", do a raw O(1) check and early return.
		if (mode == SearchMode.Exact)
		{
			if (!TryGetAlias(term, out var alias))
				return [];

			return [alias];
		}

		if (string.IsNullOrWhiteSpace(term))
			return [.. _aliases.Keys.OrderBy(c => c)];

		var comp = StringComparison.OrdinalIgnoreCase;

		var filtered = mode switch
		{
			SearchMode.StartsWith => _aliases.Keys.Where(c => c.StartsWith(term, comp)),
			_ => _aliases.Keys.Where(c => c.Contains(term, comp))
		};

		return rankByPrefix && mode != SearchMode.StartsWith
			? [.. filtered.OrderBy(c => c.StartsWith(term, comp) ? 0 : 1).ThenBy(c => c)]
			: [.. filtered.OrderBy(c => c)];
	}

}
