using System.Text;
using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.Utilities;
using Godot;

namespace FractalPike.PikeConsole.Frontend.Controllers;

public partial class OutputController : RichTextLabel, IStartupLogConsumer
{
	[ExportGroup("Dependencies")]
	[Export] LogDispatcher _dispatcher;
	[Export] LogStyler _styler;

	int _currentLines = 0;

	public override void _EnterTree()
	{
		_dispatcher.DispatchLogBatch += OnLogBatchDispatched;
		PikeConsoleStates.ConsoleMaxLines.ValueInvalidated += ValidateLines;
	}
	public override void _ExitTree()
	{
		PikeConsoleStates.ConsoleMaxLines.ValueInvalidated -= ValidateLines;
		_dispatcher.DispatchLogBatch -= OnLogBatchDispatched;
	}

	// ----- ----- ----- -----
	//	   INTERNAL API
	// ----- ----- ----- -----

	void OnLogBatchDispatched(LogEvent[] logEvents)
	{
		if (logEvents.Length == 0)
			return;

		int maxLines = PikeConsoleStates.ConsoleMaxLines.Value;


		StringBuilder sb = new();
		foreach (LogEvent logEvent in logEvents)
			sb.Append(NormalizeLog(logEvent));

		string finalText = sb.ToString();

		finalText = CutLinesFromEnd(finalText, maxLines, out int lineCount);

		if (lineCount >= maxLines)
		{
			Clear();
			AppendText($"----- Response exceeded {maxLines} lines! -----\n");
			_currentLines++;

			int firstNewline = finalText.IndexOf('\n');
			if (firstNewline != -1 && firstNewline < finalText.Length - 1)
				finalText = finalText[(firstNewline + 1)..];
		}

		AppendText(finalText);
		_currentLines += CountLines(finalText);

		ValidateLines();
	}

	// Helper method that counts the amount of newlines in some text and just trims it from the back
	// So if we have more lines than allowed, we just return the max amount of lines from the end.
	static string CutLinesFromEnd(string text, int maxLines, out int lineCount)
	{
		lineCount = 0;
		if (string.IsNullOrEmpty(text) || maxLines <= 0)
			return string.Empty;

		int startIndex = text.Length - 1;
		if (text[startIndex] == '\n')
			startIndex--;

		for (int i = startIndex; i >= 0; i--)
		{
			if (text[i] == '\n')
			{
				lineCount++;

				// This is, in effet something like: text[^maxlines..]
				if (lineCount >= maxLines)
					return text[(i + 1)..];
			}
		}

		lineCount++;
		return text;
	}

	int CountLines(string s)
	{
		int count = 0;
		for (int i = 0; i < s.Length; i++)
			if (s[i] == '\n') count++;

		return count;
	}

	void ValidateLines()
	{
		int maxLines = PikeConsoleStates.ConsoleMaxLines.Value;

		if (_currentLines <= maxLines)
			return;

		int linesToRemove = _currentLines - maxLines;

		// NOTE!!!
		// We had a while loop here before that used GetParagraphCount.
		// It choked hard on spam tests. Probably because this method can be called many times in one frame.
		// That's why we use this manual cache of current lines.
		for (int i = 0; i < linesToRemove; i++)
			RemoveParagraph(0);

		_currentLines -= linesToRemove;
	}

	string NormalizeLog(in LogEvent logEvent)
	{
		string header = _styler.GetLogHeader(logEvent);
		string footer = _styler.GetLogFooter(logEvent);
		return $"{header}{logEvent.Message}{footer}\n";
	}

	// ----- ----- ----- -----
	//		 PUBLIC API
	// ----- ----- ----- -----
	public void PushText(string text)
	{
		AppendText(text);
		_currentLines += CountLines(text);

		ValidateLines();
	}

	public void ConsumeStartupLogs(LogEvent[] logEvents) =>
		OnLogBatchDispatched(logEvents);

	public new void Clear()
	{
		base.Clear();
		_currentLines = 0;
	}
}