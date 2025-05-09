extends CodeHighlighter

var command_color = Color("#00ffff")
var flag_color = Color("#ffaa00")
var arg_color = Color("#282828")
var semicolon_color = Color("#ffffff")
func _get_line_syntax_highlighting(line: int) -> Dictionary:
	var commands = get_text_edit().get_line(line).split(";", true)
	var current_index = 0
	var highlight = {}
	for i in range(0, len(commands)):
		var command = commands[i]
		var tokens = command.split(" ", true)
		var firstToken = true
		for j in range(0, len(tokens)):
			var token = tokens[j]
			if len(token) == 0: continue
			if firstToken: 
				highlight[current_index] = { "color": command_color }
				firstToken = false
			elif token.begins_with("-"): highlight[current_index] = { "color": flag_color }
			else: highlight[current_index] = { "color" : arg_color }
			current_index += len(token) + 1
		highlight[current_index-1] = { "color": semicolon_color }
		current_index += 1
	return highlight
