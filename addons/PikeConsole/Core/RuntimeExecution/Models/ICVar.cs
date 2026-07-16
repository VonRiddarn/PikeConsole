namespace FractalPike.PikeConsole.Core.RuntimeExecution;
public interface ICVar : IRuntimeExecutable
{
	public bool Persist { get; }

	public bool ResetValue(ExecutionSource executionSource, bool ramOnly = false);

	/// <summary>Used by the individual CVar to register itself to the registry.</summary>
	public void Initialize();

	// These are mostly for routing / saving the data.
	public bool IsModified { get; }

	public string CurrentValueDisplay { get; }
	public string DefaultValueDisplay { get; }

	/// <summary>The string formated value. This value MUST be passable to the SetValue method (note that spaces become separate args).</summary>
	/// <remarks>
	/// By default, this just returns the values ToString method.<br />
	/// NOTE: If you want to display the value in a fancy way, override the DisplayValue() method. 
	/// This property is for data parsing!
	/// </remarks>
	public string FormattedValue { get; }
}
