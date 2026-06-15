@tool
extends EditorPlugin

# AUTHORS NOTE:
# I could've written this in C# and be done with it.
# It would've been so much easier, so much cleaner. 
# BUT NO. I went into the GDScript trenches for YOU.
# Yes, YOU, the user.
# 
# On a serious note:
# Writing this in GDScript allows us to inject the property drawers without having to build the project.
# It makes the addon true drag-and-drop and reduces friction.
# 
# 2026-06-10 - 01:48
# This script took me about 7 hours to make.
# The Godot ecosystem is an unforgiving beast.


# ----- ----- ----- ----- -----
# 			AUTOLOAD
# ----- ----- ----- ----- -----
const AUTOLOAD_NAME = "PikeConsoleBackend"
const AUTOLOAD_PATH = "res://addons/PikeConsole/Core/pike_console_backend.tscn"
# Basically: We use the autoloader scene tree to bridge engine logs, scan for CVars and initialize default commands.
# The actual UI is not auto-injected as there is a high chance users want to customize it or make their own.

# ----- ----- ----- ----- -----
# 			META
# ----- ----- ----- ----- -----
# The prefix the addon uses during setup.
# Doesn't affect the "real" framework, just the addon initialization.
const ADDON_PRINT_PREFIX = "[PikeConsole] "

# Predefined path structure for easier visual scope...
const PATH_ADDON_ROOT: String = "res://addons/PikeConsole"
const PATH_SETTINGS_ROOT: String = "fractal_pike/pike_console/"
const PATH_SETTINGS_CONFIG: String = PATH_SETTINGS_ROOT + "config/"
const PATH_SETTINGS_RUNTIME: String = PATH_SETTINGS_ROOT + "runtime/"
const PATH_SETTINGS_EDITOR: String = PATH_SETTINGS_ROOT + "editor/"


# ----- ----- ----- ----- -----
# 			SETTINGS
# ----- ----- ----- ----- -----
const SETTING_PATHMAP: Dictionary[String, Variant] = {
	"path": PATH_SETTINGS_ROOT + "pathmap", 
	"default_value": "", 
	"type": TYPE_STRING, 
	"hint": PROPERTY_HINT_NONE, 
	"hint_string": ""
}
const SETTING_CVAR_DIRECTORY: Dictionary[String, Variant] = {
	"path": PATH_SETTINGS_ROOT + "cvar_directory", 
	"default_value": "res://cvars", 
	"type": TYPE_STRING, 
	"hint": PROPERTY_HINT_DIR, 
	"hint_string": ""
}
const SETTING_CONFIG_DIRECTORY: Dictionary[String, Variant] = {
	"path": PATH_SETTINGS_CONFIG + "config_directory", 
	"default_value": "user://cfg", 
	"type": TYPE_STRING, 
	"hint": PROPERTY_HINT_DIR, 
	"hint_string": ""
}
const SETTING_USE_USER_CONFIGS: Dictionary[String, Variant] = {
"path": PATH_SETTINGS_CONFIG + "use_user_configs", 
"default_value": false, 
"type": TYPE_BOOL, 
"hint": PROPERTY_HINT_NONE, 
"hint_string": ""
}
const SETTING_MAX_UI_LOGS: Dictionary[String, Variant] = {
	"path": PATH_SETTINGS_RUNTIME + "max_ui_logs", 
	"default_value": 500, 
	"type": TYPE_INT, 
	"hint": PROPERTY_HINT_RANGE, 
	"hint_string": "-1,1000,1"
}
const SETTING_SUPRESS_DOCUMENTATION_WARNINGS: Dictionary[String, Variant] = {
	"path": PATH_SETTINGS_EDITOR + "suppress_documentation_warnings", 
	"default_value": false, 
	"type": TYPE_BOOL, 
	"hint": PROPERTY_HINT_NONE, 
	"hint_string": ""
}
const SETTING_COLOR_INFO: Dictionary[String, Variant] = {
	"path": PATH_SETTINGS_EDITOR + "colors/" + "info", 
	"default_value": Color("#EBEBEB"), 
	"type": TYPE_COLOR, 
	"hint": PROPERTY_HINT_COLOR_NO_ALPHA, 
	"hint_string": ""
}
const SETTING_COLOR_SUCCESS: Dictionary[String, Variant] = {
	"path": PATH_SETTINGS_EDITOR + "colors/" + "success", 
	"default_value": Color("#B2FF73"), 
	"type": TYPE_COLOR, 
	"hint": PROPERTY_HINT_COLOR_NO_ALPHA, 
	"hint_string": ""
}
const SETTING_COLOR_WARNING: Dictionary[String, Variant] = {
	"path": PATH_SETTINGS_EDITOR + "colors/" + "warning", 
	"default_value": Color("#FFC973"), 
	"type": TYPE_COLOR, 
	"hint": PROPERTY_HINT_COLOR_NO_ALPHA, 
	"hint_string": ""
}
const SETTING_COLOR_ERROR: Dictionary[String, Variant] = {
	"path": PATH_SETTINGS_EDITOR + "colors/" + "error", 
	"default_value": Color("#FF7373"), 
	"type": TYPE_COLOR, 
	"hint": PROPERTY_HINT_COLOR_NO_ALPHA, 
	"hint_string": ""
}

# ----- ----- ----- ----- -----
# 			KEYBINDS
# ----- ----- ----- ----- -----
const KB_TOGGLE_CONSOLE: Dictionary[String, Variant] = {
	"name": "pike_console_toggle",
	"default_key": KEY_SEMICOLON
}

# ----- ----- ----- ----- -----
# 		INITIALIZATION
# ----- ----- ----- ----- -----
func _enter_tree() -> void:
	initialize_project_settings()
	initialize_input_map()
	initialize_directory(ProjectSettings.get_setting(SETTING_CVAR_DIRECTORY["path"]), "CVar")
	initialize_directory(ProjectSettings.get_setting(SETTING_CONFIG_DIRECTORY["path"]), "config")
	initialize_directory(ProjectSettings.get_setting(SETTING_CONFIG_DIRECTORY["path"]) + "/users", "user config")
	add_autoload_singleton(AUTOLOAD_NAME, AUTOLOAD_PATH)
	pike_log("%s autoload has been injected to the project settings." % [AUTOLOAD_NAME])

func _exit_tree() -> void:
	remove_autoload_singleton(AUTOLOAD_NAME)
	pike_log("%s autoload has been removed from the project settings." % [AUTOLOAD_NAME])

# ----- ----- ----- ----- -----
# 	INITIALIZATION HELPERS
# ----- ----- ----- ----- -----
func initialize_project_settings() -> void: 
	var settings: Array[Dictionary] = [
		SETTING_PATHMAP,
		SETTING_CVAR_DIRECTORY,
		SETTING_CONFIG_DIRECTORY,
		SETTING_USE_USER_CONFIGS,
		SETTING_MAX_UI_LOGS,
		SETTING_SUPRESS_DOCUMENTATION_WARNINGS,
		SETTING_COLOR_INFO,
		SETTING_COLOR_SUCCESS,
		SETTING_COLOR_WARNING,
		SETTING_COLOR_ERROR
	]
	
	var dirty = false
	
	for setting in settings:
		if register_setting(setting): dirty = true
		inject_setting(setting)

	if dirty:
		pike_log("New configuration settings have been added. Saving project.godot.")
		ProjectSettings.save()

func initialize_input_map() -> void:
	var dirty = register_input(KB_TOGGLE_CONSOLE)
	inject_input(KB_TOGGLE_CONSOLE)
	
	if dirty:
		ProjectSettings.save() 
		pike_log("New keybinds added to project.godot.")

func initialize_directory(dir_path: String, dir_log_name: String) -> void:
	# Check if the folder exists without throwing an error.
	# Note: we could just do "var dir = DirAccess.open(dir_path)" and it would work with less cycles
	# That would however always print an error on first launch which can be misinterpreted as the addon not working
	if not DirAccess.dir_exists_absolute(dir_path):
		var err = DirAccess.make_dir_recursive_absolute(dir_path)
		if err == OK:
			pike_log("Created %s directory at: %s" % [dir_log_name, dir_path])
		else:
			push_error("PikeConsole: Failed to create %s directory! Error code: %s" % [dir_log_name, str(err)])

# ----- ----- ----- ----- -----
# 		GENERIC HELPERS
# ----- ----- ----- ----- -----
func pike_log(message) -> void: print(ADDON_PRINT_PREFIX + message)

# Register setting writes the setting to disk (runs only once).
# Returns a dirty flag for checking if any new settings were actually added.
func register_setting(setting: Dictionary) -> bool:
	var path: String = setting["path"]
	
	# Early return so that we don't keep saving to the project.godot file!
	if ProjectSettings.has_setting(path):
		return false
	
	var default_value: Variant = setting["default_value"]
	
	# Create the setting and sets the value.
	ProjectSettings.set_setting(path, default_value)
	
	return true
	
# Inject setting injects the property drawer to the current session (runs every time).
func inject_setting(setting: Dictionary) -> void:
	var path: String = setting["path"]
	var default_value: Variant = setting["default_value"]
	var type: int = setting["type"]
	var hint: int = setting["hint"]
	var hint_string: String = setting["hint_string"]
	
	# Defines the default value (reset spinner button) for the created setting.
	ProjectSettings.set_initial_value(path, default_value)
	ProjectSettings.add_property_info({
        "name": path,
        "type": type,
        "hint": hint,
        "hint_string": hint_string
    })

# Adding keys is basically a carbon copy of how we add other settings.
# Check if they exist and add to project.godot or return early.
# Then, check if they need to be injected, and inject them.

# Register input writes the input to disk (runs only once).
# Returns a dirty flag for checking if any new settings were actually added.
func register_input(input: Dictionary) -> bool:
	var action_name: String = input["name"]
	var path: String = "input/" + action_name
	
	if ProjectSettings.has_setting(path):
		return false
	
	var default_key: int = input["default_key"]
	var key_event := InputEventKey.new()
	key_event.physical_keycode = default_key

	var action_data := {
		"deadzone": 0.5,
		"events": [key_event]
	}

	ProjectSettings.set_setting(path, action_data)
	
	return true

# Inject setting injects the input map if needed (runs every time).
func inject_input(input: Dictionary) -> void:
	# Sadly we redo a lot of allocations.
	# The perfectionist in me screams, but this happens once. At startup.
	var action_name: String = input["name"]
	var path: String = "input/" + action_name
	var default_key: int = input["default_key"]

	var key_event := InputEventKey.new()
	key_event.physical_keycode = default_key

	var action_data := {
		"deadzone": 0.5,
		"events": [key_event]
	}

	ProjectSettings.set_initial_value(path, action_data)
	
	if not InputMap.has_action(action_name):
		InputMap.add_action(action_name)
		pike_log("'" + action_name + "' has been added to the input map. Check out Project settings > Input map")
	else:
		InputMap.action_erase_events(action_name)
	InputMap.action_add_event(action_name, key_event)