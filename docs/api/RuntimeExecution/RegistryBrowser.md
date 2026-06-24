# RegistryBrowser

TODO: Create a registry browser wrapper that wraps both AliasRegistry and RuntimeExecutableRegistry. Make helper methods like: 

- Find command
- Find cvar
- Find alias
- Find any

And route them using type comparison etc (AOT-safe).  

Also, document that system when it's done, lol.

REMINDER TO SELF!!!!

If the searchmode is "Exact", skip the query completely.  
Just check if the key is registered in the / any registry.  

Any of generic search, the if special search, obv.