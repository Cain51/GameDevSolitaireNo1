extends Node

const config_file_path = "res://2Gove/配置/接龙Godot-工作表1.csv"
var config = Config.new()


var flags: Dictionary = {}

func set_flag(flag_name: String) -> void:
	if flag_name.strip_edges().is_empty(): return
	flags[flag_name] = true
	print("系统：获得标记 -> ", flag_name)

func has_flag(flag_name: String) -> bool:
	return flags.get(flag_name, false)

func reset_game() -> void:
	flags.clear()

func _ready() -> void:
	config.load_config(config_file_path)
