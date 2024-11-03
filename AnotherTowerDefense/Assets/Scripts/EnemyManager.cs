using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour {
    [SerializeField] private float speed = 1f; // Movement speed of the enemy
    [SerializeField] private int health = 10; // Health of the enemy
    private Vector3 target; // The target position index for the enemy to move toward
    private int targetIndex = 1;
    private bool isAlive = true; // To check if the enemy is alive
    private List<Vector3> waypoints;

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
            // Calculate the rotation to look at the target
            Quaternion targetRotation = Quaternion.LookRotation(target - transform.position);
        
            // Smoothly rotate towards the target rotation and move
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);            
        } else {
            ReachTarget();
        }
    }

    private void ReachTarget() {
        if(targetIndex + 1 >= waypoints.Count){
            // Deal damage to main tower = to remaining health
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