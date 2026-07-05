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
		string prefix = string.Empty;

		// Lol, this is not how we should do it...
		// Though, it's a nice proof of concept!!
		if (logEvent.LogLevel is LogLevel.Engine_Warning or LogLevel.Engine_Error)
			prefix += "[Engine] ";
		else if (!logEvent.HasTag(RuntimeExecutionLogTags.NoHeader) && logEvent.TryGetAnyTag([
			RuntimeExecutionLogTags.Success,
			RuntimeExecutionLogTags.InvalidArgs,
			RuntimeExecutionLogTags.DeniedCheat,
			RuntimeExecutionLogTags.Failed,
			RuntimeExecutionLogTags.Error],
			out string tag))
		{
			prefix += tag switch
			{
				RuntimeExecutionLogTags.Success => "[[color=green]Success[/color]] ",
				RuntimeExecutionLogTags.InvalidArgs => "[[color=yellow]Invalid Args[/color]] ",
				RuntimeExecutionLogTags.DeniedCheat => "[[color=yellow]Cheatmode[/color]] ",
				RuntimeExecutionLogTags.Failed => "[[color=orange]Failed[/color]] ",
				RuntimeExecutionLogTags.Error => "[[color=red]Error[/color]] ",
				_ => string.Empty
			};
		}

		string sp = string.IsNullOrWhiteSpace(logEvent.SourcePath) ? string.Empty : $"{logEvent.SourcePath}: ";


		_richText.AppendText($"{prefix}{sp}{logEvent.Message}\n");
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
