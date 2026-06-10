namespace FractalPike.PikeConsole.Core.RuntimeExecution;
public interface ICVar : IRuntimeExecutable
{
	public bool Persist { get; }
	public void ResetValue();
}
