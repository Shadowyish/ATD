extends Node

@export var tower_prefabs : Array
@export var tower_buttons : Array
@export var tower_selection_panel : Panel
@export var tower_built : Sprite2D
@export var placement_instructions : Label

var selected_tower_prefabs = []
var selected_tower_prefab
var is_placement_active = false
var is_placement_confirmed = false
var is_tower_selected = false
var grid_manager

func start_placement_mode(grid_manager_script : Script) -> void:
	is_placement_active = true
	is_placement_confirmed = false
	grid_manager = grid_manager_script
	select_tower()
	placement_instructions.visible = true

# Called every frame
func _process(delta):
	if is_placement_active and is_tower_selected:
		handle_tower_placement()

# Load UI screen for Tower Selection
func select_tower():
	var selected_indices = []
	while selected_indices.size() < 3:
		var random_index = randi_range(0, tower_prefabs.size() - 1)
		if not selected_indices.has(random_index):
			selected_indices.append(random_index)

	for i in range(selected_indices.size()):
		selected_tower_prefabs[i] = tower_prefabs[selected_indices[i]]
		tower_buttons[i].get_node("Text").text = tower_prefabs[selected_indices[i]].name # Update button text
		tower_buttons[i].connect("pressed", self, "on_tower_button_pressed", [i])
	tower_selection_panel.visible = true # Show the selection panel

func handle_tower_placement():
	if Input.is_action_just_pressed("mouse_left"):
		var mouse_position = get_viewport().get_mouse_position()
		var target_tile = grid_manager.get_tile_from_world_position(mouse_position)
		if target_tile != null and target_tile.is_walkable():
			var renderer = target_tile.tile.get_node("Sprite")
			if renderer != null:
				renderer.texture = tower_built # Change the sprite of the tile
			place_tower(target_tile)
			confirm_placement()

func place_tower(tile):
	var position = tile.tile.position
	var new_tower = selected_tower_prefab.instance()
	new_tower.position = position
	var tower_script = new_tower.get_node("TowerManager")
	update_tile_weights(position, tower_script.get_range(), tower_script.get_damage())
	tile.is_walkable = false # Mark tile as blocked for pathfinding

func confirm_placement():
	is_placement_confirmed = true
	is_placement_active = false
	placement_instructions.visible = false

# Check if placement is confirmed (used by Game Controller)
func check_confirmed() -> bool:
	return is_placement_confirmed

# Called when a tower is selected from the UI
func on_tower_button_pressed(tower_index):
	selected_tower_prefab = selected_tower_prefabs[tower_index]
	is_tower_selected = true
	tower_selection_panel.visible = false # Hide the panel after selection

# End the placement mode
func end_placement_mode():
	is_placement_active = false
	is_tower_selected = false

# Update the weights of the tiles within range of the tower
func update_tile_weights(tower_position: Vector2, range: float, weight: int):
	for tile in grid_manager.get_grid():
		var distance = tower_position.distance_to(tile.tile.position)
		if distance <= range:
			tile.weight += weight
