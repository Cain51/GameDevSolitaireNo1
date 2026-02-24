extends Button
@onready var texture_rect: TextureRect = $"../TextureRect"
@onready var button_1: Button = $"../Button1"
@onready var button_3: Button = $"../Button3"

@onready var t_2: TextureRect = $"../../bt2/t2"
@onready var b_2: Button = $"../../bt2/b2"


func _on_pressed() -> void:
	texture_rect.hide()
	button_1.hide()
	button_3.hide()
	hide()
	
	t_2.show()
	b_2.show()
