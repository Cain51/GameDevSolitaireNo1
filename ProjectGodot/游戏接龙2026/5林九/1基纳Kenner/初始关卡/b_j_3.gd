extends Button
@onready var texture_rect: TextureRect = $"../TextureRect"
@onready var b_j_1: Button = $"../B_J_1"
@onready var b_j_2: Button = $"../B_J_2"

@onready var texture_rect1: TextureRect = $"../../jt3/TextureRect"
@onready var button: Button = $"../../jt3/Button"


func _on_pressed() -> void:
	hide()
	b_j_1.hide()
	b_j_2.hide()
	texture_rect.hide()
	
	texture_rect1.show()
	button.show()
