namespace FractalPike.PikeConsole.Core.RuntimeExecution;
public interface ICVar : IRuntimeExecutable
{
	public bool Persist { get; }
	public void ResetValue();

	// These are mostly for routing / saving the data.
	public bool IsModified { get; }
	/// <summary>The string formated value. This value MUST be passable to the SetValue method (note that spaces become separate args).</summary>
	/// <remarks>By default, this just reutns the values ToString method.</remarks>
	public string FormattedValue { get; }
}
