using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour {
    [SerializeField] private float speed = 1f; // Movement speed of the enemy
    [SerializeField] private int health = 10; // Health of the enemy
    private Vector3 target; // The target position index for the enemy to move toward
    private int targetIndex = 1;
    private bool isAlive = true; // To check if the enemy is alive
    private List<Vector3> waypoints;

    void Awake(){
        waypoints = GridWaypointManager.Instance.GetWaypoints();
        target = waypoints[1];
    }
    void Update() {
        if (isAlive) {
            MoveTowardsTarget();
        }else Die();
    }

    public void SetTarget(Vector3 targetPosition) {
        target = targetPosition;
    }

    public void TakeDamage(int amount) {
        health -= amount;
        if (health <= 0) {
            Die();
        }
    }

    private void MoveTowardsTarget() {
        if (Vector3.Distance(transform.position, target) > 0.1f) {
            Vector3 direction = (target - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; 
            // Apply rotation only on the z-axis
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);
            // Move towards the target position
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);            
        } else {
            ReachTarget();
        }
    }

    private void ReachTarget() {
        if(targetIndex + 1 >= waypoints.Count){
            if(MainTowerManager.Instance != null){
                //inflict damage to main tower equal to remaining health
                MainTowerManager.Instance.TakeDamage(health);
            }
            Destroy(gameObject); // Destroy the enemy object upon reaching the target
        }else{
            targetIndex++;
            target = waypoints[targetIndex];
        }
    }

    private void Die() {
        // Todo?? Add death animations/effects here
        Destroy(gameObject); // Destroy the enemy object
    }
}