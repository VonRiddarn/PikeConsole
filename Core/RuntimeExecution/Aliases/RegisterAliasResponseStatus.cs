namespace FractalPike.PikeConsole.Core.RuntimeExecution;

public enum RegisterAliasResponseStatus
{
	/// <summary>Default fallback.</summary>
	None = 0,
	/// <summary>Alias registered successfully.</summary>
	Success = 1,
	/// <summary>Alias registered and replaced old alias.</summary>
	Replaced = 2,
	/// <summary>Alias denied because a Command or CVar already occupies that signature.</summary>
	Occupied = 3,
	/// <summary>Alias denied because another alias exists with that signature.</summary>
	AlreadyExists = 4
}
