using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution;
using FractalPike.PikeConsole.Core.Utilities;
using Godot;

/* 
 * Note: I'm not sure of each objects specific task yet.
 * Thus I am naming variables based on what node they are to not lock in certain thoughts.
 * 
*/

namespace FractalPike.PikeConsole.Frontend;

public partial class PikeConsoleUI : Node
{
	[Export] LineEdit _inputField;
	[Export] RichTextLabel _richText;

	public sealed override void _EnterTree()
	{
		_inputField.TextSubmitted += OnInputSubmitted;
		PikeLogger.LogEmitted += OnLogEmitted;
	}

	public sealed override void _ExitTree()
	{
		_inputField.TextSubmitted -= OnInputSubmitted;
		PikeLogger.LogEmitted -= OnLogEmitted;
	}

	void OnLogEmitted(in LogEvent logEvent)
	{
		string header = string.Empty;

		// Lol, this is not how we should do it...
		// Though, it's a nice proof of concept!!

		header += logEvent.LogLevel switch
		{
			LogLevel.Engine_Warning => "[[color=#ffae63]Engine Warning[/color]] ",
			LogLevel.Engine_Error => "[[color=#ff5151]Engine Error[/color]] ",
			_ => string.Empty
		};

		if (!logEvent.HasTag(LogTags.NoHeader))
		{
			// If we aren't refusing a header, begin by going through header override tags.
			// If no header override tags are present, attach a header based on the loglevel.
			if (logEvent.TryGetAnyTag([
			LogTags.InvalidArgs,
			LogTags.DeniedCheat,
			LogTags.Failed,
			LogTags.ValueLimited,
			LogTags.ValueClamped],
			out string tag))
			{
				header += tag switch
				{
					LogTags.InvalidArgs => "[[color=#ffef63]Invalid Args[/color]] ",
					LogTags.DeniedCheat => "[[color=#ffef63]Cheatmode[/color]] ",
					LogTags.Failed => "[[color=#ffae63]Failed[/color]] ",
					LogTags.ValueLimited => "[[color=#ffef63]Limited[/color]] ",
					LogTags.ValueClamped => "[[color=#ffef63]Clamped[/color]] ",
					_ => string.Empty
				};
			}
			else
			{
				header += logEvent.LogLevel switch
				{
					LogLevel.Success => "[[color=#B2FF73]Success[/color]] ",
					LogLevel.Warning => "[[color=#ffae63]Warning[/color]] ",
					LogLevel.Error => "[[color=#ff5151]Error[/color]] ",
					_ => string.Empty
				};
			}
		}

		string sp = string.IsNullOrWhiteSpace(logEvent.SourcePath) ? string.Empty : $"{logEvent.SourcePath}: ";


		_richText.AppendText($"{header}{sp}{logEvent.Message}\n");
	}

	void OnInputSubmitted(string inputStatement)
	{
		_richText.AppendText($"> {_inputField.Text}\n");
		StatementExecutor.Execute(ExecutionSource.Standard, inputStatement);
		_inputField.Clear();
	}

	public void Clear()
	{
		_richText.Text = string.Empty;
	}

}
