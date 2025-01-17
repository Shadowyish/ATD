extends Area2D

# Exported variables for Tower properties
@export var fire_rate : float = 1.0  # Number of seconds between attacks
@export var damage : int = 10        # Damage per attack
@export var range : float = 5.0      # Range within which the tower can attack
@export var max_health : int = 100

var fire_countdown : float = 0.0     # Time remaining before next attack
var target : Node = null             # The current target (enemy)
var enemies_in_range : Array = []    # List of enemies within range
var current_health : int

# Called when the node enters the scene
func _ready():
	current_health = max_health
	# Create a collision shape to represent the tower's range (circle)
	var collision_shape = CircleShape2D.new()
	collision_shape.radius = range
	var area = Area2D.new()
	area.shape = collision_shape
	add_child(area)

# Called every frame
func _process(delta: float) -> void:
	# Update the target and check for attack conditions
	update_target()

	if target == null:
		return

	# Attack if fire rate countdown allows
	if fire_countdown <= 0.0:
		shoot()
		fire_countdown = fire_rate

	fire_countdown -= delta

# Find and set the target based on enemies in range
# TODO: Add Targeting Types
func update_target() -> void:
	var shortest_distance : float = INF
	var nearest_enemy : Node = null

	for enemy in enemies_in_range:
		var distance_to_enemy = global_position.distance_to(enemy.global_position)
		if distance_to_enemy < shortest_distance:
			shortest_distance = distance_to_enemy
			nearest_enemy = enemy

	if nearest_enemy != null and shortest_distance <= range:
		target = nearest_enemy
	else:
		target = null

func shoot() -> void:
	#Add Projectiles
	if target:
		target.take_damage(damage)

func _on_area_entered(area: Area2D) -> void:
	if area.is_in_group("enemy"):
		enemies_in_range.append(area)

func _on_area_exited(area: Area2D) -> void:
	if area.is_in_group("enemy"):
		enemies_in_range.erase(area)
#TODO: Creat Signals for MT for enemy hit and Heals

func _on_tower_hitbox_area_entered(area: Area2D) -> void:
	var enemy = get_parent()
	if enemy and enemy.has_method("Health"):
		take_damage(enemy.Health)

func take_damage(damage_amount : int):
	current_health -= damage_amount
	print("Tower took damage. Current health: %d" % current_health)
	if current_health <= 0:
		destroy_tower()

func heal_tower(heal_amount : int):
	current_health = min(current_health + heal_amount, max_health)
	print("Tower healed. Current health: %d" % current_health)

func get_current_health() -> int:
	return current_health

func destroy_tower():
	queue_free()
	#TODO: Send Signal to Deploy Game Over Screen, passing the logic back to Game Controller

# Draw range for visualization in the editor
func _draw() -> void:
	draw_circle(Vector2.ZERO, range, Color(1, 0, 0))  # Red circle for the range
