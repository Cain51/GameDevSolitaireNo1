extends Button
#1里面所有的节点_默认隐藏状态
@onready var texture_rect: TextureRect = $"../1/TextureRect"
@onready var button_1: Button = $"../1/Button1"
@onready var button_2: Button = $"../1/Button2"
@onready var button_3: Button = $"../1/Button3"
#自身Button过场1

func _on_pressed() -> void:
	#隐藏自己
	hide()
	#显示1内子节点
	button_1.show()
	button_2.show()
	button_3.show()
	texture_rect.show()
