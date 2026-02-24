extends Button
@onready var texture_rect: TextureRect = $"../TextureRect"
@onready var b_j_1: Button = $"../B_J_1"
@onready var b_j_3: Button = $"../B_J_3"

@onready var texture_rect1: TextureRect = $"../../jt2/TextureRect"
@onready var button: Button = $"../../jt2/Button"



func _on_pressed() -> void:
	hide()
	b_j_1.hide()
	b_j_3.hide()
	texture_rect.hide()
	
	texture_rect1.show()
	button.show()
