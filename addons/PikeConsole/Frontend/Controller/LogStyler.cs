using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.Utilities;
using Godot;
using System.Collections.Generic;
using System.Text;

namespace FractalPike.PikeConsole.Frontend.Controllers;

public partial class LogStyler : Node
{
	[ExportCategory("Colors")]
	[Export] Color _noEffect = new("#afafaf");
	[Export] Color _success = new("#b2ff73");
	[Export] Color _warningSoft = new("#ffef63");
	[Export] Color _warningHard = new("#ffae63");
	[Export] Color _error = new("#ff5151");
	[Export] Color _path = new("#afafaf");

	[ExportCategory("Header overrides")]
	[Export] bool _useDefaultHeaders = true;
	[Export] bool _accumulativeHeaders = false;
	[Export] HeaderOverride[] _headerOverrides = [];

	// Default header texts come from LogTags.
	// PikeConsole/Utilities/LogTags.cs
	HeaderOverride[] _defaultHeaderOverrides = [];

	readonly Dictionary<string, HeaderOverride> _headerRegistry = [];

	public override void _EnterTree()
	{
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

	public string GetLogHeader(in LogEvent logEvent)
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

	public string GetLogFooter(in LogEvent logEvent)
	{
		// Fast return if no tags.
		if (string.IsNullOrEmpty(logEvent.SourcePath))
			return string.Empty;
		return $"\n[color=#{_path.ToHtml()}]source \"{logEvent.SourcePath}\".[/color]";
	}
}
