using System;

namespace FractalPike.PikeConsole.Core.RuntimeExecution;
public static class ArgumentParser
{
	public static bool ValidateCount(ReadOnlySpan<string> args, int count, out string error)
	=> ValidateCount(args, count, count, out error);

	public static bool ValidateCount(ReadOnlySpan<string> args, int min, int max, out string error)
	{
		int n = args.Length;
		error = null;

		if (n > max)
		{
			error = "Too many arguments.";
			return false;
		}

		if (n < min)
		{
			error = "Not enough arguments.";
			return false;
		}

		return true;
	}

	// ----- ----- CUSTOM PARSING ----- -----
	// CVar 
	public static bool TryParseBool(ReadOnlySpan<char> input, out bool value)
	{
		// Note: In the Unity version we tried parsing to int. That's a complete waste.
		// There's no need to "check if the number is a number" as we don't even use it for math.
		if (input is "1" || input.Equals("true", StringComparison.OrdinalIgnoreCase))
		{
			value = true;
			return true;
		}
		if (input is "0" || input.Equals("false", StringComparison.OrdinalIgnoreCase))
		{
			value = false;
			return true;
		}

		value = false;
		return false;
	}

	public static bool TryParseEnum(ReadOnlySpan<char> input, ReadOnlySpan<string> options, out int index, out string error)
	{
		error = null;
		index = -1;

		// Try parsing int first as that is the fastest path. Hopecore.
		if (int.TryParse(input, out int idx))
		{
			if (idx >= 0 && idx < options.Length)
			{
				index = idx;
				return true;
			}

			error = "Index out of range.";
			return false;
		}

		// Fallback on matching the strings.
		for (int i = 0; i < options.Length; i++)
		{
			if (input.Equals(options[i], StringComparison.OrdinalIgnoreCase))
			{
				index = i;
				return true;
			}
		}

		error = $"No match for {input}.";
		return false;
	}

	// 
	// Godot
	// TODO: Add Vector2 [x, y] ["x y"]
	// TODO: Add Vector3
	// TODO: Add Color [r,g,b,a] [r,g,b] [c] (NOTE TO SELF: "c": Color.FromString() allows hex, plaintext etc)

	// ----- ----- NATIVE SHORTHANDS ----- -----

	// Ngl, quite proud of this little router / wrapper!
	// It just allows us to bulk parse any type that has a standard parse fucntion (or we can pass our own).
	// This can get usefull when parsing custom object types, like Vectors: ParseMany...["12", "0", "14"]

	public delegate bool TryParseDelegate<T>(string input, out T result);

	// Delegate passthroughs for floats and doubles so that localization doesn't break anything, EG: 10.5 vs 10,5
	static readonly TryParseDelegate<float> invariantFloatParser = (string s, out float f) =>
		float.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, out f);

	static readonly TryParseDelegate<double> invariantDoubleParser = (string s, out double d) =>
		double.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, out d);


	/// <summary>
	/// Use this instead of "TryParseMany" for floats. It automatically applies culture invarience to floating point numbers. <br />
	/// Use <c>args.AsSpan(start, end)</c> if the parameters are continuous. <br />
	/// Use shorthand: <c>[args[1], args[3], args[7]]</c> if the parameters are non-continuous.
	/// </summary>
	public static bool TryParseManyFloat(ReadOnlySpan<string> args, out float[] values, out string error) =>
	TryParseMany(invariantFloatParser, args, out values, out error);
	/// <summary>
	/// Use this instead of "TryParseMany" for doubles. It automatically applies culture invarience to floating point numbers. <br />
	/// Use <c>args.AsSpan(start, end)</c> if the parameters are continuous. <br />
	/// Use shorthand: <c>[args[1], args[3], args[7]]</c> if the parameters are non-continuous.
	/// </summary>
	public static bool TryParseManyDouble(ReadOnlySpan<string> args, out double[] values, out string error) =>
		TryParseMany(invariantDoubleParser, args, out values, out error);

	/// <summary>
	/// Takes a parser method, such as "int.TryParse" and automatically maps it to parse all arguments in an array. <br />
	/// Use <c>args.AsSpan(start, end)</c> if the parameters are continuous. <br />
	/// Use shorthand: <c>[args[1], args[3], args[7]]</c> if the parameters are non-continuous.
	/// </summary>
	/// <remarks>
	/// Floats and doubles use their own TryParseMany methods due to decimal invariants!
	/// Using this method for numbers containing decimals may lead to weird bugs.
	/// </remarks>
	/// <param name="parser">Parser method to use, EG: int.TryParse</param>
	/// <param name="args"></param>
	/// <param name="values"></param>
	/// <param name="error"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns>Success: true and fills "values" || Fail: false and fills "error"</returns>
	public static bool TryParseMany<T>(TryParseDelegate<T> parser, ReadOnlySpan<string> args, out T[] values, out string error)
	{
		if (args.Length <= 0)
		{
			values = [];
			error = "Argument list is empty!";
			return false;
		}

		values = new T[args.Length];

		for (int i = 0; i < args.Length; i++)
		{
			// If any parse fails, we abort and return false.
			if (!parser(args[i], out values[i]))
			{
				values = [];
				error = $"Failed to parse {args[i]} at index ({i}).";
				return false;
			}
		}

		error = null;
		return true;
	}
}
