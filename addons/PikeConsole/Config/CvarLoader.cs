using FractalPike.PikeConsole.Core.Logging;
using FractalPike.PikeConsole.Core.RuntimeExecution;
using Godot;
using System.IO;

namespace FractalPike.PikeConsole.Config;

public static class CvarLoader
{
	/// <summary>
	/// Tries to load an internal CVar and throws an exception if it does not exist.
	/// </summary>
	public static T LoadInternalCVar<T>(string path, string signature) where T : class, ICVar
	{
		string fullPath = $"{path}/{signature}.tres";

		// NOTE: 
		// Godot handles binary conversion etc. We can just check for a .tres file and be happy!
		// Also, we name the variable "signature" since our decided convention is: Filename = Signature
		if (ResourceLoader.Load(fullPath) is T cvar)
			return cvar;

		// NOTE: We cannot use PikeLogger here, as that could cause infinite recursion.
		// Learned that the hard way...
		GD.PrintErr($"CRITICAL: Missing internal CVar: {fullPath}");
		throw new FileNotFoundException($"CRITICAL: Missing internal framework CVar: {fullPath}");
	}
}
