extends Node

@onready var plot = $UI/Plot as Plot

var curtain: Curtain

func _ready() -> void:
	plot.on_choice_selected.connect(_choice_select)
	
	# 初始化第一个场景
	curtain = GameManager.config.get_curtain(100)
	if curtain:
		plot.load_data(curtain)

func _choice_select(target_id: int):
	if curtain.curtain_id == 302 and target_id == 303:
		if GameManager.has_flag("锦囊1"):
			target_id = 303
		elif GameManager.has_flag("锦囊2"):
			target_id = 304
		elif GameManager.has_flag("锦囊3"):
			target_id = 305
		print("302 分支检测完成，最终跳转目标：", target_id)

	var next_curtain = GameManager.config.get_curtain(target_id)
	
	if next_curtain:
		curtain = next_curtain
		

		if not curtain.flag.is_empty():
			GameManager.set_flag(curtain.flag)
		
		plot.load_data(curtain)
	else:
		push_error("跳转失败：未找到 ID 为 %d 的剧情" % target_id)
