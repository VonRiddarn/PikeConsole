TODO: Explain how to create commands using the CommandSet and how to run them both through code and the console.  
Make a note about the supress documentation setting in project settings - and advice against it.  
Explain that the "help" and "list" commands automatically parses the documentation metadata, and that it is helpful for QA / yourself in 6 months.  
  
Make sure to note that the pipeline is super agnostic, and that: `StatementExecutor("echo Hello World!")`  
is literally the same as the console doing `echo hello world` - since the console is just a router.  
  
Also, make sure to mention the background processes and how commands clean up after themselves using node lifecycles etc.  
Add a note on command conventions, and how commands should be added to the controller of a thing, rather than THE thing - with some exceptions ofc.  
Like: Enemies shouldn't have commands. EnemyManager should.  
But: The player can carry player-like commands, and even the "kill" system (because it might use the player POV to kill any entity the player looks at)

* Create commands
* * ArgumentParser
* Execute commands
* CommandSet