TODO: Explain the RTE system start to end.  
Commands are IRuntimeExecutable  
Cvars are ICVar, which is IRuntimeExecutable  

CVars are data, but they automatically register a command on initialization that connects them to the comand system.  
"Isn't this mixing concerns?"  
Technically yes, but also no.  
The CVar does know about the registry, which isn't a purist approach.  
However, cvars are still their own data container that handles their own scope.  
They just provide a shorthand reference to their execution method to their sibling system. There is no need for further abstractions.