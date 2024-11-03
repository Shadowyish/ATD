using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveManager : MonoBehaviour{   
    [SerializeField] private List<GameObject> enemyPrefabs; 
    [SerializeField] private float timeBetweenWaves = 5f; // Time between each wave

    private List<GameObject> currentEnemyList;
    private int currentWave;
    private int totalWaves; // tracks max waves for this stage
    private bool isSpawning = false;
    private int totalEnemies;
    private int enemiesPerWave;// max number of enemies per wave
    private List<Vector3> currentWaypoints;

    public void StartWaves(List<Vector3> waypoints, int waveNumber){
        if (!isSpawning){
            currentWave = 0;
            totalWaves = 1 + waveNumber; // x = y + 1
            totalEnemies = (int)Math.Ceiling((float)(waveNumber * waveNumber /3 + totalWaves)); // 1/3y^2 + (y+1)
            enemiesPerWave = Mathf.FloorToInt(totalEnemies/totalWaves); // (1/3y^2 + (y+1)) / (y+1)
            isSpawning = true; // Prevent multiple calls
            currentWaypoints = waypoints;
            PopulateEnemyList(totalEnemies);
            StartCoroutine(SpawnWaves());
        }
    }

    // Call this method from the Game Manager to stop the waves (if needed)
    public void StopWaves(){
        isSpawning = false;
        StopCoroutine(SpawnWaves());
    }
    public bool IsEnemiesAlive(){
        foreach(GameObject enemy in currentEnemyList){
            if(enemy != null){
                return true;
            }
        }
        return false;
    }
    private IEnumerator SpawnWaves(){
        while (isSpawning){
            currentWave++;
            for (int i = 0; i < enemiesPerWave; i++){
                SpawnEnemy(i);
                yield return new WaitForSeconds(1f); // Delay between enemy spawns
            }
            yield return new WaitForSeconds(timeBetweenWaves); // Delay before the next wave
            if(currentWave >= totalWaves)isSpawning = false;
        }
    }

    private void SpawnEnemy(int enemyInWave){
        Vector3 spawnPosition = currentWaypoints[0];
        Vector3 targetPosition = currentWaypoints[1];
        // Calculate the rotation to look at the second waypoint
        Quaternion rotation = Quaternion.LookRotation(targetPosition - spawnPosition);
        // Instantiate the enemy at the first waypoint and set its rotation
        // Take the instiated object and replace the prefab with it
        currentEnemyList[enemiesPerWave * currentWave + enemyInWave] = Instantiate(
            currentEnemyList[enemiesPerWave * currentWave + enemyInWave],
            spawnPosition,
            rotation);
    }

    private void PopulateEnemyList(int totalEnemies) {
        currentEnemyList.Clear(); // Clear any existing enemies
        for (int i = 0; i < totalEnemies; i++) {
            // Select a random enemy prefab
            GameObject enemy = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Count)]; 
            //add the prefab to the list       
            currentEnemyList.Add(enemy);
        }
    }
}