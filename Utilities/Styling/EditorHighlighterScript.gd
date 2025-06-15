extends CodeHighlighter
const kolor = Color("#0080ff")
const kword = ["function", "and", "break", "do", "else", "elseif", "end", "for", "goto", "if", "in", "local", "not", "or", "repeat", "return", "then", "until", "while"]
const kolor_literal = Color("#0000ff")
const kword_literal = ["true", "false", "nil"]
const str_kolor : Color = Color("#ffcc33")
const comment_kolor : Color = Color("#00cc33")
func _init() -> void:
	for kw in kword:
		add_keyword_color(kw, kolor)
	for kw in kword_literal:
		add_keyword_color(kw, kolor_literal)
	#Single line string
	add_color_region('"', '"', str_kolor, false)
	add_color_region("'", "'", str_kolor, false)
	#Multi line string
	add_color_region("[[", "]]", str_kolor, false)
	add_color_region("[=", "=]", str_kolor, false)
	#---
	add_color_region("--", "", comment_kolor, true) #Single line comment
	add_color_region("--[[", "]]", comment_kolor, false) #Multi line comment
