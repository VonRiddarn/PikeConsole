using System;
using System.Collections.Generic;

namespace FractalPike.PikeConsole.Core.RuntimeExecution;

/*
    This script handles any and all parsing of command input.
    That does NOT mean all valid-parses are actual commands.

    The system parses input and sends back a struct of commands that can be tried,
    the system does not check against the registry to see if the commands are valid.

    AUTHORS NOTE:
        The old system used a regex and LINQ which resulted in MAD string allocations and lots of substringing.
        It was fine for cold path usage, but since we are porting from Unity I also wanted to make it less allocating.
        Full transparency: AI was used to assist in translating the old LINQ / Regex logic into using a ReadOnlySpan statemachine.
        I have gone through the code and made sure I can put my name on it. I also added comments so that it is readable. 
*/

public static class StatementParser
{
	/// <summary>
	/// Used to parse an unknown input string and turn it into valid statements. <br />
	/// This could be user input in the console, or a line from a cfg file.
	/// </summary>
	/// <param name="input">Raw text statement, EG: <br />
	/// <c>[echo "hello world"; echo testing 1 2 3]</c></param>
	/// <returns>
	/// All valid statements parsed from the line, EG: <br />
	/// ("echo", ["hello world"]) ("echo", ["1", "2", "3"])
	/// </returns>
	public static ParsedStatement[] ParseLine(string input)
	{
		if (string.IsNullOrWhiteSpace(input))
			return [];

		List<ParsedStatement> results = [];
		ReadOnlySpan<char> span = input.AsSpan();

		// In quotes is used to ignore semicolons etc
		bool inQuotes = false;
		// Is eascaped is used to escape quotes (and other special characters)
		// It is implemented in a way where we can re-use for basically anything if we add more custom rules.
		bool isEscaped = false;

		int start = 0;

		for (int i = 0; i < span.Length; i++)
		{
			char c = span[i];

			// If we are in escape mode, ignore parsing, disable and continue.
			if (isEscaped)
			{
				isEscaped = false;
				continue;
			}

			// Activate escape mode.
			// This is only active for 1 character
			if (c == '\\')
			{
				isEscaped = true;
				continue;
			}

			if (c == '"')
			{
				inQuotes = !inQuotes;
				continue;
			}

			// Comment declaration detected (//)
			// Ignore anything that comes after it on this line.
			if (!inQuotes && c == '/' && i + 1 < span.Length && span[i + 1] == '/')
			{
				var statementSpan = span[start..i];

				// if (ParseStatementInternal(statementSpan) is { } parsedCmd)
				// This is kinda like writing "x = parsed; if x != null;" Cool / useful shorthand
				if (ParseStatementInternal(statementSpan) is { } parsedCmd)
					results.Add(parsedCmd);

				// Early return since the rest is a comment anyway
				return [.. results];
			}

			// If we hit a statement separator and are not in quotes, we split the statement here.
			if (!inQuotes && c == ';')
			{
				var statementSpan = span[start..i];
				start = i + 1;

				if (ParseStatementInternal(statementSpan) is { } parsedCmd)
					results.Add(parsedCmd);
			}
		}

		// Collect the final statement even if the line ended without a statement separator
		if (start < span.Length)
		{
			var statementSpan = span[start..];
			if (ParseStatementInternal(statementSpan) is { } parsedCmd)
				results.Add(parsedCmd);
		}

		return [.. results];
	}

	/// <summary>
	/// Parses a single statement slice into a signature and its arguments.
	/// Example: <c>echo "hello world" my name is peter</c><br />
	/// Returns: <c>echo</c>, <c>["hello world", "my", "name", "is", "peter"]</c>
	/// </summary>
	static ParsedStatement? ParseStatementInternal(ReadOnlySpan<char> statementSpan)
	{
		statementSpan = statementSpan.Trim();
		if (statementSpan.IsEmpty) return null;

		List<string> tokens = [];
		bool inQuotes = false;
		bool isEscaped = false;
		int tokenStart = 0;

		for (int i = 0; i < statementSpan.Length; i++)
		{
			char c = statementSpan[i];

			if (isEscaped)
			{
				isEscaped = false;
				continue;
			}

			if (c == '\\')
			{
				isEscaped = true;
				continue;
			}

			if (c == '"')
			{
				inQuotes = !inQuotes;
			}
			else if (char.IsWhiteSpace(c) && !inQuotes)
			{
				if (i > tokenStart)
				{
					tokens.Add(ExtractToken(statementSpan[tokenStart..i]));
				}
				tokenStart = i + 1;
			}
		}

		// Capture the final token
		if (tokenStart < statementSpan.Length)
		{
			tokens.Add(ExtractToken(statementSpan[tokenStart..]));
		}

		if (tokens.Count == 0) return null;

		string signature = tokens[0];

		// Allocate the arguments array exactly once
		string[] args = new string[tokens.Count - 1];
		tokens.CopyTo(1, args, 0, args.Length);

		return new ParsedStatement(signature, args);
	}

	/// <summary>
	/// Takes the token and turn it into an actual string.
	/// </summary>
	static string ExtractToken(ReadOnlySpan<char> tokenSpan)
	{
		tokenSpan = tokenSpan.Trim();

		if (tokenSpan.Length >= 2 && tokenSpan[0] == '"' && tokenSpan[^1] == '"')
		{
			tokenSpan = tokenSpan[1..^1];
		}

		string finalToken = tokenSpan.ToString();

		// NOTE: This is a slow path allocation.
		// 90% of the time we do not use escape characters in the statements, so we just skip this part.
		// If a statement contains escape characters though, we must allocate in order to format it propperly.
		if (finalToken.Contains('\\'))
		{
			finalToken = finalToken.Replace("\\\"", "\"").Replace("\\\\", "\\").Replace("\\;", ";");
		}

		return finalToken;
	}
}