using FractalPike.PikeConsole.Core.RuntimeExecution;
using Godot;

namespace FractalPike.PikeConsole.Frontend.Controllers;

public partial class InputController : LineEdit
{
	[ExportGroup("Dependencies")]
	[Export] OutputController _outputController;

	[ExportGroup("Settings")]
	[Export] string _feedbackPrefix = "] ";

	public override void _EnterTree()
	{
		TextSubmitted += OnInputSubmitted;
	}

	public override void _ExitTree()
	{
		TextSubmitted -= OnInputSubmitted;
	}

	void OnInputSubmitted(string inputStatement)
	{
		_outputController.PushText($"{_feedbackPrefix}{inputStatement}\n");
		StatementExecutor.Execute(ExecutionSource.Standard, inputStatement);
		Clear();
	}
}
