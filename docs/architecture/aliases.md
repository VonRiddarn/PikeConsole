TODO: Explain aliases and how the system works (like that it is runtime only, and that commands / CVars override aliases)  
Add a note on recursion protection and provide a copy-paste example, like: 

`alias A "echo A; B"; alias B "echo B; C"; alias C "echo C; A"`  
This also flexes the command parser.
`> A` or `> B` or `> C`  
Show that the all commands still run before the recurison hits, meaning we actually don't lose any data.  
