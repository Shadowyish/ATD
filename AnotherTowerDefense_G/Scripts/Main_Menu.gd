extends Node

# Called when the "Start Game" button is clicked
func _on_StartGame_pressed():
	# TODO: Pass Logic off to Game Controller via Signal
	pass

# Called when the "Quit Game" button is clicked
func _on_QuitGame_pressed():
	get_tree().quit()
