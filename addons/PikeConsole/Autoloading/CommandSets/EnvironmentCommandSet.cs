using System;
using System.Diagnostics;
using System.Text;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution;
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
using Godot;

namespace FractalPike.PikeConsole.Autoloading;

/*
 * Post optimization clarity (NOTE TO SELF):
 * There is no need, in this cold path, to cache all interop values just to save a few nanoseconds.
 * It just made the code unreadable. Do not reintroduce.
*/

public partial class EnvironmentCommandSet : CommandSet
{
	protected override string Prefix => "env";

	static string DisplayBytes(double bytes, bool asMB = true) => asMB ?
		$"{bytes / 1_048_576f:F2} MB ({bytes / 1_073_741_824f:F2} GB)"
		: $"{bytes / 1_073_741_824f:F2} GB";

	static string GetProjectAndVersion()
	{
		string name = string.Empty;
		string version = string.Empty;

		// Anything that is not an editor is obv a release.
		if (!OS.HasFeature("editor"))
		{
			try
			{
				var fileInfo = FileVersionInfo.GetVersionInfo(OS.GetExecutablePath());
				name = fileInfo.ProductName;
				version = fileInfo.ProductVersion;
			}
			catch { }
		}

		// Fallback to project settings
		if (string.IsNullOrWhiteSpace(name))
			name = ProjectSettings.GetSetting("application/config/name").AsString();

		if (string.IsNullOrWhiteSpace(version))
			version = ProjectSettings.GetSetting("application/config/version").AsString();

		version = string.IsNullOrWhiteSpace(version) ? string.Empty : $"({version})";

		return $"{(OS.IsDebugBuild() ? "[DEBUG] " : string.Empty)}{name} {version}".Trim();
	}

	protected override Command[] InstantiateCommands() => [
		Command(
			Signature("info"),
			"Shows detailed information about the current environment.",
			"Shows OS, Engine version, GPU, GPU-API and RAM.",
			$"{Signature("info")} [no args]",
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
			Signature("mem"),
			"Log a snapshot of the environments memory usage at this time.",
			"Shows all relevant memory information as is. Provides live-diagnostic data for the snapshot.",
			$"{Signature("mem")} [no args]",
			false,
			static (_) => {
				StringBuilder sb = new($"MEMORY SNAPSHOT\n");
				sb.AppendLine($"\tSystem RAM: {DisplayBytes((long)OS.GetMemoryInfo()["physical"], false)}");

				double usedVramBytes = Performance.GetMonitor(Performance.Monitor.RenderVideoMemUsed);
				sb.AppendLine($"\tVRAM Used: {DisplayBytes(usedVramBytes)}.");

				long systemAvailableBytes = (long)OS.GetMemoryInfo()["free"];
				sb.AppendLine($"\tFree: {DisplayBytes(systemAvailableBytes)}");

				long csharpRamBytes = GC.GetTotalMemory(false);
				sb.AppendLine($"\tUsed (.NET): {DisplayBytes(csharpRamBytes)}");

				// NOTE:
				// This is very, very estimated and a little out of my league.
				// The magic numbers are a high estimate of byte allocations. 
				// This is NOT my domain at all, but I've counted:
				// 		Dictionary: ~35 bytes per entry
				// 		Commands and Cvars: ~900 bytes per entry
				// If these values are whack a PR and explanation would be appreciated!
				// 
				// When testing in an empty project with 10 000 entries (5 000 Cvar, 5 000 Commands),
				// we are counting about 2MB higher than the actual .NET allocated memory, which is prefered over counting low.
				int cvars = RuntimeExecutableRegistry.CvarCount;
				int cmds = RuntimeExecutableRegistry.CommandCount;
				long dictOverhead = (cvars + cmds) * 35;
				long estimatedBytes = dictOverhead + (cvars + cmds) * 900;

				sb.AppendLine($"\tRuntime executables ({cvars + cmds})");
				sb.AppendLine($"\t\tCvars: {cvars}");
				sb.AppendLine($"\t\tCommands: {cmds}");
				sb.Append($"\t\tEST: {DisplayBytes(estimatedBytes)}");

				PikeLogger.Log(LogTarget.Runtime, $"{sb.ToString()}", forceLog: true);
				return new(ExecutionResponseStatus.Success, null);
			}
		),
		Command(
			Signature("gc"),
			"Performs a manual garbage collection.",
			"Forces the garbage collector to run. Used for debugging and testing memory allocation.",
			$"{Signature("gc")} [no args]",
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
			Signature("time"),
			"Log a snapshot of the environments time context at this time.",
			"Shows all relevant time information as is. Provides live-diagnostic data for the snapshot.",
			$"{Signature("time")} [no args]",
			false,
			static (_) =>
			{
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
