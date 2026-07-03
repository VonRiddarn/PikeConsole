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
		// Lol, this is not how we should dod it...
		_richText.Text += $"{logEvent.Message}\n";
	}

	void OnInputSubmitted(string inputStatement)
	{
		_richText.Text += $"> {_inputField.Text}\n";
		StatementExecutor.Execute(ExecutionSource.Player, inputStatement);
		_inputField.Clear();
	}

	public void Clear()
	{
		_richText.Text = string.Empty;
	}

}
