using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField] private GridWaypointManager waypointManager;
    [SerializeField] private EnemyWaveManager enemySpawner;
    [SerializeField] private BuildPhaseManager towerManager;
    private int waveNumber = 1;   // Tracks the current wave number
    
    public enum GamePhase { GeneratePath, BuildPhase, WavePhase }
    public GamePhase currentPhase;


    //https://docs.unity3d.com/6000.0/Documentation/Manual/coroutines.html
    //https://docs.unity3d.com/ScriptReference/Coroutine.html
    private void Start() {
        currentPhase = GamePhase.GeneratePath;
        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop() {
        yield return new WaitForSeconds(3f);
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
                    currentPhase = GamePhase.GeneratePath;
                    waveNumber++;
                    break;
            }
            yield return null;
        }
    }

    private IEnumerator GeneratePath() {
        // Generate the enemy path using A* (handled by the waypointManager)
        waypointManager.GenerateEnemyPath();
        yield return null;
    }

    private IEnumerator BuildPhase() {
        // Allow the player to place towers in valid locations
        towerManager.StartPlacementMode(waypointManager);

        // Wait for player confirmation (e.g., a button press to end the build phase)
        while (!towerManager.IsPlacementConfirmed()) {
            yield return null;
        }
        towerManager.EndPlacementMode();

        yield return null;
    }

    private IEnumerator SpawnWave() {
        // Trigger enemy spawning for the current wave
        enemySpawner.StartWaves(waypointManager.GetWaypoints(), waveNumber);

        // Wait until all enemies are defeated or reach the endpoint
        while (enemySpawner.IsEnemiesAlive()) {
            yield return null;
        }
        //Stop spawner coroutine
        enemySpawner.StopWaves();
        yield return null;
    }
}