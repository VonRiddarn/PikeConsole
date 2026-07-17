using System.Collections.Generic;
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

	[ExportGroup("Colors")]
	[Export] Color _noEffect = new Color("#afafaf");
	[Export] Color _success = new Color("#b2ff73");
	[Export] Color _warningSoft = new Color("#ffef63");
	[Export] Color _warningHard = new Color("#ffae63");
	[Export] Color _error = new Color("#ff5151");
	[Export] Color _path = new Color("#afafaf");

	[ExportGroup("Header overrides")]
	[Export] bool _useDefaultHeaders = true;
	[Export] bool _accumulativeHeaders = false;
	[Export] HeaderOverride[] _headerOverrides = [];

	// Default header texts come from LogTags.
	// PikeConsole/Utilities/LogTags.cs
	HeaderOverride[] _defaultHeaderOverrides = [];

	Dictionary<string, HeaderOverride> _headerRegistry = new();

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

		if (_useDefaultHeaders)
		{
			_defaultHeaderOverrides = [
				new HeaderOverride(LogTags.InvalidArgs, "Invalid Args", _warningSoft),
				new HeaderOverride(LogTags.Failed, "Failed", _warningHard),
				new HeaderOverride(LogTags.DeniedCheat, "Cheats Denied", _warningSoft),
				new HeaderOverride(LogTags.ValueLimited, "Limited", _warningSoft),
				new HeaderOverride(LogTags.ValueClamped, "Clamped", _warningHard),
				new HeaderOverride(LogTags.ValueNoChange, "No Change", _noEffect),
				new HeaderOverride(LogTags.NotFound, "Not Found", _warningSoft),
				new HeaderOverride(LogTags.Conflict, "Conflict", _warningHard),
			];
		}

		foreach (var ho in _defaultHeaderOverrides)
			_headerRegistry[ho.LogTag] = ho;

		foreach (var ho in _headerOverrides)
			_headerRegistry[ho.LogTag] = ho;
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

		while (pgf > PikeConsoleCVars.ConsoleMaxLines.Value)
		{
			RemoveParagraph(0, true);
			pgf--;
		}
	}

	string NormalizeLog(in LogEvent logEvent)
	{
		string header = GetLogHeader(logEvent);
		string footer = GetLogFooter(logEvent);
		return $"{header}{logEvent.Message}{footer}\n";
	}

	string GetLogHeader(in LogEvent logEvent)
	{
		// Fast return if no tags.
		if (logEvent.HasTag(LogTags.NoHeader))
			return string.Empty;

		StringBuilder sb = logEvent.LogLevel switch
		{
			LogLevel.Engine_Warning => new($"[[color=#{_warningHard.ToHtml()}]Engine Warning[/color]]"),
			LogLevel.Engine_Error => new($"[[color=#{_error.ToHtml()}]Engine Error[/color]]"),
			_ => new()
		};

		foreach (string tag in logEvent.Tags)
		{
			if (_headerRegistry.TryGetValue(tag, out HeaderOverride ho))
			{
				sb.Append($"[[color=#{ho.Color.ToHtml()}]{ho.Label}[/color]]");

				// If we do not want accumulative headers, we just break after finding at least one.
				// The exceptrion is engine logs, which will always accumulate before the "real" header.
				if (!_accumulativeHeaders)
					break;
			}
		}

		if (sb.Length < 1)
		{
			string coreHeader = logEvent.LogLevel switch
			{
				LogLevel.Success => "[[color=#B2FF73]Success[/color]]",
				LogLevel.Warning => "[[color=#ffae63]Warning[/color]] ",
				LogLevel.Error => "[[color=#ff5151]Error[/color]] ",
				_ => string.Empty
			};

			if (!string.IsNullOrWhiteSpace(coreHeader))
				sb.Append($"{coreHeader}");
		}

		if (sb.Length > 0)
			sb.Append('\n');

		return sb.ToString();
	}

	string GetLogFooter(in LogEvent logEvent)
	{
		// Fast return if no tags.
		if (string.IsNullOrEmpty(logEvent.SourcePath))
			return string.Empty;
		return $"\n[color=#{_path.ToHtml()}]in \"{logEvent.SourcePath}\".[/color]";
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
