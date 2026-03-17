extends Node2D

@onready var roll_button: Button = $BG/VBox/RollButton
@onready var result_label: Label = $BG/VBox/ResultPanel/ResultLabel
@onready var exit_button: Button = $BG/ExitButton

var _phase: int = 0
var _timer: Timer
var _groups := ["Unity组", "Godot组", "究极彩虹混编小组"]
var _second_phase_pools := {
	"Unity组": ["阿吉", "Alfafar", "耳听怒", "季伯常"],
	"Godot组": ["Kenner", "Gove", "瑞天", "bass_Rush", "林九"],
	"究极彩虹混编小组": ["路过的红色皮卡", "猫猫D菌", "尘聲", "龙QAVC"],
}
var _current_pool: Array = []
var _first_group: String = ""

func _ready() -> void:
	randomize()
	_timer = Timer.new()
	_timer.wait_time = 0.05
	_timer.one_shot = false
	add_child(_timer)
	_timer.timeout.connect(_on_timer_timeout)
	roll_button.pressed.connect(_on_roll_button_pressed)
	exit_button.pressed.connect(_on_exit_button_pressed)
	result_label.text = ""
	_current_pool = _groups.duplicate()
	_update_button_text()

func _on_roll_button_pressed() -> void:
	if _phase == 0:
		_current_pool = _groups
		_timer.start()
		_phase = 1
		_update_button_text()
	elif _phase == 1:
		_timer.stop()
		_first_group = result_label.text
		_phase = 2
		_update_button_text()
	elif _phase == 2:
		if _second_phase_pools.has(_first_group):
			_current_pool = _second_phase_pools[_first_group]
		else:
			_current_pool = []
		_timer.start()
		_phase = 3
		_update_button_text()
	elif _phase == 3:
		_timer.stop()
		_phase = 4
		_update_button_text()
	elif _phase == 4:
		_phase = 0
		_first_group = ""
		_current_pool = _groups
		result_label.text = ""
		_update_button_text()

func _on_timer_timeout() -> void:
	if _current_pool.size() == 0:
		return
	var index := randi() % _current_pool.size()
	result_label.text = _current_pool[index]

func _on_exit_button_pressed() -> void:
	get_tree().quit()

func _update_button_text() -> void:
	match _phase:
		0:
			roll_button.text = "开始分组抽取"
		1:
			roll_button.text = "停"
		2:
			roll_button.text = "开始选手抽取"
		3:
			roll_button.text = "停"
		4:
			roll_button.text = "重新抽取"
