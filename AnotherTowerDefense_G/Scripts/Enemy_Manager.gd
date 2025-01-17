extends CharacterBody2D

@export var speed : float = 1.0 # Movement speed of the enemy
@export var health : int = 10  # Health of the enemy
var target : Vector2            # Current target position
var target_index : int = 1      # Target waypoint index
var is_alive : bool = true      # Whether the enemy is alive
var waypoints : Array[Vector2]  # List of waypoints
signal enemy_hit

func _process(delta):
	if target == null:
		if waypoints == null:
			return
		else: target = waypoints[1]
	if is_alive:
		move_towards_target(delta)
	else:
		die()

# Set the target position directly
func set_target(target_position: Vector2):
	target = target_position

# Take damage and check if the enemy should die
func take_damage(amount: int):
	health -= amount
	if health <= 0:
		die()

# Move towards the target position
func move_towards_target(delta):
	if position.distance_to(target) > 0.1:
		var direction = (target - position).normalized()
		var angle = direction.angle()
		rotation = lerp_angle(rotation, angle, speed * delta)
		velocity = direction * speed
		move_and_slide()
	else:
		reach_target()

# Handle reaching the current target
func reach_target():
	if target_index + 1 >= waypoints.size():
		# TODO: Send Signal to Main Tower to deal Damage
		emit_signal("enemy_hit")
		queue_free() # Destroy the enemy
	else:
		target_index += 1
		target = waypoints[target_index]

# Handle enemy death
func die():
	# TODO: Add death animations/effects here
	queue_free() # Destroy the enemy
