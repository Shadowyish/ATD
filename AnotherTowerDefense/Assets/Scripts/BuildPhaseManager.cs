using UnityEngine;
using System.Collections.Generic;

public class BuildPhaseManager : MonoBehaviour {
    [SerializeField] private List<GameObject> towerPrefabs; 
    private GameObject selectedTowerPrefab;
    private bool isPlacementActive = false;
    private bool isPlacementConfirmed = false;
    private GridWaypointManager gridManager;

    //Load UI and Placement Visualizations
    public void StartPlacementMode(GridWaypointManager gridManager) {
        isPlacementActive = true;
        isPlacementConfirmed = false;
        this.gridManager = gridManager;
        SelectTower();
    }

    void Update() {
        if (isPlacementActive) {
            HandleTowerPlacement();
        }
    }

    //Load UI screen for Tower Selection
    private void SelectTower() {
        // For now, pick a random tower
        int randomIndex = Random.Range(0, towerPrefabs.Count);
        selectedTowerPrefab = towerPrefabs[randomIndex];
        Debug.Log("Selected Tower: " + selectedTowerPrefab.name);
    }

    private void HandleTowerPlacement() {
        // Check for mouse click
        if (Input.GetMouseButtonDown(0)) {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Tile targetTile = gridManager.GetTileFromWorldPosition(mousePosition);
            //TODO: Vizualize Space Selected
            if(targetTile != null && targetTile.IsWalkable) {
                PlaceTower(targetTile);
                ConfirmPlacement();
            }
        }
    }

    private void PlaceTower(Tile tile) {
        // Instantiate tower prefab and set position
        Vector3 position = tile.tile.GetComponent<Transform>().position;
        GameObject newTower = Instantiate(selectedTowerPrefab, position, Quaternion.identity);
        tile.IsWalkable = false; // Mark tile as blocked for pathfinding
    }

    // Call this to confirm the placement phase
    public void ConfirmPlacement() {
        isPlacementConfirmed = true;
        isPlacementActive = false;
    }

    // Accessed by GameManager to check if placement is confirmed
    public bool IsPlacementConfirmed() {
        return isPlacementConfirmed;
    }

    //shell function will use to end placement UI to trasition to wave UI
    public void EndPlacementMode() {
        isPlacementActive = false;
    }
}