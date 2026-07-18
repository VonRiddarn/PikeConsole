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

	public override void _EnterTree()
	{
		if (_dispatcher != null)
			_dispatcher.DispatchLogBatch += OnLogBatchDispatched;
		else
		{
			PikeLogger.LogError(LogTarget.Editor, $"Dispoatcher has not been through the editor in \"{Name}\".");
			PikeConsoleStates.RuntimeConsoleEnabled.Value = false;
			return;
		}

		PikeConsoleStates.ConsoleMaxLines.ValueInvalidated += ValidateLines;
	}
	public override void _ExitTree()
	{
		_dispatcher.DispatchLogBatch -= OnLogBatchDispatched;
		PikeConsoleStates.ConsoleMaxLines.ValueInvalidated -= ValidateLines;
	}

	void OnLogBatchDispatched(LogEvent[] logEvents)
	{
		if (logEvents.Length == 0)
			return;

		int maxLines = PikeConsoleStates.ConsoleMaxLines.Value;


		StringBuilder sb = new();
		foreach (LogEvent logEvent in logEvents)
			sb.Append(NormalizeLog(logEvent));

		string incomingText = sb.ToString();

		incomingText = CutLinesFromEnd(incomingText, maxLines, out int lineCount);

		if (lineCount >= maxLines)
		{
			Clear();
			AppendText($"----- Response exceeded {PikeConsoleStates.ConsoleMaxLines.Value} lines! -----\n");

			int firstNewline = incomingText.IndexOf('\n');
			if (firstNewline != -1 && firstNewline < incomingText.Length - 1)
				incomingText = incomingText[(firstNewline + 1)..];
		}

		AppendText(incomingText);
		ValidateLines();
	}

	// ----- ----- ----- -----
	//		  HELPERS
	// ----- ----- ----- -----

	static string CutLinesFromEnd(string text, int maxLines, out int lineCount)
	{
		lineCount = 0;
		if (string.IsNullOrEmpty(text) || maxLines <= 0)
			return string.Empty;

		// This is just to skip the last newline,
		int startIndex = text.Length - 1;
		if (text[startIndex] == '\n')
			startIndex--;

		// Basically, go through the text backwards and count the lines.
		// If we exceed the maxlines, we can cut that part entirely so it isn't handle by the RichTextLabel (which was super slow)
		for (int i = startIndex; i >= 0; i--)
		{
			if (text[i] == '\n')
			{
				lineCount++;
				if (lineCount >= maxLines)
					return text[(i + 1)..];
			}
		}

		lineCount++;
		return text;
	}

	void ValidateLines()
	{
		int maxLines = PikeConsoleStates.ConsoleMaxLines.Value;
		int pgf = GetParagraphCount();
		int allowedLines = maxLines + 1;

		if (pgf <= allowedLines)
			return;

		if (pgf > allowedLines * 2)
		{
			Clear();
			AppendText($"\n----- Trimmed {pgf - maxLines} lines to save memory -----\n");
			return;
		}

		while (pgf > maxLines)
		{
			RemoveParagraph(0, true);
			pgf--;
		}
	}

	string NormalizeLog(in LogEvent logEvent)
	{
		string header = _styler.GetLogHeader(logEvent);
		string footer = _styler.GetLogFooter(logEvent);
		return $"{header}{logEvent.Message}{footer}\n";
	}

	// ----- ----- ----- -----
	//		     API
	// ----- ----- ----- -----
	public void PushText(string text)
	{
		AppendText(text);
		ValidateLines();
	}

	public void ConsumeStartupLogs(LogEvent[] logEvents) =>
		OnLogBatchDispatched(logEvents);
}
