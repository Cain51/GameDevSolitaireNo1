extends Button
@onready var texture_rect: TextureRect = $"../TextureRect"
@onready var b_j_2: Button = $"../B_J_2"
@onready var b_j_3: Button = $"../B_J_3"

@onready var texture_rect1: TextureRect = $"../../jt1/TextureRect"
@onready var button: Button = $"../../jt1/Button"

func _on_pressed() -> void:
	hide()
	b_j_2.hide()
	b_j_3.hide()
	texture_rect.hide()
	
	texture_rect1.show()
	button.show()
