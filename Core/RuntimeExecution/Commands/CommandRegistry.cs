using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

#nullable enable
namespace FractalPike.PikeConsole.Core.RuntimeExecution;

public static class CommandRegistry
{
	public static IReadOnlyDictionary<string, IRuntimeExecutable> Commands => _commands;
	static readonly Dictionary<string, IRuntimeExecutable> _commands = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Protective and pragmatic wrapper for <c>_commands.TryGetValue(signature, out command)</c>
	/// </summary>
	public static bool TryGetCommand(string signature, out IRuntimeExecutable? command)
	{
		return _commands.TryGetValue(signature, out command);
	}

	public static Response<RegisterCommandResponseStatus> Register(IRuntimeExecutable command)
	{
		// TODO: Add registration when CVars are ported.
		// They are needed for type-checking the response.
		// BIG REFACTOR FROM UNTIY FRAMEWORK!!
		// THE REGISTRY WILL LOG FAILURES. This makes the registry self diagnostic.
		// LogTarget.All - This will allow players to report bugs from a compiled release.
		throw new NotImplementedException();
	}

	/// <summary>
	/// Register commands to the registry in bulk.
	/// Used internally by the CommandSet class.
	/// </summary>
	/// <param name="commands">IEnumerable of commands. Fast path expects an array.</param>
	/// <returns>An array of <c>Response&lt; RegisterCommandResponseStatus &gt;[]</c></returns>
	public static Response<RegisterCommandResponseStatus>[] Register(IEnumerable<IRuntimeExecutable> commands)
	{
		// Due to the opinionated design in CommandSet we are passing an array 9/10 times.
		// Fast path the array or spread it to store state.
		var commandList = commands as IRuntimeExecutable[] ?? [.. commands];
		int count = commandList.Length;

		// Allocate an array of responses that is exactly the size of commands to process.
		var responses = new Response<RegisterCommandResponseStatus>[count];

		for (int i = 0; i < count; i++)
			responses[i] = Register(commandList[i]);

		return responses;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Unregister(IRuntimeExecutable command)
	{
		_commands.Remove(command.Signature);
	}

	/// <summary>
	/// Unregister commands from the registry in bulk.
	/// Used internally by the CommandSet class.
	/// </summary>
	/// <param name="commands">IEnumerable of commands. Fast path expects an array.</param>
	public static void Unregister(IEnumerable<IRuntimeExecutable> commands)
	{
		foreach (IRuntimeExecutable command in commands)
			Unregister(command);
	}

	/// <summary>
	/// LINQ "SQL-style" lookup.
	/// Gets all commands containing the term with an optional type filter.
	/// </summary>
	/// <remarks>For games with very large sets (thousands) of commands / CVars this might cause some overhead.
	/// Though command querying is not part of the hot-path, so this should cover most usecases.</remarks>
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
		var query = _commands.Values.AsEnumerable();

		if (!string.IsNullOrWhiteSpace(term))
		{
			query = mode switch
			{
				SearchMode.Contains => query.Where(c => c.Signature.Contains(term, StringComparison.OrdinalIgnoreCase)),
				SearchMode.StartsWith => query.Where(c => c.Signature.StartsWith(term, StringComparison.OrdinalIgnoreCase)),
				SearchMode.Exact => query.Where(c => c.Signature.Equals(term, StringComparison.OrdinalIgnoreCase)),
				_ => query
			};
		}

		if (filterType != null)
			query = query.Where(filterType.IsInstanceOfType);

		if (rankByPrefix && mode != SearchMode.StartsWith)
			query = query.OrderBy(c => c.Signature.StartsWith(term, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
						 .ThenBy(c => c.Signature);
		else
			query = query.OrderBy(c => c.Signature);

		return [.. query];
	}
}
