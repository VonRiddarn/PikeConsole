using System;
using Godot;

namespace FractalPike.PikeConsole.Core.RuntimeExecution;
public static class ArgumentParser
{
	public static bool ValidateCount(ReadOnlySpan<string> args, int count, out string error)
	{
		if (!ValidateCount(args, count, count, out error))
		{
			error = $"{error} Argument count must be exactly {count}.";
			return false;
		}

		error = string.Empty;
		return true;
	}

	public static bool ValidateCount(ReadOnlySpan<string> args, int min, int max, out string error)
	{
		int n = args.Length;
		error = string.Empty;

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

	public static bool ValidateCount(ReadOnlySpan<string> args, ReadOnlySpan<int> counts, out int count, out string error)
	{
		int len = args.Length;
		count = -1;

		for (int i = 0; i < counts.Length; i++)
		{
			if (len == counts[i])
			{
				count = len;
				error = string.Empty;
				return true;
			}
		}

		error = $"Invalid argument count. Expected one of: {string.Join(", ", counts.ToArray())}";
		return false;
	}

	// ----- ----- Shorthands ----- -----
	/// <summary>
	/// Invariant shorthand.
	/// </summary>
	public static bool TryParseFloat(ReadOnlySpan<char> input, out float value) =>
	float.TryParse(input, System.Globalization.CultureInfo.InvariantCulture, out value);

	/// <summary>
	/// Invariant shorthand.
	/// </summary>
	public static bool TryParseDouble(ReadOnlySpan<char> input, out double value) =>
	double.TryParse(input, System.Globalization.CultureInfo.InvariantCulture, out value);

	// ----- ----- CUSTOM PARSING ----- -----
	// CVar 
	public static bool TryParseBool(ReadOnlySpan<char> input, out bool value, out string error)
	{
		error = string.Empty;

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

		error = $"Can't parse \"{input}\" to a boolean value!";
		value = false;
		return false;
	}

	public static bool TryParseEnum(ReadOnlySpan<char> input, ReadOnlySpan<string> options, out int index, out string error)
	{
		error = string.Empty;
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

		error = $"No match for \"{input}\".";
		return false;
	}

	// ----- ----- GODOT PARSING ----- -----
	/*
	 * Note: 
	 * Some of the code below is not as DRY as it could be. 
	 * Focus has been on getting the features to run, not flexing abstractions.
	*/
	// Color [r,g,b,a] [r,g,b] [c] (NOTE TO SELF: "c": Color.FromString() allows hex and plaintext)
	public static bool TryParseColor(ReadOnlySpan<string> args, out Color value, out string error)
	{
		value = Colors.Black;

		// If arguments are whack, early return.
		if (!ValidateCount(args, [1, 3, 4], out int argCount, out error))
			return false;

		// If we just have 1 argument, we can assume a hex code has been passed.
		if (argCount == 1)
		{
			var fallback = new Color(-1, -1, -1, -1);
			value = Color.FromString(args[0], fallback);

			if (value == fallback)
			{
				// Since -1 on all values is impossible, we know this is our custom fallback color.
				error = $"Cannot parse \"{args[0]}\" to a Color.";
				value = Colors.Black;
				return false;
			}

			return true;
		}

		// If we have 3 or 4 arguments, we can assume either RGB or RGBA is passed.
		// Thus, we must make sure all arguments are actually ints. Luckily I've made a method for just that.
		if (!TryParseManyByte(args, out byte[] channels, out error))
			return false;

		byte r = channels[0];
		byte g = channels[1];
		byte b = channels[2];
		byte a = argCount == 4 ? channels[3] : (byte)255;

		value = Color.Color8(r, g, b, a);
		return true;
	}

	// ----- ----- -------- ----- -----
	// ----- ----- VECTOR 3 ----- -----
	// ----- ----- -------- ----- -----

	/// <summary>
	/// Simple Vector3 parse. Checks for exactly 3 arguments and if they are all floats.
	/// </summary>
	/// <param name="args">Arguments to parse</param>
	/// <param name="value">Usable vector</param>
	/// <param name="error">Error. Empty if successfull</param>
	/// <returns></returns>
	public static bool TryParseVector3(ReadOnlySpan<string> args, out Vector3 value, out string error)
	{
		value = Vector3.Zero;

		if (!ValidateCount(args, 3, out error))
			return false;

		if (!TryParseManyFloat(args, out float[] axies, out error))
			return false;

		error = string.Empty;
		value = new Vector3(axies[0], axies[1], axies[2]);
		return true;
	}

	/// <summary>
	/// Contextual Vector3 parse. Checks for exactly 3 arguments and uses XYZ as value pairs.
	/// </summary>
	/// <param name="args">Arguments to parse</param>
	/// <param name="value">Usable vector</param>
	/// <param name="error">Error. Empty if successfull</param>
	/// <returns></returns>
	public static bool TryParseVector3Contextual(ReadOnlySpan<string> args, Vector3 currentState, out Vector3 value, out string error)
	{
		value = currentState;

		// If the arg length isn't right we don't even bother with anything else.
		if (!ValidateCount(args, 3, out error))
			return false;

		// Dark magic that forces allocation on the stack. Lowkey overoptimization. 
		// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/stackalloc
		Span<float> axies = stackalloc float[3];

		for (int i = 0; i < 3; i++)
		{
			string arg = args[i];

			// Here we're just hard-checking the context.
			// I F-ING LOVE DISJUNCTIVE PATTERNS!!
			if (arg is "x" or "X")
				axies[i] = currentState.X;
			else if (arg is "y" or "Y")
				axies[i] = currentState.Y;
			else if (arg is "z" or "Z")
				axies[i] = currentState.Z;

			// If it's not a contextual string, we try to parse it. 
			else if (float.TryParse(arg, System.Globalization.CultureInfo.InvariantCulture, out float f))
				axies[i] = f;

			// If it's none of all that, we're dealing with whack data.
			else
			{
				value = Vector3.Zero;
				error = $"Failed to parse \"{args[i]}\" at index ({i}). Expected a number or x, y, z.";
				return false;
			}
		}

		error = string.Empty;
		value = new Vector3(axies[0], axies[1], axies[2]);
		return true;
	}

	/// <summary>
	/// Simple Vector3I (integer) parse. Checks for exactly 3 arguments and if they are all ints.
	/// </summary>
	/// <param name="args">Arguments to parse</param>
	/// <param name="value">Usable vector</param>
	/// <param name="error">Error. Empty if successfull</param>
	public static bool TryParseVector3I(ReadOnlySpan<string> args, out Vector3I value, out string error)
	{
		value = Vector3I.Zero;

		if (!ValidateCount(args, 3, out error))
			return false;

		if (!TryParseManyInt(args, out int[] axies, out error))
			return false;

		error = string.Empty;
		value = new Vector3I(axies[0], axies[1], axies[2]);
		return true;
	}

	/// <summary>
	/// Contextual Vector3I (integer) parse. Checks for exactly 3 arguments and uses XYZ as value pairs.
	/// </summary>
	/// <param name="args">Arguments to parse</param>
	/// <param name="value">Usable vector</param>
	/// <param name="error">Error. Empty if successfull</param>
	/// <returns></returns>
	public static bool TryParseVector3IContextual(ReadOnlySpan<string> args, Vector3I currentState, out Vector3I value, out string error)
	{
		value = currentState;

		// If the arg length isn't right we don't even bother with anything else.
		if (!ValidateCount(args, 3, out error))
			return false;

		// Dark magic that forces allocation on the stack. Lowkey overoptimization. 
		// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/stackalloc
		Span<int> axies = stackalloc int[3];

		for (int i = 0; i < 3; i++)
		{
			string arg = args[i];

			// Here we're just hard-checking the context.
			// I F-ING LOVE DISJUNCTIVE PATTERNS!!
			if (arg is "x" or "X")
				axies[i] = currentState.X;
			else if (arg is "y" or "Y")
				axies[i] = currentState.Y;
			else if (arg is "z" or "Z")
				axies[i] = currentState.Z;

			// If it's not a contextual string, we try to parse it. 
			else if (int.TryParse(arg, out int parsedInt))
				axies[i] = parsedInt;

			// If it's none of all that, we're dealing with whack data.
			else
			{
				value = Vector3I.Zero;
				error = $"Failed to parse \"{args[i]}\" at index ({i}). Expected a whole number or x, y, z.";
				return false;
			}
		}

		error = string.Empty;
		value = new Vector3I(axies[0], axies[1], axies[2]);
		return true;
	}

	// ----- ----- -------- ----- -----
	// ----- ----- VECTOR 2 ----- -----
	// ----- ----- -------- ----- -----

	/// <summary>
	/// Simple Vector2 parse. Checks for exactly 2 arguments and if they are all floats.
	/// </summary>
	/// <param name="args">Arguments to parse</param>
	/// <param name="value">Usable vector</param>
	/// <param name="error">Error. Empty if successfull</param>
	/// <returns></returns>
	public static bool TryParseVector2(ReadOnlySpan<string> args, out Vector2 value, out string error)
	{
		value = Vector2.Zero;

		if (!ValidateCount(args, 2, out error))
			return false;

		if (!TryParseManyFloat(args, out float[] axies, out error))
			return false;

		error = string.Empty;
		value = new Vector2(axies[0], axies[1]);
		return true;
	}

	/// <summary>
	/// Contextual Vector2 parse. Checks for exactly 2 arguments and uses XY as value pairs.
	/// </summary>
	/// <param name="args">Arguments to parse</param>
	/// <param name="value">Usable vector</param>
	/// <param name="error">Error. Empty if successfull</param>
	/// <returns></returns>
	public static bool TryParseVector2Contextual(ReadOnlySpan<string> args, Vector2 currentState, out Vector2 value, out string error)
	{
		value = currentState;

		// If the arg length isn't right we don't even bother with anything else.
		if (!ValidateCount(args, 2, out error))
			return false;

		// Dark magic that forces allocation on the stack. Lowkey overoptimization. 
		// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/stackalloc
		Span<float> axies = stackalloc float[2];

		for (int i = 0; i < 2; i++)
		{
			string arg = args[i];

			// Here we're just hard-checking the context.
			// I F-ING LOVE DISJUNCTIVE PATTERNS!!
			if (arg is "x" or "X")
				axies[i] = currentState.X;
			else if (arg is "y" or "Y")
				axies[i] = currentState.Y;

			// If it's not a contextual string, we try to parse it. 
			else if (float.TryParse(arg, System.Globalization.CultureInfo.InvariantCulture, out float f))
				axies[i] = f;

			// If it's none of all that, we're dealing with whack data.
			else
			{
				value = Vector2.Zero;
				error = $"Failed to parse \"{args[i]}\" at index ({i}). Expected a number or x, y.";
				return false;
			}
		}

		error = string.Empty;
		value = new Vector2(axies[0], axies[1]);
		return true;
	}

	/// <summary>
	/// Simple Vector2I (integer) parse. Checks for exactly 2 arguments and if they are all ints.
	/// </summary>
	/// <param name="args">Arguments to parse</param>
	/// <param name="value">Usable vector</param>
	/// <param name="error">Error. Empty if successfull</param>
	public static bool TryParseVector2I(ReadOnlySpan<string> args, out Vector2I value, out string error)
	{
		value = Vector2I.Zero;

		if (!ValidateCount(args, 2, out error))
			return false;

		if (!TryParseManyInt(args, out int[] axies, out error))
			return false;

		error = string.Empty;
		value = new Vector2I(axies[0], axies[1]);
		return true;
	}

	/// <summary>
	/// Contextual Vector3I (integer) parse. Checks for exactly 3 arguments and uses XYZ as value pairs.
	/// </summary>
	/// <param name="args">Arguments to parse</param>
	/// <param name="value">Usable vector</param>
	/// <param name="error">Error. Empty if successfull</param>
	/// <returns></returns>
	public static bool TryParseVector2IContextual(ReadOnlySpan<string> args, Vector2I currentState, out Vector2I value, out string error)
	{
		value = currentState;

		// If the arg length isn't right we don't even bother with anything else.
		if (!ValidateCount(args, 2, out error))
			return false;

		// Dark magic that forces allocation on the stack. Lowkey overoptimization. 
		// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/stackalloc
		Span<int> axies = stackalloc int[2];

		for (int i = 0; i < 2; i++)
		{
			string arg = args[i];

			// Here we're just hard-checking the context.
			// I F-ING LOVE DISJUNCTIVE PATTERNS!!
			if (arg is "x" or "X")
				axies[i] = currentState.X;
			else if (arg is "y" or "Y")
				axies[i] = currentState.Y;

			// If it's not a contextual string, we try to parse it. 
			else if (int.TryParse(arg, out int parsedInt))
				axies[i] = parsedInt;

			// If it's none of all that, we're dealing with whack data.
			else
			{
				value = Vector2I.Zero;
				error = $"Failed to parse \"{args[i]}\" at index ({i}). Expected a whole number or x, y.";
				return false;
			}
		}

		error = string.Empty;
		value = new Vector2I(axies[0], axies[1]);
		return true;
	}

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

	// ----- ----- TRYPARSEMANY WRAPPERS GOES HERE ----- -----

	/*
	 * Originally I was going to leave the generic method exposed.
	 * That idea was scrapped and replaced with these wrappers to reduce boilerplate 
	 * and provide a better DX when using the parser.
	 */

	/// <summary>
	/// Use <c>args.AsSpan(start, end)</c> if the parameters are continuous. <br />
	/// Use shorthand: <c>[args[1], args[3], args[7]]</c> if the parameters are non-continuous.
	/// </summary>
	public static bool TryParseManyInt(ReadOnlySpan<string> args, out int[] values, out string error) =>
	TryParseManyInternal(int.TryParse, args, out values, out error);

	/// <summary>
	/// Use <c>args.AsSpan(start, end)</c> if the parameters are continuous. <br />
	/// Use shorthand: <c>[args[1], args[3], args[7]]</c> if the parameters are non-continuous.
	/// </summary>
	public static bool TryParseManyByte(ReadOnlySpan<string> args, out byte[] values, out string error) =>
	TryParseManyInternal(byte.TryParse, args, out values, out error, "Bytes must contain a value between 0 - 255.");
	/// <summary>
	/// Use <c>args.AsSpan(start, end)</c> if the parameters are continuous. <br />
	/// Use shorthand: <c>[args[1], args[3], args[7]]</c> if the parameters are non-continuous.
	/// </summary>
	public static bool TryParseManyFloat(ReadOnlySpan<string> args, out float[] values, out string error) =>
	TryParseManyInternal(invariantFloatParser, args, out values, out error);
	/// <summary>
	/// Use <c>args.AsSpan(start, end)</c> if the parameters are continuous. <br />
	/// Use shorthand: <c>[args[1], args[3], args[7]]</c> if the parameters are non-continuous.
	/// </summary>
	public static bool TryParseManyDouble(ReadOnlySpan<string> args, out double[] values, out string error) =>
		TryParseManyInternal(invariantDoubleParser, args, out values, out error);

	/// <summary>
	/// Use <c>args.AsSpan(start, end)</c> if the parameters are continuous. <br />
	/// Use shorthand: <c>[args[1], args[3], args[7]]</c> if the parameters are non-continuous.
	/// </summary>
	public static bool TryParseManyBool(ReadOnlySpan<string> args, out bool[] values, out string error) =>
		TryParseManyInternal((string s, out bool b) => TryParseBool(s.AsSpan(), out b, out _), args, out values, out error);

	/// <summary>
	/// Use <c>args.AsSpan(start, end)</c> if the parameters are continuous. <br />
	/// Use shorthand: <c>[args[1], args[3], args[7]]</c> if the parameters are non-continuous.
	/// </summary>
	public static bool TryParseManyEnum(ReadOnlySpan<string> args, string[] options, out int[] values, out string error) =>
		TryParseManyInternal((string s, out int idx) => TryParseEnum(s.AsSpan(), options, out idx, out _), args, out values, out error);

	// ----- ----- WRAPPERS ABOVE THIS LINE! ----- -----

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
	static bool TryParseManyInternal<T>(TryParseDelegate<T> parser, ReadOnlySpan<string> args, out T[] values, out string error, string msgAddon = "")
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
				string addon = string.IsNullOrWhiteSpace(msgAddon) ? string.Empty : $" {msgAddon}";
				error = $"Failed to parse \"{args[i]}\" at index ({i}).{addon}";
				return false;
			}
		}

		error = string.Empty;
		return true;
	}
}
