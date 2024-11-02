using UnityEngine;

public class BuildPhaseManager : MonoBehaviour {
    [SerializeField] private List<GameObject> towerPrefabs; 
    private GameObject selectedTowerPrefab;
    private bool isPlacementActive = false;
    private bool isPlacementConfirmed = false;
    private GridWaypointManager gridManager;
    void Start() {
    }

    public void StartPlacementMode(GridWaypointManager gridManager) {
        isPlacementActive = true;
        isPlacementConfirmed = false;
        this.gridManager = gridManager;
    }

    void Update() {
        if (isPlacementActive) {
            HandleTowerPlacement();
            if (Input.GetKeyDown(KeyCode.Space)) {
                ConfirmPlacement();
            }
        }
    }

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

            if (targetTile != null && targetTile.IsWalkable) {
                PlaceTower(targetTile);
            }
        }
    }

    private void PlaceTower(Tile tile) {
        // Instantiate tower prefab and set position
        Vector3 position = tile.centerPosition;
        GameObject newTower = Instantiate(selectedtowerPrefab, position, Quaternion.identity);
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