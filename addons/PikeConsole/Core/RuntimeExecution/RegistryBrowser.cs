using FractalPike.PikeConsole.Core.RuntimeExecution;
using FractalPike.PikeConsole.Core.RuntimeExecution.Aliases;
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
using System.Linq;

namespace FractalPike.PikeConsole.Core.RuntimeExecution;

// Note, this is just a wrapper class for registries. 
// Should we, for some reason, want to add another registry we update this wrapper too.

public static class RegistryBrowser
{

	/// <summary>
	/// Find all runtime executables (Commands + CVars) matching the query.
	/// </summary>
	/// <remarks>
	/// O(N log N) lookup. Should only be used in cold path.
	/// </remarks>
	/// <param name="term">Signature term</param>
	/// <param name="rankByPrefix">Starting matches first</param>
	public static IRuntimeExecutable[] FindExecutables(string term, SearchMode searchMode, bool rankByPrefix)
	{
		return RuntimeExecutableRegistry.Search<IRuntimeExecutable>(term, searchMode, rankByPrefix);
	}

	/// <summary>
	/// Find all Commands matching the query.
	/// </summary>
	/// <remarks>
	/// O(N log N) lookup. Should only be used in cold path.
	/// </remarks>
	/// <param name="term">Signature term</param>
	/// <param name="rankByPrefix">Starting matches first</param>
	public static Command[] FindCommands(string term, SearchMode searchMode, bool rankByPrefix)
	{
		return RuntimeExecutableRegistry.Search<Command>(term, searchMode, rankByPrefix);
	}

	/// <summary>
	/// Find all CVars matching the query.
	/// </summary>
	/// <remarks>
	/// O(N log N) lookup. Should only be used in cold path.
	/// </remarks>
	/// <param name="term">Signature term</param>
	/// <param name="rankByPrefix">Starting matches first</param>
	public static ICVar[] FindCVars(string term, SearchMode searchMode, bool rankByPrefix)
	{
		return RuntimeExecutableRegistry.Search<ICVar>(term, searchMode, rankByPrefix);
	}

	/// <summary>
	/// Find all aliases matching the query.
	/// </summary>
	/// <remarks>
	/// O(N log N) lookup. Should only be used in cold path.
	/// </remarks>
	/// <param name="term">Signature term</param>
	/// <param name="rankByPrefix">Starting matches first</param>
	public static Alias[] FindAliases(string term, SearchMode searchMode, bool rankByPrefix)
	{
		string[] signatures = AliasRegistry.Search(term, searchMode, rankByPrefix);
		Alias[] aliases = new Alias[signatures.Length];

		for (int i = 0; i < aliases.Length; i++)
		{
			string signature = signatures[i];
			string statement = AliasRegistry.TryGetAlias(signature, out string stmt) ? stmt : "Error fetching statement";

			aliases[i] = new Alias(signature, statement);
		}

		return aliases;
	}

	/// <summary>
	/// Find all signatures matching the query.
	/// </summary>
	/// <remarks>
	/// O(N log N) lookup. Should only be used in cold path.
	/// </remarks>
	/// <param name="term">Signature term</param>
	/// <param name="rankByPrefix">Starting matches first</param>
	public static string[] FindSignatures(string term, bool includeAliases)
	{
		SearchMode searchMode = SearchMode.StartsWith;

		string[] rtes = RuntimeExecutableRegistry.SearchSignatures<IRuntimeExecutable>(term, searchMode, true);
		if (!includeAliases)
			return rtes;

		string[] aliases = AliasRegistry.Search(term, searchMode, true);

		return [.. rtes.Concat(aliases).Order()];
	}

	/// <summary>
	/// Find all signatures matching the query using a type filter.
	/// </summary>
	/// <remarks>
	/// O(N log N) lookup. Should only be used in cold path.
	/// </remarks>
	/// <param name="term">Signature term</param>
	/// <param name="rankByPrefix">Starting matches first</param>
	public static string[] FindSignatures<T>(string term) where T : IRuntimeExecutable
	{
		SearchMode searchMode = SearchMode.StartsWith;

		return RuntimeExecutableRegistry.SearchSignatures<T>(term, searchMode, true);
	}
}
