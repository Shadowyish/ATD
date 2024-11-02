using System.Collections;
using UnityEngine;

public class Tile {
    public bool IsWalkable; // If the tile can be walked on
    public int weight; // Cost to move onto this tile
    public Vector3 centerPosition;
}
public class GridWaypointManager : MonoBehaviour {
    [SerializeField] private int gridWidth;
    [SerializeField] private int gridHeight;
    [SerializeField] private float tileSize;
    private List<Vector3> currentWaypoints;
    private Tile[,] grid; // 2D array to represent the grid
    private Tile mainTower;

    void Start() {
	    gridWidth = 10;
	    gridHeight = 10;
	    tileSize = 1;
        grid = new Tile[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                grid[x, y] = new Tile {
                    IsWalkable = true,
                    weight = 1,
                    centerPosition = newVector3() 
                }
            }
        }
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
    private Tile GetTileFromWorldPosition(Vector3 position) {
    	int x = Mathf.FloorToInt(position.x / tileSize);
    	int y = Mathf.FloorToInt(position.y / tileSize);
    	return grid[x, y]; 
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
        return tile.weight + abs((main_tower.centerPosition.x - tile.centerPosition.x) + (main_tower.centerPosition.y - tile.centerPosition.y));
    }
    private List<Tile> GetNeighbors(Tile tile) {
        List<Tile> neighbors = new List<Tile>();
        // Get tile's current grid coordinates
        int x = Mathf.FloorToInt((tile.centerPosition.x) / tileSize);
        int y = Mathf.FloorToInt((tile.centerPosition.y) / tileSize);

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
    private AStar(Vector3 start, Vector3 end) {
    	List<Tile> toVisit = new List<Tile>();
    	HashSet<Tile> visited = new HashSet<Tile>();
        Stack<Tile> pathTiles = new Stack<Tile>();
    
    	// Convert world position to grid coordinates
   	    Tile startTile = GetTileFromWorldPosition(start);
    
    	toVisit.Add(startTile);

    	while (toVisit.Count > 0) {
        	Tile currentTile = GetLowestFCostTile(toVisit, endTile);

        	if (currentTile == mainTower) {
            	currentWaypoints.Clear();                
                while(pathTiles.Count !=0){
                    currentWaypoints.Add(pathTiles.pop().centerPosition);
                }
        	}

            toVisit.Remove(currentTile);
            visited.Add(currentTile);
            pathTiles.push(currentTile);

            //check all the neighbor tiles for nodes to add/update.
            foreach (Tile neighbor in GetNeighbors(currentTile)) {
                if (!neighbor.IsWalkable || visited.Contains(neighbor)) {
                    continue; // Skip if not walkable or already evaluated
                }
                
                int newCostToNeighbor = getWeight(currentTile) + getWeight(neighbor.Weight);
                //update old weight on unwalked node if better one found
                if (newCostToNeighbor < getWeight(neighbor) || !toVisit.Contains(neighbor)) {
                    neighbor.Weight = newCostToNeighbor;
                    if (!toVisit.Contains(neighbor)) {
                        toVisit.Add(neighbor);
                    }
                }
            }
        }
    }
}