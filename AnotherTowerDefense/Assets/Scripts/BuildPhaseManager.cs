using UnityEngine;
using System.Collections.Generic;

public class BuildPhaseManager : MonoBehaviour {
    [SerializeField] private List<GameObject> towerPrefabs;
    [SerializeField] private GameObject towerSelectionPanel;
    [SerializeField] private List<UnityEngine.UI.Button> towerButtons;
    [SerializeField] private Sprite towerBuilt;
    [SerializeField] private TMPro.TextMeshProUGUI placementInstructions;
    private GameObject[] selectedTowerPrefabs = new GameObject[3];
    private GameObject selectedTowerPrefab;
    private bool isPlacementActive = false;
    private bool isPlacementConfirmed = false;
    private bool isTowerSelected = false;
    private GridWaypointManager gridManager;

    public void StartPlacementMode(GridWaypointManager gridManager) {
        isPlacementActive = true;
        isPlacementConfirmed = false;
        this.gridManager = gridManager;
        SelectTower();
        placementInstructions.gameObject.SetActive(true);
    }
    void Update() {
        if (isPlacementActive&&isTowerSelected) {
            HandleTowerPlacement();
        }
    }
    //Load UI screen for Tower Selection
    private void SelectTower() {      
        List<int> selectedIndices = new List<int>();
        while (selectedIndices.Count < 3) {
            int randomIndex = Random.Range(0, towerPrefabs.Count);
            
            //if (!selectedIndices.Contains(randomIndex)) {
                selectedIndices.Add(randomIndex);
        }

        int i = 0;
        foreach (int index in selectedIndices) {
            selectedTowerPrefabs[i] = towerPrefabs[index];
            towerButtons[i].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = towerPrefabs[index].name; // Update button text
            towerButtons[i].onClick.RemoveAllListeners();
            int buttonIndex = i; // Cache index to avoid closure issue in C#
            towerButtons[i].onClick.AddListener(() => SelectTowerFromUI(buttonIndex));
            i++;
        }
        towerSelectionPanel.SetActive(true); // Show the selection panel
    }
    private void HandleTowerPlacement() {
        // Check for mouse click
        if (Input.GetMouseButtonDown(0)) {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Tile targetTile = gridManager.GetTileFromWorldPosition(mousePosition);
            if(targetTile != null && targetTile.IsWalkable) {
                SpriteRenderer renderer = targetTile.tile.GetComponent<SpriteRenderer>();
                if (renderer != null) {
                    renderer.sprite = towerBuilt; // Change the sprite of the tile
                }
                PlaceTower(targetTile);
                ConfirmPlacement();
            }
        }
    }
    private void PlaceTower(Tile tile) {
        // Instantiate tower prefab and set position
        Vector3 position = tile.tile.GetComponent<Transform>().position;
        GameObject newTower = Instantiate(selectedTowerPrefab, position, Quaternion.identity);
        // Change weights for tiles in range of tower
        TowerManager tower = newTower.GetComponent<TowerManager>();
        UpdateTileWeights(position, tower.getRange(), tower.getDamage());
        tile.IsWalkable = false; // Mark tile as blocked for pathfinding
    }
    // Call this to confirm the placement phase
    public void ConfirmPlacement() {
        isPlacementConfirmed = true;
        isPlacementActive = false;
        placementInstructions.gameObject.SetActive(false);
    }
    // Accessed by GameManager to check if placement is confirmed
    public bool IsPlacementConfirmed() {
        return isPlacementConfirmed;
    }
    public void SelectTowerFromUI(int towerIndex) {
    selectedTowerPrefab = selectedTowerPrefabs[towerIndex];
    isTowerSelected = true;
    towerSelectionPanel.SetActive(false); // Hide the panel after selection
    }
    //shell function to ensure lock for wave phase
    public void EndPlacementMode() { 
        isPlacementActive = false;
        isTowerSelected = false;
    } 
    private void UpdateTileWeights(Vector3 towerPosition, float range, int weight){
        foreach(Tile tile in gridManager.getGrid()){
            float distance = Vector3.Distance(towerPosition, tile.tile.transform.position);
            if (distance <= range){
                tile.weight += weight;
            }
        }
    }
}