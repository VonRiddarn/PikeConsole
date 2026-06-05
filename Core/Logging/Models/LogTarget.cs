using System;

namespace FractalPike.PikeConsole.Core.Logging;

[Flags]
public enum LogTarget
{
	Debug = 1,
	Runtime = 2,
	All = Debug | Runtime
};