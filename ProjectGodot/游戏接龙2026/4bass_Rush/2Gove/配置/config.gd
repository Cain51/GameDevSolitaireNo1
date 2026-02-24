class_name Config

var _curtains: Dictionary = {}  # id -> Curtain

# 加载CSV配置
func load_config(file_path: String) -> void:
	var file = FileAccess.open(file_path, FileAccess.READ)
	if file == null:
		push_error("无法打开文件: ", file_path)
		return
	
	var content = file.get_as_text()
	var lines = content.split("\n")  # PackedStringArray
	
	if lines.size() < 2:
		push_error("CSV文件无数据")
		return
	
	var line_index = 0
	# 跳过表头（第0行）
	line_index += 1
	
	while line_index < lines.size():
		var line = lines[line_index].strip_edges()
		if line.is_empty():
			line_index += 1
			continue
		
		# 解析CSV行（支持引号内换行）
		var parse_result = _parse_csv_line(lines, line_index)
		var values = parse_result[0]
		var last_line_index = parse_result[1]
		
		if values.size() < 4:
			line_index = last_line_index + 1
			continue
		
		# 创建场景
		var curtain = Curtain.new()
		curtain.curtain_id = int(values[0])
		curtain.title = values[1]
		curtain.image_path = values[2]
		curtain.contenxt = values[3]
		curtain.choices = []
		
		# 解析选择项（每2列一组: 文本, 跳转ID）
		var choice_index = 4
		var choice_counter = 0
		
		while choice_index + 1 < values.size() and choice_index < 10:
			var choice_text = values[choice_index]
			var jump_id_str = values[choice_index + 1]
			
			# 跳过空选项
			if not choice_text.is_empty() and not jump_id_str.is_empty():
				var choice = Choice.new()
				choice.choice_id = curtain.curtain_id * 100 + choice_counter
				choice.context = choice_text
				choice.jumpCurtain = int(jump_id_str)
				curtain.choices.append(choice)
				choice_counter += 1
			
			choice_index += 2
		
		if values.size() > 10:
			curtain.flag = values[10].strip_edges()

		_curtains[curtain.curtain_id] = curtain
		line_index = last_line_index + 1
	
	print("加载完成: %d 个场景" % _curtains.size())

# 通过ID获取场景
func get_curtain(id: int) -> Curtain:
	return _curtains.get(id, null)

# CSV解析函数 - 支持引号内换行
func _parse_csv_line(lines: PackedStringArray, start_line: int) -> Array:
	var result: PackedStringArray
	var current = ""
	var in_quote = false
	var i = 0
	var line_index = start_line
	var line = lines[line_index]
	
	while true:
		while i < line.length():
			var c = line[i]
			
			if c == '"':
				in_quote = !in_quote
			elif c == ',' and not in_quote:
				# 处理字段内容
				current = current.strip_edges()
				# 移除首尾引号并处理转义
				if current.begins_with('"') and current.ends_with('"'):
					current = current.substr(1, current.length() - 2)
					current = current.replace('""', '"')
				result.append(current)
				current = ""
			else:
				current += c
			
			i += 1
		
		# 如果当前在引号内且还有下一行，则继续读取
		if in_quote and line_index + 1 < lines.size():
			line_index += 1
			line = lines[line_index]
			current += "\n"  # 添加换行符
			i = 0
			continue
		else:
			break
	
	# 处理最后一个字段
	current = current.strip_edges()
	if current.begins_with('"') and current.ends_with('"'):
		current = current.substr(1, current.length() - 2)
		current = current.replace('""', '"')
	result.append(current)
	
	return [result, line_index]

# 可选：批量获取多个场景
func get_curtains(ids: Array[int]) -> Array[Curtain]:
	var result: Array[Curtain] = []
	for id in ids:
		var curtain = get_curtain(id)
		if curtain:
			result.append(curtain)
	return result

# 可选：获取所有场景
func get_all_curtains() -> Array[Curtain]:
	return _curtains.values()

# 可选：重置配置
func clear():
	_curtains.clear()

# 可选：保存为Resource文件
func save_as_resources(output_dir: String):
	var count = 0
	for curtain in _curtains.values():
		var path = output_dir + "/curtain_" + str(curtain.curtain_id) + ".tres"
		var error = ResourceSaver.save(curtain, path)
		if error == OK:
			count += 1
		else:
			push_error("保存失败: ", path, " 错误码: ", error)
	print("已保存 %d 个资源到: %s" % [count, output_dir])
