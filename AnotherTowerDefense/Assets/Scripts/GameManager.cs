using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public waypointManager waypointManager;            // Reference to the GridManager for pathfinding and tile data
    public EnemyManager enemyManager;          // Reference to the EnemySpawner to handle waves of enemies
    public TowerManager towerManager;          // Reference to the TowerManager to handle tower placement and upgrades
    public int waveNumber = 1;                 // Tracks the current wave number

    private List<Tile> currentPath;            // Stores the current path for enemy movement

    // Game phases
    public enum GamePhase { GeneratePath, BuildPhase, WavePhase }
    public GamePhase currentPhase;

    private void Start() {
        currentPhase = GamePhase.GeneratePath;
        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop() {
        while (true) {
            switch (currentPhase) {
                case GamePhase.GeneratePath:
                    yield return StartCoroutine(GeneratePath());
                    currentPhase = GamePhase.BuildPhase;
                    break;

                case GamePhase.BuildPhase:
                    yield return StartCoroutine(BuildPhase());
                    currentPhase = GamePhase.WavePhase;
                    break;

                case GamePhase.WavePhase:
                    yield return StartCoroutine(SpawnWave());
                    waveNumber++;
                    currentPhase = GamePhase.GeneratePath;
                    break;
            }
            yield return null;
        }
    }

    private IEnumerator GeneratePath() {
        // Generate the enemy path using A* (handled by the GridManager)
        currentPath = gridManager.GenerateEnemyPath();

        // Visualize the path if needed, for player information
        gridManager.DisplayPath(currentPath);

        yield return null;  // Path generation happens instantly in this example
    }

    private IEnumerator BuildPhase() {
        // Allow the player to place towers in valid locations
        towerManager.StartPlacementMode();

        // Wait for player confirmation (e.g., a button press to end the build phase)
        while (!towerManager.IsPlacementConfirmed()) {
            yield return null;
        }

        // End the build phase
        towerManager.EndPlacementMode();

        yield return null;
    }

    private IEnumerator SpawnWave() {
        // Trigger enemy spawning for the current wave
        enemySpawner.SpawnWave(currentPath, waveNumber);

        // Wait until all enemies are defeated or reach the endpoint
        while (enemySpawner.AreEnemiesAlive()) {
            yield return null;
        }

        // Wave has ended
        yield return null;
    }
}