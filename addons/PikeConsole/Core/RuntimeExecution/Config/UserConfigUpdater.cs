using System.Threading;
using System.Threading.Tasks;
using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution.Cvars;
using Godot;

namespace FractalPike.PikeConsole.Core.RuntimeExecution.Config;

public partial class UserConfigUpdater : Node
{
	[Export] public CVarBool LogOnSave { get; private set; }

	CancellationTokenSource _debounceCts;
	const int DEBOUNCE_MS = 1200;

	public override void _EnterTree()
	{

		if (LogOnSave == null)
			PikeLogger.LogError(LogTarget.All, $"(NODE: {Name} | FractalPike.PikeConsole.Core.RuntimeExecution.Config) Missing CVar for \"LogOnSave\".", forceLog: true);
		else
			LogOnSave.Initialize();

		if (!PikeConsoleConfig.UserConfigsEnabled)
			return;

		UserConfigManager.SelectConfig(UserConfigManager.ActiveConfig.FileName);

		PersistentCVarRegistry.ValueUpdated += OnCVarChanged;
	}

	public override void _ExitTree()
	{
		if (PikeConsoleConfig.UserConfigsEnabled)
			PersistentCVarRegistry.ValueUpdated -= OnCVarChanged;

		_debounceCts?.Cancel();
		_debounceCts?.Dispose();
	}

	async void OnCVarChanged(ICVar _)
	{
		_debounceCts?.Cancel();

		_debounceCts = new();
		var tempToken = _debounceCts.Token;

		try
		{
			await Task.Delay(DEBOUNCE_MS, tempToken);

			var active = UserConfigManager.ActiveConfig;
			var response = UserConfigManager.SaveConfig(active.FileName);

			if (response.Status == ConfigResponseStatus.Success && LogOnSave != null && LogOnSave.Value)
				PikeLogger.LogSuccess(LogTarget.Runtime, $"Profile \"{active.DisplayName}\" has been saved.", forceLog: true);
			else if (response.Status != ConfigResponseStatus.Error)
				PikeLogger.LogWarning(LogTarget.Runtime, $"{response.Message}", forceLog: true, tags: response.Tags);
			else
				PikeLogger.LogError(LogTarget.All, $"{response.Message}", forceLog: true, tags: response.Tags);
		}
		catch (TaskCanceledException)
		{
			// Temptoken is dead due to debounce. (A new save was triggered)
			// Just ignore and no op.
		}
	}
}
