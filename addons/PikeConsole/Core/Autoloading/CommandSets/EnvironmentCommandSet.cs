using System;
using System.Text;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution;
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
using Godot;

namespace FractalPike.PikeConsole.Core.Autoloading;

/*
 * Post optimization clarity (NOTE TO SELF):
 * There is no need, in this cold path, to cache all interop values just to save a few nanoseconds.
 * It just made the code unreadable. Do not reintroduce.
*/

public partial class EnvironmentCommandSet : CommandSet
{
	static string DisplayBytes(double bytes, bool asMB = true) => asMB ?
		$"{bytes / 1_048_576f:F2} MB ({bytes / 1_073_741_824f:F2} GB)"
		: $"{bytes / 1_073_741_824f:F2} GB";

	static string GetProjectAndVersion()
	{
		string n = ProjectSettings.GetSetting("application/config/name").AsString();
		string v = ProjectSettings.GetSetting("application/config/version").AsString();
		v = string.IsNullOrWhiteSpace(v) ? "" : $"({v})";
		return $"{n} {v}";
	}

	const string PREFIX = "env";
	protected override Command[] InstantiateCommands() => [
		Command(
			$"{PREFIX}_info",
			"Shows detailed information about the current environment.",
			"Shows OS, Engine version, GPU, GPU-API and RAM.",
			$"{PREFIX}_info [no args]",
			false,
			(_) => {
				StringBuilder sb = new($"-- {GetProjectAndVersion()}\n");
				sb.AppendLine($"Godot version: {Engine.GetVersionInfo()["string"].AsString()}");
				sb.AppendLine($"OS: {OS.GetName()} | RAM: {DisplayBytes((long)OS.GetMemoryInfo()["physical"], false)}");
				sb.AppendLine($"CPU: {OS.GetProcessorName()}");
				sb.Append($"GPU: {RenderingServer.GetVideoAdapterName()} [API: {RenderingServer.GetVideoAdapterApiVersion()}]");

				PikeLogger.Log(LogTarget.Runtime, $"{sb.ToString()}", forceLog: true);
				return new(ExecutionResponseStatus.Success, null);
			}
		),
		Command(
			$"{PREFIX}_mem",
			"Log a snapshot of the environments memory usage at this time.",
			"Shows all relevant memory information as is. Provides live-diagnostic data for the snapshot.",
			$"{PREFIX}_mem [no args]",
			false,
			(_) => {
				StringBuilder sb = new($"-- Memory snapshot\n");
				sb.AppendLine($"System RAM: {DisplayBytes((long)OS.GetMemoryInfo()["physical"], false)}");

				double usedVramBytes = Performance.GetMonitor(Performance.Monitor.RenderVideoMemUsed);
				sb.AppendLine($"VRAM Used: {DisplayBytes(usedVramBytes)}.");

				long systemAvailableBytes = (long)OS.GetMemoryInfo()["free"];
				sb.AppendLine($"Free: {DisplayBytes(systemAvailableBytes)}");

				long csharpRamBytes = GC.GetTotalMemory(false);
				sb.Append($"Used (.NET): {DisplayBytes(csharpRamBytes)}");

				PikeLogger.Log(LogTarget.Runtime, $"{sb.ToString()}", forceLog: true);
				return new(ExecutionResponseStatus.Success, null);
			}
		),
		Command(
			$"{PREFIX}_gc",
			"Performs a manual garbage collection.",
			"Forces the garbage collector to run. Used for debugging and testing memory allocation.",
			$"{PREFIX}_gc [no args]",
			false,
			(_) => {
				try{
					GC.Collect();
				}
				catch(Exception err)
				{
					return new(ExecutionResponseStatus.Error, $"Failed to collect garbage: {err.Message}");
				}
				return new(ExecutionResponseStatus.Success, "C# Garbage collected.");
			}
		),
		Command(
			$"{PREFIX}_time",
			"Log a snapshot of the environments time context at this time.",
			"Shows all relevant time information as is. Provides live-diagnostic data for the snapshot.",
			$"{PREFIX}_time [no args]",
			false,
			(_) => {
				StringBuilder sb = new("-- Time snapshot\n");
				sb.AppendLine($"{DateTime.Now}");

				// Uptime for the actual system running the game.
				TimeSpan uptime = TimeSpan.FromMilliseconds(System.Environment.TickCount64);
				sb.AppendLine($"System Uptime: {uptime.Hours:D2}h {uptime.Minutes:D2}m {uptime.Seconds:D2}s");

				// Uptime for the actual C++ engine.
				double timeInSeconds = Time.GetTicksMsec() / 1000.0;
				int frames = Engine.GetFramesDrawn();
				sb.AppendLine($"Godot Uptime: {timeInSeconds:F2}");
				sb.AppendLine($"In Frames: {frames}");
				sb.AppendLine($"Avg FPS: {Math.Floor(frames / timeInSeconds)}");

				sb.Append($"Scale: {Engine.TimeScale}");

				PikeLogger.Log(LogTarget.Runtime, $"{sb.ToString()}", forceLog: true);
				return new(ExecutionResponseStatus.Success, null);
			}
		)
	];
}
