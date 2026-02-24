extends Button

@onready var texture_rect: TextureRect = $"../TextureRect"
@onready var b_j_1: Button = $"../B_J_1"
@onready var b_j_2: Button = $"../B_J_2"
@onready var b_j_3: Button = $"../B_J_3"



func _on_pressed() -> void:
	hide()
	
	texture_rect.show()
	b_j_1.show()
	b_j_2.show()
	b_j_3.show()
