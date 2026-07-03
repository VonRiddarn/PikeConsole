using System;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution;
using Godot;

/* 
 * Note: I'm not sure of each objects specific task yet.
 * Thus I am naming variables based on what node they are to not lock in certain thoughts.
 * 
*/

public partial class PikeConsoleUI : Node
{
	[Export] LineEdit _lineEdit;
	[Export] RichTextLabel _richText;


	public override void _EnterTree()
	{
		_lineEdit.TextSubmitted += OnInputSubmitted;
		PikeLogger.LogEmitted += OnLogEmitted;
	}

	public override void _ExitTree()
	{
		_lineEdit.TextSubmitted -= OnInputSubmitted;
		PikeLogger.LogEmitted -= OnLogEmitted;
	}

	private void OnLogEmitted(in LogEvent logEvent)
	{
		// Lol, this is not how we should dod it...
		_richText.Text += $"{logEvent.Message}\n";
	}

	private void OnInputSubmitted(string text)
	{
		StatementExecutor.Execute(ExecutionSource.Player, text);
		_lineEdit.Clear();
	}
}
