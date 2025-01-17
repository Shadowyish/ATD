extends Node

@export var enemy_prefabs : Array[PackedScene] # List of enemy prefabs
@export var time_between_waves : float = 5.0  # Time between each wave

var current_enemy_list : Array[Node2D] = []   # List of current enemies
var current_wave : int = 0
var total_waves : int = 0
var is_spawning : bool = false
var total_enemies : int = 0
var enemies_per_wave : int = 0
var current_waypoints : Array[Vector2] = []   # List of waypoints for enemy paths

func start_waves(waypoints: Array[Vector2], wave_number: int):
	if not is_spawning:
		current_wave = 0
		total_waves = max(1, wave_number - randi_range(2, wave_number - 1))
		total_enemies = wave_number * 2 + total_waves * total_waves
		enemies_per_wave = int(total_enemies / total_waves)
		total_enemies = enemies_per_wave * total_waves
		is_spawning = true
		current_waypoints = waypoints
		populate_enemy_list(total_enemies)
		spawn_waves()

func stop_waves():
	is_spawning = false

func are_enemies_alive() -> bool:
	for enemy in current_enemy_list:
		if enemy and enemy.is_inside_tree():
			return true
	return false

func spawn_waves():
	if is_spawning:
		for i in range(enemies_per_wave):
			spawn_enemy(i)
			await get_tree().create_timer(1.0).timeout
		await get_tree().create_timer(time_between_waves).timeout # Delay between waves
		current_wave += 1
		if current_wave < total_waves:
			spawn_waves()
		else:
			is_spawning = false

func spawn_enemy(enemy_in_wave: int):
	var index = enemies_per_wave * current_wave + enemy_in_wave
	if index >= current_enemy_list.size():
		push_error("Attempted to spawn enemy at index %d, but list size is %d. Skipping spawn." % [index, current_enemy_list.size()])
		return
	elif current_enemy_list[index] == null:
		return
	var spawn_position = current_waypoints[0]
	var target_position = current_waypoints[1]
	var direction = (target_position - spawn_position).normalized()
	var angle = direction.angle()
	var enemy_instance = current_enemy_list[index].instance()
	enemy_instance.position = spawn_position
	enemy_instance.rotation = angle
	add_child(enemy_instance)
	enemy_instance.waypoints = current_waypoints
	current_enemy_list[index] = enemy_instance

func populate_enemy_list(pop_count: int):
	current_enemy_list.clear()
	for i in range(pop_count):
		var enemy = enemy_prefabs[randi_range(0, enemy_prefabs.size() - 1)]
		current_enemy_list.append(enemy)
