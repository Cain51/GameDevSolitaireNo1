extends Button
@onready var texture_rect: TextureRect = $"../TextureRect"
@onready var button_2: Button = $"../Button2"
@onready var button_3: Button = $"../Button3"
@onready var t_1: TextureRect = $"../../bt1/t1"
@onready var b_1: Button = $"../../bt1/b1"

func _on_pressed() -> void:
	texture_rect.hide()
	button_2.hide()
	button_3.hide()
	hide()
	
	t_1.show()
	b_1.show()
