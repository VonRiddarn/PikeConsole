using System;
using System.Collections.Generic;
using System.Linq;

namespace FractalPike.PikeConsole.Core.RuntimeExecution;
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


	// TODO: TEST ALIAS SEARCH : REMOVE THIS TODO WHEN TESTED!!!!

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
		if (string.IsNullOrWhiteSpace(term))
			return [.. _aliases.Keys.OrderBy(c => c)];

		var comp = StringComparison.OrdinalIgnoreCase;

		var filtered = mode switch
		{
			SearchMode.StartsWith => _aliases.Keys.Where(c => c.StartsWith(term, comp)),
			SearchMode.Exact => _aliases.Keys.Where(c => c.Equals(term, comp)),
			_ => _aliases.Keys.Where(c => c.Contains(term, comp))
		};

		return rankByPrefix && mode != SearchMode.StartsWith
			? [.. filtered.OrderBy(c => c.StartsWith(term, comp) ? 0 : 1).ThenBy(c => c)]
			: [.. filtered.OrderBy(c => c)];
	}

}
