extends Node
@export var main_tower_info_text : Label
@export var game_phase_info_text : Label
@export var main_tower_prefab: PackedScene
var main_tower: Node
var build_manager: Node
var waypoint_manager : Node
var enemy_spawner : Node
var game_running = false
var wave_number : int = 1  # Tracks the current wave number
enum GamePhase { GENERATE_PATH, BUILD_PHASE, WAVE_PHASE }
var current_phase : GamePhase = GamePhase.GENERATE_PATH

func _process(delta):
	# Update UI texts each frame
	if game_running:
		if main_tower:
			main_tower_info_text.text = "Main Tower HP: %d/100" % main_tower.instance.get_current_health()
			game_phase_info_text.text = str(current_phase)
		else:
			game_running = false
			# TODO: GameOver, Display Stats, and pass back to Main Menu

func start_game_loop():
	# TODO: Start up variables
	var main_tower = main_tower_prefab.instantiate()
	main_tower.position = waypoint_manager.main_tower.tile.position
	add_child(main_tower)
	await get_tree().create_timer(3.0).timeout
	game_running = true
	while game_running:
		match current_phase:
			GamePhase.GENERATE_PATH:
				await generate_path()
				current_phase = GamePhase.BUILD_PHASE
			GamePhase.BUILD_PHASE:
				await build_phase()
				current_phase = GamePhase.WAVE_PHASE
			GamePhase.WAVE_PHASE:
				await spawn_wave()
				current_phase = GamePhase.GENERATE_PATH
				wave_number += 1

# Generate path using the waypoint manager
func generate_path():
	waypoint_manager.generate_enemy_path()

# Build phase where the player places towers
func build_phase():
	build_manager.start_placement_mode(waypoint_manager)
	# Wait for player confirmation (a button press to end the build phase)
	while not build_manager.check_confirmed():
		pass
	build_manager.end_placement_mode()

# Spawn wave and wait until enemies are defeated or reach the endpoint
func spawn_wave():
	enemy_spawner.start_waves(waypoint_manager.get_waypoints(), wave_number)
	
	# Wait until all enemies are defeated or still alive
	while enemy_spawner.is_enemies_alive():
		pass
	# Stop spawner coroutine
	enemy_spawner.stop_waves()
