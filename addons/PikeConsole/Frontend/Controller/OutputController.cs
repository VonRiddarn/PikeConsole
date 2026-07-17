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
			PikeConsoleAPI.RuntimeConsole.SetEnabled(false);
			return;
		}

		PikeConsoleCVars.ConsoleMaxLines.ValueInvalidated += ValidateLines;
	}
	public override void _ExitTree()
	{
		_dispatcher.DispatchLogBatch -= OnLogBatchDispatched;
		PikeConsoleCVars.ConsoleMaxLines.ValueInvalidated -= ValidateLines;
	}

	void OnLogBatchDispatched(LogEvent[] logEvents)
	{
		if (logEvents.Length == 0)
			return;

		StringBuilder sb = new();
		foreach (LogEvent logEvent in logEvents)
			sb.Append(NormalizeLog(logEvent));

		AppendText(sb.ToString());

		ValidateLines();
	}

	// ----- ----- ----- -----
	//		  HELPERS
	// ----- ----- ----- -----

	void ValidateLines()
	{
		int maxLines = PikeConsoleCVars.ConsoleMaxLines.Value;
		int pgf = GetParagraphCount();

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
