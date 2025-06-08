extends CodeHighlighter

const CMD_COLOR : Color = Color("#00ffff")
const FLAG_COLOR : Color = Color("#ffaa00")
const ARG_COLOR : Color = Color("#282828")
const SEMICOLON_COLOR : Color = Color("#ffffff")
func _get_line_syntax_highlighting(line: int) -> Dictionary:
	var input_str : String = get_text_edit().get_line(line)
	var highlight_dict = {}
	var in_quote = false
	var current_token = ''
	var token_start = 0
	var index = 0
	var commands = []
	var current_command = []

	# First, split by ; while keeping track of quoted strings
	while index < len(input_str):
		var chr = input_str[index]

		if chr == '"':
			in_quote = not in_quote

		if chr == ';' and not in_quote:
			if current_token:
				current_command.append([current_token, token_start])
				current_token = ''
			if current_command:
				commands.append(current_command)
			# Highlight the semicolon
			current_command.append([";", index])
			index += 1
			current_command = []
			continue

		if chr == " " and not in_quote:
			if current_token:
				current_command.append([current_token, token_start])
				current_token = ''
		else:
			if not current_token:
				token_start = index
				current_token += chr

		index += 1

	if current_token:
		current_command.append([current_token, token_start])
	if current_command:
		commands.append(current_command)
	
	# Now, highlight each command
	for cmd in commands:
		for i in range(len(cmd)):
			var c = cmd[i]
			if i == 0:
				highlight_dict[c[1]] = { "color": CMD_COLOR }
			elif c[0][0] == "-":
				highlight_dict[c[1]] = { "color": FLAG_COLOR }
			elif c[0] == ";":
				highlight_dict[c[1]] = { "color": SEMICOLON_COLOR}
			else:
				highlight_dict[c[1]] = { "color": ARG_COLOR }
	return highlight_dict
