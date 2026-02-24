extends Button

@onready var t_1: TextureRect = $"../t1"

@onready var b__引导: Button = $"../../锦囊关卡/B_ 引导"

func _on_pressed() -> void:
	hide()
	t_1.hide()
	
	b__引导.show()
