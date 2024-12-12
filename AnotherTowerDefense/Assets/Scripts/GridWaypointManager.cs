using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile {
    public bool IsWalkable; // If the tile can be walked on
    public int weight; // Cost to move onto this tile
    public GameObject tile;
    public Tile parent;
}

// Grid tiles are calculated from the 0,0 of this object, make sure this object is placed on world origin
public class GridWaypointManager : MonoBehaviour {
    public static GridWaypointManager Instance { get; private set; }  // Singleton reference
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float tileSize = 1;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject mainTowerPrefab;
    [SerializeField] private Sprite EnemyPath;
    [SerializeField] private Sprite BaseTile; 
    private List<Vector3> currentWaypoints = new List<Vector3>();
    private Tile[,] grid; // 2D array to represent the grid
    private Tile mainTower;
    
    private void Awake() {
        // Set this instance as the singleton instance
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject); // Prevent multiple instances
        }
    }
    void Start() {
        grid = new Tile[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                grid[x, y] = new Tile {
                    IsWalkable = true,
                    weight = 1,
                    tile = Instantiate(tilePrefab, new Vector3(x + .5f,- y - .5f, 0f), Quaternion.identity, transform)
                };
            }
        }
        mainTower = grid[(int) gridWidth/2, (int)gridHeight/2];
        Instantiate(mainTowerPrefab, mainTower.tile.GetComponent<Transform>().position, Quaternion.identity);
    }

    // Method to update tile state (blocked/unblocked)
    // May expand later if needed
    public void UpdateTileState(int x, int y, bool isWalkable, int weight) {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight) {
            grid[x, y].IsWalkable = isWalkable;
            grid[x, y].weight = weight;
        }
    }
    
    public Tile[,] getGrid(){
        return grid;
    }

    // public method for enemies to pull the waypoints of generated path
    public List<Vector3> GetWaypoints() {
        return currentWaypoints;        
    }

    // Need a fix for this/ double check works TODO::
    public Tile GetTileFromWorldPosition(Vector3 position) {
    	int x = Mathf.FloorToInt((int)position.x/ tileSize);
    	int y = Mathf.FloorToInt(-(int)position.y/ tileSize);
    	return grid[x, y]; 
    }

    public void GenerateEnemyPath(){
        ResetEnemyPath();
        Tile start = GetRandomEdgeTile();
        AStar(start); 
    }

    private Tile GetRandomEdgeTile() {
        // Choose an edge (0 = top, 1 = right, 2 = bottom, 3 = left)
        int edge = UnityEngine.Random.Range(0, 4);
        Tile chosenTile;

        switch (edge) {
            case 0: // Top edge
                int xTop = UnityEngine.Random.Range(0, gridWidth);
                chosenTile = grid[xTop, 0];
                break;
            case 1: // Right edge
                int yRight = UnityEngine.Random.Range(0, gridHeight);
                chosenTile = grid[gridWidth - 1, yRight];
                break;
            case 2: // Bottom edge
                int xBottom = UnityEngine.Random.Range(0, gridWidth);
                chosenTile = grid[xBottom, gridHeight - 1];
                break;
            case 3: // Left edge
                int yLeft = UnityEngine.Random.Range(0, gridHeight);
                chosenTile = grid[0, yLeft];
                break;
            default:
                chosenTile = grid[0, 0]; // Fallback, though shouldn't happen
                break;
        }

        return chosenTile;
    }

    private Tile GetLowestFCostTile(Dictionary<Tile, int> toVisit) {
        Tile lowest = null;
        int lowestCost = int.MaxValue;
        foreach (var kvp in toVisit) {
            if (kvp.Value < lowestCost) {
                lowestCost = kvp.Value;
                lowest = kvp.Key;
            }
        }
        return lowest;
    }

    private int getWeight(Tile tile){
        return tile.weight + (int)Math.Abs((mainTower.tile.GetComponent<Transform>().position.x - tile.tile.GetComponent<Transform>().position.x)) + 
        (int)Math.Abs((-mainTower.tile.GetComponent<Transform>().position.y + tile.tile.GetComponent<Transform>().position.y));
    }

    private List<Tile> GetNeighbors(Tile tile) {
        List<Tile> neighbors = new List<Tile>();
        // Get tile's current grid coordinates
        int x = Mathf.FloorToInt(((int)tile.tile.GetComponent<Transform>().position.x) / tileSize);
        int y = Mathf.FloorToInt((- (int)tile.tile.GetComponent<Transform>().position.y) / tileSize);
        // Define potential neighboring positions (4-directional: up, down, left, right)
        int[,] directions = new int[,] { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } };

        for (int i = 0; i < directions.GetLength(0); i++) {
            int neighborX = x + directions[i, 0];
            int neighborY = y + directions[i, 1];
            // Bounds checking
            if (neighborX >= 0 && neighborX < gridWidth && neighborY >= 0 && neighborY < gridHeight) {
                Tile neighborTile = grid[neighborX, neighborY];
                if (neighborTile.IsWalkable) {  // Only consider walkable tiles
                    neighbors.Add(neighborTile);
                }
            }
        }
        return neighbors;
    }

    private void AStar(Tile startTile) {
        foreach(Tile tile in grid){tile.parent = null;}
    	Dictionary<Tile, int> toVisit = new Dictionary<Tile, int>();
    	HashSet<Tile> visited = new HashSet<Tile>();
        Stack<Tile> pathTiles = new Stack<Tile>(); 
    	
        toVisit[startTile] = 0;

    	while (toVisit.Count > 0) {
            //pull next best tile from non-visited to explore
        	Tile currentTile = GetLowestFCostTile(toVisit);
            int cost = toVisit[currentTile];
            toVisit.Remove(currentTile);
            visited.Add(currentTile);
            if (currentTile.tile.GetComponent<Transform>().position == mainTower.tile.GetComponent<Transform>().position) {
                while(currentTile.parent != null){
                    pathTiles.Push(currentTile);
                    ChangeTileType(currentTile, EnemyPath);
                    currentTile = currentTile.parent;
                }               
                while(pathTiles.Count !=0){
                    Tile pathTile = pathTiles.Pop();
                    pathTile.IsWalkable = false;
                    currentWaypoints.Add(pathTile.tile.GetComponent<Transform>().position);
                }return;
        	}

            //check all the neighbor tiles for nodes to add/update.
            foreach (Tile neighbor in GetNeighbors(currentTile)) {
                if (!neighbor.IsWalkable || visited.Contains(neighbor)) {
                    continue; // Skip if not walkable or already evaluated
                }
                int newCostToNeighbor = cost + getWeight(neighbor);
                //update old weight on unwalked node if better one found
                if (!toVisit.ContainsKey(neighbor)|| newCostToNeighbor < toVisit[neighbor]) {
                    neighbor.parent = currentTile;
                    toVisit[neighbor] = newCostToNeighbor;
                }
            }
        }
    }
    private void ResetEnemyPath(){
        //reset the vizualization
        for(int i = 0; i < gridWidth; i++){
            for(int j = 0; j < gridHeight; j++ ){
                SpriteRenderer renderer = grid[i, j].tile.GetComponent<SpriteRenderer>();
                if(renderer.sprite == EnemyPath){
                    renderer.sprite = BaseTile;
                }
            }    
        }
        //reset the path lock and clear waypoints
        foreach(Vector3 tilePosition in currentWaypoints){
            GetTileFromWorldPosition(tilePosition).IsWalkable = true;
        }
        currentWaypoints.Clear();    
    }
    //Used for debugging the A*
    private void ChangeTileType(Tile tile, Sprite tileType) {
        if (tile.tile != null) {
            SpriteRenderer renderer = tile.tile.GetComponent<SpriteRenderer>();
            if (renderer != null) {
                renderer.sprite = tileType; // Change the sprite of the tile
            }
        }
    }
}