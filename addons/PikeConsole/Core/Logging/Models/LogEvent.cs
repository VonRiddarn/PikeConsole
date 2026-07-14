using System;

namespace FractalPike.PikeConsole.Core.Logging;

/// <summary>
/// A struct used for outwards communications from the PikeLogger.
/// </summary>
public readonly struct LogEvent(int callerKeyHash, LogLevel logLevel, string message, bool forceLog, string[] tags, string sourcePath = "")
{
	/// <summary>
	/// Unique key built using the callers filepath and linenumber. Used by listeners for throttling.
	/// </summary>
	public readonly int CallerKeyHash = callerKeyHash;

	/// <summary>
	/// The "severity" or "category" of the log. (EG: LogLevel.Info | LogLevel.Error)
	/// </summary>
	public readonly LogLevel LogLevel = logLevel;
	/// <summary>
	/// The log message in string format.
	/// </summary>
	public readonly string Message = message;

	/// <summary>
	/// Flag for listeners. Used to bypass throttling.
	/// </summary>
	public readonly bool ForceLog = forceLog;

	/// <summary>
	/// Optional tags that can be appended to the event to be used for anything from filtering to formatting.
	/// </summary>
	public readonly string[] Tags = tags;

	/// <summary>
	/// The caller path in plaintext. Empty if "includePath" was not checked in the PikeLogger.
	/// </summary>
	public readonly string SourcePath = sourcePath;

	/// <summary>
	/// Compares many tags and returns true if any one exists in the tags array.
	/// </summary>
	/// <param name="searchTags"></param>
	/// <returns></returns>
	public bool HasAnyTag(string[] searchTags)
	{
		if (Tags == null || Tags.Length == 0)
			return false;

		for (int i = 0; i < searchTags.Length; i++)
		{
			// Note: This is faster than LINQ's "Contains" and zero allocating.
			if (Array.IndexOf(Tags, searchTags[i]) >= 0)
				return true;
		}

		return false;
	}

	/// <summary>
	/// Fetches the first instance of a tag and outputs it into the tag parameter.
	/// Useful for tag based switch statements.
	/// </summary>
	/// <param name="searchTags"></param>
	/// <param name="tag"></param>
	/// <returns></returns>
	public bool TryGetAnyTag(string[] searchTags, out string tag)
	{
		tag = string.Empty;

		if (Tags == null || Tags.Length == 0)
			return false;

		for (int i = 0; i < searchTags.Length; i++)
		{
			int index = Array.IndexOf(Tags, searchTags[i]);
			// Note: This is faster than LINQ's "Contains" and zero allocating.
			if (index >= 0)
			{
				tag = Tags[index];
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Compares for one tag and returns true if it exists in the tags array.
	/// </summary>
	/// <param name="searchTag"></param>
	/// <returns></returns>
	public bool HasTag(string searchTag)
	{
		if (Tags == null || Tags.Length == 0)
			return false;

		return Array.IndexOf(Tags, searchTag) >= 0;
	}
}
