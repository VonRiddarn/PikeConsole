# Commands (and how to use them!)  

What makes both commands (_and CVars_) special in PikeConsole is that you actually do not initialize them yourself.  
  
Instead we make use of a **special Node** called the [`CommandSet`](../api/RuntimeExecution/Commands/CommandSet.md)  
By inheriting from this Node, we can create a command in just a few seconds.  
  
_Let's try it out by creating a simple echo command..._  

## 1️⃣ Create a Node to host the command set

`Right click` your scene tree and press `Add a child Node` or press ++ctrl++ + ++a++.  
Create a Node using the root type `Node`.  

Name this Node anything you want. It is recommended to keep the names clear and suffix it with "CommandSet" for hierarchical clarity.  

## 2️⃣ Inherit the [`CommandSet`](../api/RuntimeExecution/Commands/CommandSet.md) class on the Node.

Add a script to the Node and have it inherit from the [`CommandSet`](../api/RuntimeExecution/Commands/CommandSet.md) class.  
/// note
The [`CommandSet`](../api/RuntimeExecution/Commands/CommandSet.md) should auto-complete in your IDE and automatically import the namespace.  
If it doesn't, you need to manually import the the namespace at the top of the file:  
```csharp {linenums="1"}
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
```
///

Once that is done, you will get prompted to implement the abstract classes.  
For VSCode, press ++ctrl++ + ++period++ and choose `Import abstract class`.  

**You should now have something like this:**

```csharp {linenums="1"}
using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
using System;

public partial class MyCommandSet : CommandSet
{
	protected override Command[] InstantiateCommands()
	{
		throw new NotImplementedException();
	}
}
```

## 3️⃣ Add the command to the Node  

The `InstantiateCommands` is a declarative method that is automatically run by the [`CommandSet`](../api/RuntimeExecution/Commands/CommandSet.md) Node to initialize commands.
Any commands returned by this method will be automatically added to the command registry.  

To instantiate the command, we will use the [`Command()`](../api/RuntimeExecution/Commands/CommandSet.md#command) shorthand method that is provided by the [`CommandSet`](../api/RuntimeExecution/Commands/CommandSet.md). 
This shorthand automatically tags our commands with self-diagnostic metadata.  

///tip
For commands that shouldn't be included in release builds of the game you can instead use the shorthand [`CommandHidden()`](../api/RuntimeExecution/Commands/CommandSet.md#commandhidden).
///

/// warning
- Do not instantiate commands using the `new` keyword.  
- Do not instantiate commands outside the `InstantiateCommands()` method.
///

I have written the code for the echo command below...

=== "Inline"
	```csharp {linenums="1"}
	using FractalPike.PikeConsole.Core.RuntimeExecution;
	using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
	using Godot;

	public partial class MyCommandSet : CommandSet
	{
		protected override Command[] InstantiateCommands() => [
			Command(
				"my_echo", // Signature
				false, // Is Cheat
				(args) => 
					new(ExecutionResponseStatus.Success, $"{args.Join(" ")}")
			),
		];
	}
	```

=== "Structured"
	```csharp {linenums="1"}
	using FractalPike.PikeConsole.Core.RuntimeExecution;
	using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
	using Godot;

	public partial class MyCommandSet : CommandSet
	{
		protected override Command[] InstantiateCommands()
		{
			return [
				Command(
					"my_echo", // Signature 
					false, // Is Cheat
					EchoCommand
				),
			];
		}

		Response<ExecutionResponseStatus> EchoCommand(string[] args)
		{
			return new(ExecutionResponseStatus.Success, $"{args.Join(" ")}");
		}
	}
	```

_There is **no real performative difference** between the two examples. Choose the syntax you like._

/// warning | Make sure to avoid naming collisions
The `echo` command is already occupied by the `GlobalCommandSet`.  
Make sure to name your command something else, like `my_echo`!
///

## 4️⃣ Run the command!  

**That's it!**  
Now you just need to open up the developer console and type in your new command!  

```
> my_echo Hello world!
Hello world!
```

Cheat protection is managed automatically. If we temporarily change `isCheat` parameter to `true`:  
```
> my_echo Hello world!
my_echo is cheat protected!
> cheatmode 1
Set cheatmode to True
> my_echo Hello world!
Hello world!
```

## 🧩 The `Command` shorthand properties

In the above example we used a custom shorthand provided by the [`CommandSet`](../api/RuntimeExecution/Commands/CommandSet.md) API.  
This shorthand comes in 2 flavors of parameters: **Documented** and **Quick**. 
Out of the two, the framework will nudge you to use the documented version.  

/// note
You can turn off warnings for undocumented commands by enabling:  
`Project settings [General]` > `Fractal Pike` > `Pike Console` > `Supress Documentation Warnings`
///

The API-Reference contains more information about the [Command](../api/RuntimeExecution/Commands/CommandSet.md#command) shorthands.

### 📄 Documenting the echo command

By applying the information above, we can now document our echo command, like so:  

=== "Inline"
	```csharp {linenums="1"}
	using FractalPike.PikeConsole.Core.RuntimeExecution;
	using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
	using Godot;

	public partial class MyCommandSet : CommandSet
	{
		protected override Command[] InstantiateCommands() => [
			Command(
					"my_echo",
					"Joins and echoes the arguments back to the caller",
					null,
					"my_echo [args...]",
					false,
					(args) =>
						new(ExecutionResponseStatus.Success, $"{args.Join(" ")}")
				),
			];
	}
	```

=== "Structured"
	```csharp {linenums="1"}
	using FractalPike.PikeConsole.Core.RuntimeExecution;
	using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
	using Godot;

	public partial class MyCommandSet : CommandSet
	{
		protected override Command[] InstantiateCommands()
		{
			return [
				Command(
						"my_echo",
						"Joins and echoes the arguments back to the caller",
						null,
						"my_echo [args...]",
						false,
						EchoCommand
					),
			];
		}

		Response<ExecutionResponseStatus> EchoCommand(string[] args)
		{
			return new(ExecutionResponseStatus.Success, $"{args.Join(" ")}");
		}
	}
	```

Adding documentation to your commands may feel tedious, but it helps immensely 
with QA testing, runtime experience and remembering what stuff does 6 months from now.

## ℹ️ **Godot Lifecycle methods** inside a [`CommandSet`](../api/RuntimeExecution/Commands/CommandSet.md) Node  

The [`CommandSet`](../api/RuntimeExecution/Commands/CommandSet.md) Node uses `_EnterTree`, `_Ready` and `_ExitTree` for internal functioning, 
thus if you need to access the lifecycle methods within a [`CommandSet`](../api/RuntimeExecution/Commands/CommandSet.md) you 
override the API-provided wrapper methods instead:  

- `OnEnterTree`  
- `OnReady`  
- `OnExitTree`  

The wrapper also contains a method for when cheatmode is edited.  
This can be used to force-disable stuff like noclip.  

- `OnCheatModeChanged`

/// note | Here is that code added to our previous example
=== "Inline"
	```csharp {linenums="1"}
	using FractalPike.PikeConsole.Core.Logging;
	using FractalPike.PikeConsole.Core.RuntimeExecution;
	using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
	using Godot;

	public partial class MyCommandSet : CommandSet
	{
		protected override void OnEnterTree() =>
			PikeLogger.Log(LogTarget.All, $"Tree entered!");

		protected override void OnReady() =>
			PikeLogger.Log(LogTarget.All, $"Node ready!");

		protected override void OnExitTree() =>
			PikeLogger.Log(LogTarget.All, $"Tree exited!");

		protected override void OnCheatModeChanged(bool newState)
		{
			if (newState == false)
			{
				PikeLogger.Log(LogTarget.All, $"Force removing noclip...");
			}
		}

		protected override Command[] InstantiateCommands() => [
			Command(
					"my_echo",
					"Joins and echoes the arguments back to the caller",
					null,
					"my_echo [args...]",
					false,
					(args) =>
						new(ExecutionResponseStatus.Success, $"{args.Join(" ")}")
				),
			];
	}
	```

=== "Structured"
	```csharp {linenums="1"}
	using FractalPike.PikeConsole.Core.Logging;
	using FractalPike.PikeConsole.Core.RuntimeExecution;
	using FractalPike.PikeConsole.Core.RuntimeExecution.Commands;
	using Godot;

	public partial class MyCommandSet : CommandSet
	{
		protected override void OnEnterTree()
		{
			PikeLogger.Log(LogTarget.All, $"Tree entered!");
		}

		protected override void OnReady()
		{
			PikeLogger.Log(LogTarget.All, $"Node ready!");
		}
		
		protected override void OnExitTree()
		{
			PikeLogger.Log(LogTarget.All, $"Tree exited!");
		}

		protected override void OnCheatModeChanged(bool newState)
		{
			if (newState == false)
			{
				PikeLogger.Log(LogTarget.All, $"Force removing noclip...");
			}
		}

		protected override Command[] InstantiateCommands()
		{
			return [
				Command(
						"my_echo",
						"Joins and echoes the arguments back to the caller",
						null,
						"my_echo [args...]",
						false,
						EchoCommand
					),
				];
		}

		Response<ExecutionResponseStatus> EchoCommand(string[] args)
		{
			return new(ExecutionResponseStatus.Success, $"{args.Join(" ")}");
		}
	}
	```
///

## 📦 Execution responses

All commands **must** return a response after execution.  
The command response builds on the generic `Resonse<T>` struct containing a `Status` and a `Message`.

The **[Statement Executor](../api/RuntimeExecution/StatementExecutor.md)** will log any response where the `Message` is not empty.  
Commands denied for being cheat protected are handled automatically.

```csharp
// This will print to the console.
return new Response<ExecutionResponseStatus> (
	ExecutionResponseStatus.InvalidArgs, 
	"Yo, those arguments are whack!"
	);
```

```csharp
// This will fail silently
return new Response<ExecutionResponseStatus> (
	ExecutionResponseStatus.InvalidArgs, 
	null
	);
```

/// note | Available ExecutionResponseStatuses
`Success`
: The command executed expectedly.

`InvalidArgs`
: The command failed to execute because of invalid string parameters.

`Failed`
: The command failed expectedly. 
_EG: Couldn't find an entity of a certain ID._

`Error`
: Failed unexpectedly.  
_EG: By catching an exception._
///