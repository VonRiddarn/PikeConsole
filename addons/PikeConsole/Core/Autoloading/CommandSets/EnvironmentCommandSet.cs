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
		v = string.IsNullOrWhiteSpace(v) ? string.Empty : $"({v})";
		return $"{(OS.IsDebugBuild() ? "[DEBUG]" : string.Empty)}{n} {v}";
	}

	const string PREFIX = "env";
	protected override Command[] InstantiateCommands() => [
		Command(
			$"{PREFIX}_info",
			"Shows detailed information about the current environment.",
			"Shows OS, Engine version, GPU, GPU-API and RAM.",
			$"{PREFIX}_info [no args]",
			false,
			static (_) => {
				StringBuilder sb = new($"{GetProjectAndVersion()}\n");
				sb.AppendLine($"\tGodot version: {Engine.GetVersionInfo()["string"].AsString()}");
				sb.AppendLine($"\tOS: {OS.GetName()} | RAM: {DisplayBytes((long)OS.GetMemoryInfo()["physical"], false)}");
				sb.AppendLine($"\tCPU: {OS.GetProcessorName()}");
				sb.Append($"\tGPU: {RenderingServer.GetVideoAdapterName()} [API: {RenderingServer.GetVideoAdapterApiVersion()}]");

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
			static (_) => {
				StringBuilder sb = new($"MEMORY SNAPSHOT\n");
				sb.AppendLine($"\tSystem RAM: {DisplayBytes((long)OS.GetMemoryInfo()["physical"], false)}");

				double usedVramBytes = Performance.GetMonitor(Performance.Monitor.RenderVideoMemUsed);
				sb.AppendLine($"\tVRAM Used: {DisplayBytes(usedVramBytes)}.");

				long systemAvailableBytes = (long)OS.GetMemoryInfo()["free"];
				sb.AppendLine($"\tFree: {DisplayBytes(systemAvailableBytes)}");

				long csharpRamBytes = GC.GetTotalMemory(false);
				sb.Append($"\tUsed (.NET): {DisplayBytes(csharpRamBytes)}");

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
			static (_) => {
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
			static (_) => {
				StringBuilder sb = new("TIME SNAPSHOT\n");
				sb.AppendLine($"\t{DateTime.Now}");

				// Uptime for the actual system running the game.
				TimeSpan uptime = TimeSpan.FromMilliseconds(System.Environment.TickCount64);
				sb.AppendLine($"\tSystem Uptime: {uptime.Hours:D2}h {uptime.Minutes:D2}m {uptime.Seconds:D2}s");

				// Uptime for the actual C++ engine.
				double timeInSeconds = Time.GetTicksMsec() / 1000.0;
				int frames = Engine.GetFramesDrawn();
				sb.AppendLine($"\tGodot Uptime: {timeInSeconds:F2}");
				sb.AppendLine($"\tIn Frames: {frames}");
				sb.AppendLine($"\tAvg FPS: {Math.Floor(frames / timeInSeconds)}");

				sb.Append($"\tScale: {Engine.TimeScale}");

				PikeLogger.Log(LogTarget.Runtime, $"{sb.ToString()}", forceLog: true);
				return new(ExecutionResponseStatus.Success, null);
			}
		)
	];
}
