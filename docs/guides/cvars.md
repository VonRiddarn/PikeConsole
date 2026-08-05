TODO: Explain how and when to use CVars and how to expand and create your own CVar.  

**When to use CVars**  
**Global state (GamePlay & Environment modifiers)**  
Talk about how you can add `ph_gravity` and a  `pl_gravity` modifier.  
How this can be used when performing QA or when setting up specific maps.  
EG: Moonbase automatically sets the CVar `ph_gravity` to a lower value on map load.  
Or: Snowmap automatically sets the `cl_blur_intensity` and `pl_movement_max_speed`.  

**Settigns**  
Talk about how the the audio manager slider can just point to a CVar and subscribe to it instead of having to check a massive config for specific settings.  
Making a new slider is just douplicating it and switching the CVar resource reference.  
If done this way it also marries well with the CFG system that can execute commands on startup, meaning the settings save system is already prepared!  

- CVars (Before commands, because they are easier!)
-   - CVar folder
-   - Right click creation flow
-   - Consume the CVar from a subscriber
-   - More info, like how to make your own CVar type (link to `docs/cvars.md`)