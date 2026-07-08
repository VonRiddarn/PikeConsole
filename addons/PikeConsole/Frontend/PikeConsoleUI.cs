using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution;
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
			LogLevel.Engine_Warning => "[[color=#ff9c4b]Engine Warning[/color]] ",
			LogLevel.Engine_Error => "[[color=#FF7373]Engine Error[/color]] ",
			_ => string.Empty
		};

		if (!logEvent.HasTag(RuntimeExecutionLogTags.NoHeader))
		{
			// If we aren't refusing a header, begin by going through header override tags.
			// If no header override tags are present, attach a header based on the loglevel.
			if (logEvent.TryGetAnyTag([
			RuntimeExecutionLogTags.InvalidArgs,
			RuntimeExecutionLogTags.DeniedCheat,
			RuntimeExecutionLogTags.Failed],
			out string tag))
			{
				header += tag switch
				{
					RuntimeExecutionLogTags.InvalidArgs => "[[color=#FFC973]Invalid Args[/color]] ",
					RuntimeExecutionLogTags.DeniedCheat => "[[color=#FFC973]Cheatmode[/color]] ",
					RuntimeExecutionLogTags.Failed => "[[color=#ff9c4b]Failed[/color]] ",
					_ => string.Empty
				};
			}
			else
			{
				header += logEvent.LogLevel switch
				{
					LogLevel.Success => "[[color=#B2FF73]Success[/color]] ",
					LogLevel.Warning => "[[color=#ff9c4b]Warning[/color]] ",
					LogLevel.Error => "[[color=#FF7373]Error[/color]] ",
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
		StatementExecutor.Execute(ExecutionSource.Player, inputStatement);
		_inputField.Clear();
	}

	public void Clear()
	{
		_richText.Text = string.Empty;
	}

}
