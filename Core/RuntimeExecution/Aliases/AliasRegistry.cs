using System;
using System.Collections.Generic;

namespace FractalPike.PikeConsole.Core.RuntimeExecution;
public static class AliasRegistry
{
	static readonly Dictionary<string, string> _aliases = new(StringComparer.OrdinalIgnoreCase);

	// TODO: Optimize, and port the old Unity based system here. 
	// Register(), Unregister()...
}
