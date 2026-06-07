using Godot;
using System;

public readonly struct CustomStackTrace(string filePath = "", int lineNumber = 0)
{
	public readonly string FilePath = filePath;
	public readonly int LineNumber = lineNumber;

	public void Deconstruct(out string filePath, out int lineNumber)
	{
		filePath = FilePath;
		lineNumber = LineNumber;
	}
}
