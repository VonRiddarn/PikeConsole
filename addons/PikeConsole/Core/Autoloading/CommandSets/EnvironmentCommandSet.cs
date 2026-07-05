using System;
using System.Text;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution;
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
using Godot;

namespace FractalPike.PikeConsole.Core.Autoloading;

public partial class EnvironmentCommandSet : CommandSet
{
	string _projectName = "GAME NAME NOT INITIALIZED";
	string _cpuName = "CPU INFO NOT INITIALIZED";
	string _osName = "OS INFO NOT INITIALIZED";
	string _systemRam = "SYSTEM RAM INFO NOT INITIALIZED";
	string _godotVersion = "GODOT VERSION INFO NOT INITIALIZED";
	string _videoAdapter = "VIDEO ADAPTER INFO NOT INITIALIZED";
	string _videoAdapterApi = "VIDEO ADAPTER API INFO NOT INITIALIZED";

	const string PREFIX = "env";

	static string DisplayBytes(double bytes) => $"{bytes / 1_048_576f:F2} MB ({bytes / 1_073_741_824f:F2} GB)";

	protected override void OnEnterTree()
	{
		// Go through all the interop stuff and cache everything inside the .NET runtime
		string n = ProjectSettings.GetSetting("application/config/name").AsString();
		string v = ProjectSettings.GetSetting("application/config/version").AsString();
		v = string.IsNullOrWhiteSpace(v) ? "" : $"({v})";

		_projectName = $"{n} {v}";
		_osName = OS.GetName();
		_cpuName = OS.GetProcessorName();
		_godotVersion = Engine.GetVersionInfo()["string"].AsString();
		_videoAdapter = RenderingServer.GetVideoAdapterName();
		_videoAdapterApi = RenderingServer.GetVideoAdapterApiVersion();

		long physicalBytes = (long)OS.GetMemoryInfo()["physical"];
		_systemRam = $"{physicalBytes / 1_073_741_824f:F2} GB";
	}

	protected override Command[] InstantiateCommands() => [
		Command(
			$"{PREFIX}_info",
			"Shows detailed information about the current environment.",
			"Shows OS, Engine version, GPU, GPU-API and RAM.",
			$"{PREFIX}_info [no args]",
			false,
			(_) => {
				StringBuilder sb = new($"{_projectName}");
				sb.AppendLine($"Godot version: {_godotVersion}");
				sb.AppendLine($"OS: {_osName} | RAM: {_systemRam}");
				sb.AppendLine($"CPU: {_cpuName}");
				sb.Append($"GPU: {_videoAdapter} [API: {_videoAdapterApi}]");

				PikeLogger.Log(LogTarget.Runtime, $"{sb.ToString()}");
				return new(ExecutionResponseStatus.Success, null);
			}
		),
		Command(
			$"{PREFIX}_mem",
			"Log a snapshot of the environments memory usage at this time.",
			"Shows all relevant memory information as is. Crosses interop bridge to collect live-diagnostic data for the snapshot.",
			$"{PREFIX}_mem [no args]",
			false,
			(_) => {
				StringBuilder sb = new($"Memory snapshot\n");
				sb.AppendLine($"System RAM: {_systemRam}");

				double usedVramBytes = Performance.GetMonitor(Performance.Monitor.RenderVideoMemUsed);
				sb.AppendLine($"VRAM Used: {DisplayBytes(usedVramBytes)}.");

				long systemAvailableBytes = (long)OS.GetMemoryInfo()["free"];
				sb.AppendLine($"Free: {DisplayBytes(systemAvailableBytes)}");

				long csharpRamBytes = GC.GetTotalMemory(false);
				sb.Append($"Used (.NET): {DisplayBytes(csharpRamBytes)}");

				PikeLogger.Log(LogTarget.Runtime, $"{sb.ToString()}");
				return new(ExecutionResponseStatus.Success, null);
			}
		),
	];
}
