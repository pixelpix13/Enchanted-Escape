using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeBuilder : MonoBehaviour
{
    #region Variables:
    // ------------------------------------------------------
    // User defined variables - set in editor:
    // ------------------------------------------------------
    [Header("Maze generation values:")]
    [Tooltip("How many cells tall is the maze. MUST be an even number. " +
        "If number is odd, it will be reduced by 1.\n\n" +
        "Minimum value of 4.")]
    public int mazeHeight;
    [Tooltip("How many cells wide is the maze. Must be an even number. " +
        "If number is odd, it will be reduced by 1.\n\n" +
        "Minimum value of 4.")]
    public int mazeWidth;

    [Header("Maze object variables:")]
    [Tooltip("Tile prefab object.")]
    [SerializeField]
    private GameObject tilePrefab;

    [Tooltip("If you want to disable the main sprite so the tile has no background, set to TRUE. This will create a maze with only walls.")]
    public bool hideTileSprite;

    // ------------------------------------------------------
    // System defined variables - You don't need to touch these:
    // ------------------------------------------------------

    // Variable to store size of centre room. Hard coded to be 2.
    private int centerSize = 2;

    // Dictionary to hold and locate all tiles in maze.
    private Dictionary<Vector2, Tile> allTiles = new Dictionary<Vector2, Tile>();
    // List to hold unvisited tiles.
    private List<Tile> unvisitedTiles = new List<Tile>();
    // List to store 'stack' tiles, tiles being checked during generation.
    private List<Tile> stack = new List<Tile>();

    // Array will hold 4 centre room tiles, from 0 -> 3 these are:
    // Top left (0), top right (1), bottom left (2), bottom right (3).
    private Tile[] centerTiles = new Tile[4];

    // Tile variables to hold current and checking Tiles.
    private Tile currentTile;
    private Tile checkTile;

    // Array of all possible neighbour positions.
    private Vector2[] neighbourPositions = new Vector2[] { new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, -1) };

    // Size of the tiles, used to determine how far apart to place tiles during generation.
    private float tileSize;

    private GameObject mazeParent;
    #endregion

    private void Start()
    {
        GenerateMaze(mazeHeight, mazeWidth);
    }

    private void GenerateMaze(int height, int width)
    {
        if (mazeParent != null) DestroyMaze();

        mazeHeight = height;
        mazeWidth = width;
        CreateLayout();
    }

    // Creates the grid of tiles.
    public void CreateLayout()
    {
        InitValues();

        // Set starting point, set spawn point to start.
        Vector2 startPos = new Vector2(-(tileSize * (mazeWidth / 2)) + (tileSize / 2), -(tileSize * (mazeHeight / 2)) + (tileSize / 2));
        Vector2 spawnPos = startPos;

        for (int x = 1; x <= mazeWidth; x++)
        {
            for (int y = 1; y <= mazeHeight; y++)
            {
                GenerateTile(spawnPos, new Vector2(x, y));

                // Increase spawnPos y.
                spawnPos.y += tileSize;
            }

            // Reset spawnPos y and increase spawnPos x.
            spawnPos.y = startPos.y;
            spawnPos.x += tileSize;
        }

        CreateCenter();
        RunAlgorithm();
        MakeExit();
    }

    // This is where the fun stuff happens.
    public void RunAlgorithm()
    {
        // Get start tile, make it visited (i.e. remove from unvisited list).
        unvisitedTiles.Remove(currentTile);

        // While we have unvisited tiles.
        while (unvisitedTiles.Count > 0)
        {
            List<Tile> unvisitedNeighbours = GetUnvisitedNeighbours(currentTile);
            if (unvisitedNeighbours.Count > 0)
            {
                // Get a random unvisited neighbour.
                checkTile = unvisitedNeighbours[Random.Range(0, unvisitedNeighbours.Count)];
                // Add current tile to stack.
                stack.Add(currentTile);
                // Compare and remove walls.
                CompareWalls(currentTile, checkTile);
                // Make currentTile the neighbour tile.
                currentTile = checkTile;
                // Mark new current tile as visited.
                unvisitedTiles.Remove(currentTile);
            }
            else if (stack.Count > 0)
            {
                // Make current tile the most recently added Tile from the stack.
                currentTile = stack[stack.Count - 1];
                // Remove it from stack.
                stack.Remove(currentTile);
            }
        }
    }

    public void MakeExit()
    {
        // Create and populate list of all possible edge tiles.
        List<Tile> edgeTiles = new List<Tile>();

        foreach (KeyValuePair<Vector2, Tile> tile in allTiles)
        {
            if (tile.Key.x == 0 || tile.Key.x == mazeWidth || tile.Key.y == 0 || tile.Key.y == mazeHeight)
            {
                edgeTiles.Add(tile.Value);
            }
        }

        // Get edge tile randomly from list.
        Tile newTile = edgeTiles[Random.Range(0, edgeTiles.Count)];

        // Remove appropriate wall for chosen edge tile.
        if (newTile.gridPos.x == 0) RemoveWall(newTile.tScript, 1);
        else if (newTile.gridPos.x == mazeWidth) RemoveWall(newTile.tScript, 2);
        else if (newTile.gridPos.y == mazeHeight) RemoveWall(newTile.tScript, 3);
        else RemoveWall(newTile.tScript, 4);

        Debug.Log("Maze generation finished.");
    }

    public List<Tile> GetUnvisitedNeighbours(Tile curTile)
    {
        // Create a list to return.
        List<Tile> neighbours = new List<Tile>();
        // Create a Tile object.
        Tile nTile = curTile;
        // Store current tile grid pos.
        Vector2 cPos = curTile.gridPos;

        foreach (Vector2 p in neighbourPositions)
        {
            // Find position of neighbour on grid, relative to current.
            Vector2 nPos = cPos + p;
            // If tile exists.
            if (allTiles.ContainsKey(nPos)) nTile = allTiles[nPos];
            // If tile is unvisited.
            if (unvisitedTiles.Contains(nTile)) neighbours.Add(nTile);
        }

        return neighbours;
    }

    // Compare neighbour with current and remove appropriate walls.
    public void CompareWalls(Tile cTile, Tile nTile)
    {
        // If neighbour is left of current.
        if (nTile.gridPos.x < cTile.gridPos.x)
        {
            RemoveWall(nTile.tScript, 2);
            RemoveWall(cTile.tScript, 1);
        }
        // Else if neighbour is right of current.
        else if (nTile.gridPos.x > cTile.gridPos.x)
        {
            RemoveWall(nTile.tScript, 1);
            RemoveWall(cTile.tScript, 2);
        }
        // Else if neighbour is above current.
        else if (nTile.gridPos.y > cTile.gridPos.y)
        {
            RemoveWall(nTile.tScript, 4);
            RemoveWall(cTile.tScript, 3);
        }
        // Else if neighbour is below current.
        else if (nTile.gridPos.y < cTile.gridPos.y)
        {
            RemoveWall(nTile.tScript, 3);
            RemoveWall(cTile.tScript, 4);
        }
    }

    // Function disables wall of your choosing, pass it the script attached to the desired tile
    // and an 'ID', where the ID = the wall. 1 = left, 2 = right, 3 = up, 4 = down.
    public void RemoveWall(TileScript tScript, int wallID)
    {
        if (wallID == 1) tScript.wallLeft.SetActive(false);
        else if (wallID == 2) tScript.wallRight.SetActive(false);
        else if (wallID == 3) tScript.wallUp.SetActive(false);
        else if (wallID == 4) tScript.wallDown.SetActive(false);
    }

    public void CreateCenter()
    {
        // Get the 4 centre tiles using the width and height variables.
        // Remove the required walls for each.
        centerTiles[0] = allTiles[new Vector2((mazeWidth / 2), (mazeHeight / 2) + 1)];
        RemoveWall(centerTiles[0].tScript, 4);
        RemoveWall(centerTiles[0].tScript, 2);
        centerTiles[1] = allTiles[new Vector2((mazeWidth / 2) + 1, (mazeHeight / 2) + 1)];
        RemoveWall(centerTiles[1].tScript, 4);
        RemoveWall(centerTiles[1].tScript, 1);
        centerTiles[2] = allTiles[new Vector2((mazeWidth / 2), (mazeHeight / 2))];
        RemoveWall(centerTiles[2].tScript, 3);
        RemoveWall(centerTiles[2].tScript, 2);
        centerTiles[3] = allTiles[new Vector2((mazeWidth / 2) + 1, (mazeHeight / 2))];
        RemoveWall(centerTiles[3].tScript, 3);
        RemoveWall(centerTiles[3].tScript, 1);

        // Create a List of ints, using this, select one at random and remove it.
        // We then use the remaining 3 ints to remove 3 of the centre tiles from the 'unvisited' list.
        // This ensures that one of the centre tiles will connect to the maze but the other three won't.
        // This way, the centre room will only have 1 entry / exit point.
        List<int> rndList = new List<int> { 0, 1, 2, 3 };
        int startTile = rndList[Random.Range(0, rndList.Count)];
        rndList.Remove(startTile);
        currentTile = centerTiles[startTile];
        foreach (int c in rndList)
        {
            unvisitedTiles.Remove(centerTiles[c]);
        }
    }

    public void GenerateTile(Vector2 pos, Vector2 keyPos)
    {
        // Create new Tile object.
        Tile newTile = new Tile();

        // Store reference to position in grid.
        newTile.gridPos = keyPos;
        // Set and instantiate tile GameObject.
        newTile.tileObject = Instantiate(tilePrefab, pos, tilePrefab.transform.rotation);
        // Child new tile to parent.
        if (mazeParent != null) newTile.tileObject.transform.parent = mazeParent.transform;
        // Set name of tileObject.
        newTile.tileObject.name = "Tile - X:" + keyPos.x + " Y:" + keyPos.y;
        // Get reference to attached TileScript.
        newTile.tScript = newTile.tileObject.GetComponent<TileScript>();
        // Disable Tile sprite, if applicable.
        if (hideTileSprite) newTile.tileObject.GetComponent<SpriteRenderer>().enabled = false;

        // Add to Lists.
        allTiles[keyPos] = newTile;
        unvisitedTiles.Add(newTile);
    }

    public void DestroyMaze()
    {
        if (mazeParent != null) Destroy(mazeParent);
    }

    public void InitValues()
    {
        // Check generation values to prevent generation failing.
        if (IsOdd(mazeHeight)) mazeHeight--;
        if (IsOdd(mazeWidth)) mazeWidth--;

        if (mazeHeight <= 3) mazeHeight = 4;
        if (mazeWidth <= 3) mazeWidth = 4;

        // Determine size of tile using localScale.
        tileSize = tilePrefab.transform.localScale.x;

        // Create an empty parent object to hold the maze in the scene.
        mazeParent = new GameObject();
        mazeParent.transform.position = Vector2.zero;
        mazeParent.name = "Maze";
    }

    public bool IsOdd(int value)
    {
        return value % 2 != 0;
    }

    public class Tile
    {
        public Vector2 gridPos;
        public GameObject tileObject;
        public TileScript tScript;
    }
}
