class_name Plot extends Control

@onready var image = $BGImage
@onready var title = $PlotTitle
@onready var context = $Panel/Label as Label
@onready var choice1 = $ChoiceList/Button
@onready var choice2 = $ChoiceList/Button2
@onready var choice3 = $ChoiceList/Button3
@onready var choices = [self.choice1, self.choice2, self.choice3]

var _curtain: Curtain
signal on_choice_selected
var is_playing = false
const weizhitupian = "res://2Gove/图片/未知图片.png"

func _ready() -> void:
	for i in choices.size():
		var button = choices[i] as Button
		button.pressed.connect(_on_button_pressed.bind(i))
		button.hide()
		
func _on_button_pressed(i: int):
	# 显示中时不能点击按钮
	if is_playing:
		return
		
	var choice = _curtain.choices[i] if i < _curtain.choices.size() else null
	if choice == null:
		print("未知选项: %d" % i)
	else:
		print("选择 %d: %s" % [i, choice.context])
		on_choice_selected.emit(i, choice.jumpCurtain)

func load_data(curtain: Curtain):
	print("load_data: %s" % curtain.title)
	_curtain = curtain
	if curtain.image_path.is_empty():
		image.texture = load(weizhitupian)
	else:
		image.texture = load(curtain.image_path)
	title.text = curtain.title
	context.text = curtain.contenxt
	for i in choices.size():
		var choice = curtain.choices[i] if i < curtain.choices.size() else null
		var button = self.choices[i] as Button
		if choice == null:
			button.hide()
		else:
			button.show()
			button.text = choice.context
	
	show_anim()

var speaker = DisplayServer.tts_get_voices()[0]["name"]
func show_anim():
	is_playing = true
	DisplayServer.tts_stop()
	DisplayServer.tts_speak(context.text, speaker, 50, 1.0, 1.5)  # 语音朗读
	var tween = get_tree().create_tween()
	tween.tween_property(context, "visible_ratio", 1, 2).from(0)
	await tween.finished
	is_playing = false
