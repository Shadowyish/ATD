using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private float fireRate = 1f;// Number of seconds between attacks
    [SerializeField] private int damage = 10;// Damage per attack
    [SerializeField] private float range = 5f;// Range within which the tower can attack
    private float fireCountdown = 0f;// Time remaining before next attack
    private Enemy target;
    private List<Enemy> enemiesInRange = new List<Enemy>();

    void Start(){
    }

    void Update(){
        // Check for targets in range
        UpdateTarget();

        if (target == null)
            return;
        // Attack if fire rate countdown allows
        if (fireCountdown <= 0f){
            Shoot();
            fireCountdown = fireRate;
        }
        fireCountdown -= Time.deltaTime;
    }

    // Find and set the target based on enemies in range
    void UpdateTarget(){
        float shortestDistance = Mathf.Infinity;
        Enemy nearestEnemy = null;

        foreach (Enemy enemy in enemiesInRange){
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance){
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= range){
            target = nearestEnemy.transform;
        }
        else{
            target = null;
        }
    }

    void Shoot(){
        target.dealDamage(damage);    
    }

    // Detect enemies entering range
    private void OnTriggerEnter(Collider other){
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null){
            enemiesInRange.Add(enemy);
        }
    }

    // Remove enemies that exit range
    private void OnTriggerExit(Collider other){
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null){
            enemiesInRange.Remove(enemy);
        }
    }

    // Draw range in the editor for visualization
    private void OnDrawGizmosSelected(){
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}