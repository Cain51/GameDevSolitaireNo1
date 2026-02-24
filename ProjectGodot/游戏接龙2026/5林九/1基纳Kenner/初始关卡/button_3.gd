extends Button
@onready var texture_rect: TextureRect = $"../TextureRect"
@onready var button_1: Button = $"../Button1"
@onready var button_2: Button = $"../Button2"

@onready var t_3: TextureRect = $"../../bt3/t3"
@onready var b_3: Button = $"../../bt3/b3"


func _on_pressed() -> void:
	texture_rect.hide()
	button_1.hide()
	button_2.hide()
	hide()
	
	t_3.show()
	b_3.show()
