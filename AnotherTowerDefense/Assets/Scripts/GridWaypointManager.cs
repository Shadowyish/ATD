using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile {
    public bool IsWalkable; // If the tile can be walked on
    public int weight; // Cost to move onto this tile
    public GameObject tile;
}

// Grid tiles are calculated from the 0,0 of this object, make sure this object is placed on world origin
public class GridWaypointManager : MonoBehaviour {
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float tileSize = 1;
    [SerializeField] private GameObject tilePrefab;
    private List<Vector3> currentWaypoints;
    private Tile[,] grid; // 2D array to represent the grid
    private Tile mainTower;

    void Start() {
        grid = new Tile[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                grid[x, y] = new Tile {
                    IsWalkable = true,
                    weight = 1,
                    tile = Instantiate(tilePrefab, new Vector3(x - .5f, 0 - y - .5f, 0f), Quaternion.identity, transform)
                };
            }
        }
        mainTower = grid[(int) gridWidth/2, (int)gridHeight/2];
    }

    // Method to update tile state (blocked/unblocked)
    // May expand later if needed
    public void UpdateTileState(int x, int y, bool isWalkable) {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight) {
            grid[x, y].IsWalkable = isWalkable;
        }
    }

    // public method for enemies to pull the waypoints of generated path
    public List<Vector3> GetWaypoints() {
        return currentWaypoints;        
    }

    // Need a fix for this/ double check works TODO::
    public Tile GetTileFromWorldPosition(Vector3 position) {
    	int x = Mathf.FloorToInt(position.x / tileSize);
    	int y = Mathf.FloorToInt(-position.y / tileSize);
    	return grid[x, y]; 
    }

    public void GenerateEnemyPath(){
        Tile start = GetRandomEdgeTile();
        AStar(start);
        //TODO: Highlight Path  
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

    private Tile GetLowestFCostTile(List<Tile> list) {
        Tile lowest = list[0];
        foreach (Tile tile in list) {
            
            if (getWeight(tile) < getWeight(lowest)) {
                lowest = tile;
            }
        }
        return lowest;
    }

    private int getWeight(Tile tile){
        return tile.weight + (int)Math.Abs((mainTower.tile.GetComponent<Transform>().position.x - tile.tile.GetComponent<Transform>().position.x) + 
        (mainTower.tile.GetComponent<Transform>().position.y - tile.tile.GetComponent<Transform>().position.y));
    }

    private List<Tile> GetNeighbors(Tile tile) {
        List<Tile> neighbors = new List<Tile>();
        // Get tile's current grid coordinates
        int x = Mathf.FloorToInt((tile.tile.GetComponent<Transform>().position.x) / tileSize);
        int y = Mathf.FloorToInt((tile.tile.GetComponent<Transform>().position.y) / tileSize);

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
    	List<Tile> toVisit = new List<Tile>();
    	HashSet<Tile> visited = new HashSet<Tile>();
        Queue<Tile> pathTiles = new Queue<Tile>(); 
    	
        toVisit.Add(startTile);

    	while (toVisit.Count > 0) {
            //pull next best tile from non-visited to explore
        	Tile currentTile = GetLowestFCostTile(toVisit);

        	if (currentTile == mainTower) {
            	currentWaypoints.Clear();                
                while(pathTiles.Count !=0){
                    //Retrace path: Might need to change, think this returns reversed path
                    currentWaypoints.Add(pathTiles.Dequeue().tile.GetComponent<Transform>().position);
                }
        	}

            toVisit.Remove(currentTile);
            visited.Add(currentTile);
            pathTiles.Enqueue(currentTile);

            //check all the neighbor tiles for nodes to add/update.
            foreach (Tile neighbor in GetNeighbors(currentTile)) {
                if (!neighbor.IsWalkable || visited.Contains(neighbor)) {
                    continue; // Skip if not walkable or already evaluated
                }
                
                int newCostToNeighbor = getWeight(currentTile) + getWeight(neighbor);
                //update old weight on unwalked node if better one found
                if (newCostToNeighbor < getWeight(neighbor) || !toVisit.Contains(neighbor)) {
                    neighbor.weight = newCostToNeighbor;
                    if (!toVisit.Contains(neighbor)) {
                        toVisit.Add(neighbor);
                    }
                }
            }
        }
    }
}