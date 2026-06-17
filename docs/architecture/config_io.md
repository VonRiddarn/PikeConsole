TODO: Explain the config (.cfg) system and how it can be applied.  
Explain the inspiration (GoldSrc, Half life, Counter-Strike) and what advantages one gets from embracing it.  
Like: 
- Unbreakable soft-references to settings (Cool name: Decoupled UI bindings ?)
- Delta-settings editing (only save settings that are changed) Maybe call it "Delta-serialization" for coolness and clarity?
- Agnostic UI - Console commands, GUI, config...  

About the **decoupled UI bindings**  
Talk about how the the audio manager slider can just point to a CVar and subscribe to it instead of having to check a massive config for specific settings.  
Making a new slider is just douplicating it and switching the CVar resource reference.  
Config files run the command at startup, and if the setting no longer exists, nothing happens! (That's good.)  

Maybe this belongs in the CVar section under something like "When can I use CVars, and for what?"

NOTE TO SELF: Do not write this doc before the cfg system is in place!  
It might undergo lots of changes from the Unity version and baking that into the docs would be bad.