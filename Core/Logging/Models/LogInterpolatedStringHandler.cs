using System.Runtime.CompilerServices;
using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.Logging.Models;

[InterpolatedStringHandler]
public ref struct LogInterpolatedStringHandler
{
	DefaultInterpolatedStringHandler _handler;
	public bool IsEnabled { get; }

	public LogInterpolatedStringHandler(int literalLength, int formattedCount, LogTarget target, out bool isEnabled)
	{
		isEnabled = PikeLogger.IsTargetEnabled(target);
		IsEnabled = isEnabled;

		if (isEnabled)
		{
			_handler = new DefaultInterpolatedStringHandler(literalLength, formattedCount);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AppendLiteral(string value)
	{
		if (IsEnabled) _handler.AppendLiteral(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AppendFormatted<T>(T value)
	{
		if (IsEnabled) _handler.AppendFormatted(value);
	}

	internal string ToStringAndClear()
	{
		return IsEnabled ? _handler.ToStringAndClear() : string.Empty;
	}
}
