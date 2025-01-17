extends Node

@export var grid_width: int = 10
@export var grid_height: int = 10
@export var tile_size: float = 1.0
@export var tile_prefab: PackedScene
@export var enemy_path_sprite: Sprite2D
@export var base_tile_sprite: Sprite2D

class Tile:
	var is_walkable: bool = true
	var weight: int = 1
	var tile: Node
	var parent: Tile = null

var grid: Array = []
var main_tower: Tile
var current_waypoints: Array = []

# Called when the node enters the scene tree for the first time
func _ready():
	# Build the grid and set the Main tower in the center
	grid = []
	for x in range(grid_width):
		var row: Array = []
		for y in range(grid_height):
			var new_tile = Tile.new()
			new_tile.is_walkable = true
			new_tile.weight = 1
			new_tile.tile = tile_prefab.instantiate()
			new_tile.tile.position = Vector3(x + 0.5, -y - 0.5, 0)
			add_child(new_tile.tile)  # Add the tile to the scene
			row.append(new_tile)
		grid.append(row)
	
	main_tower = grid[grid_width / 2][grid_height / 2]

func update_tile_state(x: int, y: int, is_walkable: bool, weight: int):
	if x >= 0 and x < grid_width and y >= 0 and y < grid_height:
		grid[x][y].is_walkable = is_walkable
		grid[x][y].weight = weight

func get_grid() -> Array:
	return grid

# Public method for enemies to pull waypoints
func get_waypoints() -> Array:
	return current_waypoints

# Function to calculate tile from world position
func get_tile_from_world_position(position: Vector3) -> Tile:
	var x = int(floor(position.x / tile_size))
	var y = int(floor(-position.y / tile_size))
	return grid[x][y]

# Function to generate the enemy path
func generate_enemy_path():
	reset_enemy_path()
	var start_tile = get_random_edge_tile()
	a_star(start_tile)

func get_random_edge_tile() -> Tile:
	var edge = randi() % 4
	var chosen_tile: Tile = null

	match edge:
		0:
			var x_top = randi() % grid_width
			chosen_tile = grid[x_top][0]
		1:
			var y_right = randi() % grid_height
			chosen_tile = grid[grid_width - 1][y_right]
		2:
			var x_bottom = randi() % grid_width
			chosen_tile = grid[x_bottom][grid_height - 1]
		3:
			var y_left = randi() % grid_height
			chosen_tile = grid[0][y_left]

	return chosen_tile

# A* Pathfinding Algorithm
func get_lowest_f_cost_tile(to_visit: Dictionary) -> Tile:
	var lowest: Tile = null
	var lowest_cost = INF
	for tile in to_visit.keys():
		var cost = to_visit[tile]
		if cost < lowest_cost:
			lowest_cost = cost
			lowest = tile
	return lowest

func get_weight(tile: Tile) -> int:
	var dx = abs(main_tower.tile.position.x - tile.tile.position.x)
	var dy = abs(main_tower.tile.position.y - tile.tile.position.y)
	return tile.weight + int(dx) + int(dy)

# Function to get the neighbors of a tile
func get_neighbors(tile: Tile) -> Array:
	var neighbors = []
	var x = int(floor(tile.tile.position.x / tile_size))
	var y = int(floor(-tile.tile.position.y / tile_size))
	var directions = [
		Vector2(0, 1), Vector2(0, -1), Vector2(1, 0), Vector2(-1, 0)
	]
	
	for direction in directions:
		var neighbor_x = x + direction.x
		var neighbor_y = y + direction.y
		if neighbor_x >= 0 and neighbor_x < grid_width and neighbor_y >= 0 and neighbor_y < grid_height:
			var neighbor_tile = grid[neighbor_x][neighbor_y]
			if neighbor_tile.is_walkable:
				neighbors.append(neighbor_tile)

	return neighbors

# A* implementation
func a_star(start_tile: Tile):
	for row in grid:
		for tile in row:
			tile.parent = null
	var to_visit = Dictionary()
	var visited = []
	var path_tiles = []

	to_visit[start_tile] = 0

	while to_visit.size() > 0:
		var current_tile = get_lowest_f_cost_tile(to_visit)
		var cost = to_visit[current_tile]
		to_visit.erase(current_tile)
		visited.append(current_tile)
		
		if current_tile.tile.position == main_tower.tile.position:
			while current_tile.parent != null:
				path_tiles.append(current_tile)
				change_tile_type(current_tile, enemy_path_sprite)
				current_tile = current_tile.parent
			
			while path_tiles.size() > 0:
				var path_tile = path_tiles.pop_back()
				path_tile.is_walkable = false
				current_waypoints.append(path_tile.tile.position)
			return

		for neighbor in get_neighbors(current_tile):
			if not neighbor.is_walkable or visited.has(neighbor):
				continue

			var new_cost_to_neighbor = cost + get_weight(neighbor)
			if not to_visit.has(neighbor) or new_cost_to_neighbor < to_visit[neighbor]:
				neighbor.parent = current_tile
				to_visit[neighbor] = new_cost_to_neighbor

# Reset the enemy path visualization
func reset_enemy_path():
	for row in grid:
		for tile in row:
			var renderer = tile.tile.get_node("Sprite")
			if renderer.sprite == enemy_path_sprite:
				renderer.sprite = base_tile_sprite

	for tile_position in current_waypoints:
		get_tile_from_world_position(tile_position).is_walkable = true
	current_waypoints.clear()

# Helper function to change the tile type (for debugging A* visualization)
func change_tile_type(tile: Tile, tile_type: Sprite2D):
	if tile.tile:
		var renderer = tile.tile.get_node("Sprite")
		if renderer:
			renderer.sprite = tile_type
