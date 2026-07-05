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
			LogLevel.Engine_Warning => "[[color=orange]Engine Warning[/color]] ",
			LogLevel.Engine_Error => "[[color=red]Engine Error[/color]] ",
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
					RuntimeExecutionLogTags.InvalidArgs => "[[color=yellow]Invalid Args[/color]] ",
					RuntimeExecutionLogTags.DeniedCheat => "[[color=yellow]Cheatmode[/color]] ",
					RuntimeExecutionLogTags.Failed => "[[color=orange]Failed[/color]] ",
					_ => string.Empty
				};
			}
			else
			{
				header += logEvent.LogLevel switch
				{
					LogLevel.Success => "[[color=green]Success[/color]] ",
					LogLevel.Warning => "[[color=orange]Warning[/color]] ",
					LogLevel.Error => "[[color=red]Error[/color]] ",
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
