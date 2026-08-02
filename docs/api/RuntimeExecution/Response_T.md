=== "Response&lt;T&gt;"
	# Response&lt;T&gt;
	`public readonly struct Response<T>(T status, string message = "", string[]? tags = null) where T : Enum`  

	**Inherits**: None  
	**Namespace**: `FractalPike.PikeConsole.Core.Utilities`  

	## Description

	Holds a status return type alongside an optional message.  
	Should the message be `null`, it will fallback safely to `string.Empty`.

	Used to normalize predictable responses across the system.  

	/// note | Parameters  
	`T` : `Status`
	: The status of the response. Can use any enum.

	`string` : `Message`
	: Optional response message. By default, most internal systems will log non-empty messages and ignore the rest.

	`string[]` : `Tags`
	: Optional tags for adding META-data to a response.
	///
=== "Response&lt;T, P&gt;"
	# Response&lt;T, P&gt;
	`public readonly struct Response<T, P>(T status, P payload, string message = "", string[]? tags = null) where T : Enum`  

	**Inherits**: None  
	**Namespace**: `FractalPike.PikeConsole.Core.Utilities`  

	## Description

	Holds a status return type and data-payload alongside an optional message.  
	Should the message be `null`, it will fallback safely to `string.Empty`.  

	Used to normalize predictable responses across the system.  

	/// note | Parameters  
	`T` : `Status`
	: The status of the response. Can use any enum.

	`P` : `Payload`
	: Data passed back alongside the response.

	`string` : `Message`
	: Optional response message. By default, most internal systems will log non-empty messages and ignore the rest.

	`string[]` : `Tags`
	: Optional tags for adding META-data to a response.
	///