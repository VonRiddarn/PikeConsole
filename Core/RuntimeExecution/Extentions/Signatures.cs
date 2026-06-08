namespace FractalPike.PikeConsole.Core.Extensions;

public static class StringExtensions
{
	// Not super strict or anything, but this just trims the signature and turns spaces into underscores.
	// The statement parser separates commands from arguments with spaces, so it's important the signature is valid.
	public static string ToSignature(this string str) => !string.IsNullOrWhiteSpace(str) ? str.Trim().ToLower().Replace(' ', '_') : "";
}