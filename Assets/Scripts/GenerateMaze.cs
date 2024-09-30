using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMaze : MonoBehaviour
{
    [SerializeField]
    GameObject roomPrefab;

    // The grid.
    Room[,] rooms = null;

    [SerializeField]
    int numX = 10;  // Number of rooms horizontally
    [SerializeField]
    int numY = 10;  // Number of rooms vertically

    // Room dimensions.
    float roomWidth;
    float roomHeight;

    // Stack for backtracking.
    Stack<Room> stack = new Stack<Room>();

    // Flag to indicate if the generation is in progress.
    bool generating = false;

    // Get the size of the room based on the prefab's child components.
    private void GetRoomSize()
    {
        SpriteRenderer[] spriteRenderers =
          roomPrefab.GetComponentsInChildren<SpriteRenderer>();

        Vector3 minBounds = Vector3.positiveInfinity;
        Vector3 maxBounds = Vector3.negativeInfinity;

        foreach (SpriteRenderer ren in spriteRenderers)
        {
            minBounds = Vector3.Min(minBounds, ren.bounds.min);
            maxBounds = Vector3.Max(maxBounds, ren.bounds.max);
        }

        roomWidth = maxBounds.x - minBounds.x;
        roomHeight = maxBounds.y - minBounds.y;
    }

    // Adjust the camera to fit the generated maze.
    private void SetCamera()
    {
        Camera.main.transform.position = new Vector3(
          numX * (roomWidth - 1) / 2,
          numY * (roomHeight - 1) / 2,
          -100.0f);

        float min_value = Mathf.Min(
          numX * (roomWidth - 1),
          numY * (roomHeight - 1));
        Camera.main.orthographicSize = min_value * 0.75f;
    }

    // Start the maze generation process.
    private void Start()
    {
        GetRoomSize();

        rooms = new Room[numX, numY];

        for (int i = 0; i < numX; ++i)
        {
            for (int j = 0; j < numY; ++j)
            {
                GameObject room = Instantiate(roomPrefab,
                  new Vector3(i * roomWidth, j * roomHeight, 0.0f),
                  Quaternion.identity);

                room.name = "Room_" + i.ToString() + "_" + j.ToString();
                rooms[i, j] = room.GetComponent<Room>();
                rooms[i, j].Index = new Vector2Int(i, j);
            }
        }

        SetCamera();
    }

    // Remove the wall between two rooms.
    private void RemoveRoomWall(int x, int y, Room.Directions dir)
    {
        if (dir != Room.Directions.NONE)
        {
            rooms[x, y].SetDirFlag(dir, false);
        }
        Room.Directions opp = Room.Directions.NONE;
        switch (dir)
        {
            case Room.Directions.TOP:
                if (y < numY - 1)
                {
                    opp = Room.Directions.BOTTOM;
                    ++y;
                }
                break;

            case Room.Directions.BOTTOM:
                if (y > 0)
                {
                    opp = Room.Directions.TOP;
                    --y;
                }
                break;

            case Room.Directions.LEFT:
                if (x > 0)
                {
                    opp = Room.Directions.RIGHT;
                    --x;
                }
                break;

            case Room.Directions.RIGHT:
                if (x < numX - 1)
                {
                    opp = Room.Directions.LEFT;
                    ++x;
                }
                break;
        }
        if (opp != Room.Directions.NONE)
        {
            rooms[x, y].SetDirFlag(opp, false);
        }
    }

    // Get a list of neighbors that have not been visited.
    public List<Tuple<Room.Directions, Room>> GetNeighboursNotVisited(int cx, int cy)
    {
        List<Tuple<Room.Directions, Room>> neighbours = new List<Tuple<Room.Directions, Room>>();
        foreach (Room.Directions dir in Enum.GetValues(typeof(Room.Directions)))
        {
            int x = cx;
            int y = cy;
            switch (dir)
            {
                case Room.Directions.TOP:
                    if (y < numY - 1)
                    {
                        ++y;
                        if (!rooms[x, y].visited)
                        {
                            neighbours.Add(new Tuple<Room.Directions, Room>(Room.Directions.TOP, rooms[x, y]));
                        }
                    }
                    break;
                case Room.Directions.RIGHT:
                    if (x < numX - 1)
                    {
                        ++x;
                        if (!rooms[x, y].visited)
                        {
                            neighbours.Add(new Tuple<Room.Directions, Room>(Room.Directions.RIGHT, rooms[x, y]));
                        }
                    }
                    break;
                case Room.Directions.BOTTOM:
                    if (y > 0)
                    {
                        --y;
                        if (!rooms[x, y].visited)
                        {
                            neighbours.Add(new Tuple<Room.Directions, Room>(Room.Directions.BOTTOM, rooms[x, y]));
                        }
                    }
                    break;
                case Room.Directions.LEFT:
                    if (x > 0)
                    {
                        --x;
                        if (!rooms[x, y].visited)
                        {
                            neighbours.Add(new Tuple<Room.Directions, Room>(Room.Directions.LEFT, rooms[x, y]));
                        }
                    }
                    break;
            }
        }
        return neighbours;
    }

    // Perform one step of the maze generation algorithm.
    private bool GenerateStep()
    {
        if (stack.Count == 0) return true;  // If no more rooms in the stack, maze is complete.
        Room r = stack.Peek();  // Get the current room from the stack.
        var neighbours = GetNeighboursNotVisited(r.Index.x, r.Index.y);  // Get unvisited neighbors.
        if (neighbours.Count != 0)
        {
            // Choose a random neighbor.
            var index = UnityEngine.Random.Range(0, neighbours.Count);
            var item = neighbours[index];
            Room neighbour = item.Item2;
            neighbour.visited = true;
            RemoveRoomWall(r.Index.x, r.Index.y, item.Item1);  // Remove the wall between the rooms.
            stack.Push(neighbour);  // Add the neighbor to the stack.
        }
        else
        {
            stack.Pop();  // No more neighbors, backtrack.
        }
        return false;
    }

    // Start creating the maze.
    public void CreateMaze()
    {
        if (generating) return;  // If already generating, don't start a new process.
        Reset();
        RemoveRoomWall(0, 0, Room.Directions.BOTTOM);  // Make the entrance.
        RemoveRoomWall(numX - 1, numY - 1, Room.Directions.RIGHT);  // Make the exit.
        stack.Push(rooms[0, 0]);  // Start from the top-left corner.
        StartCoroutine(Coroutine_Generate());
    }

    // Coroutine to generate the maze step-by-step.
    IEnumerator Coroutine_Generate()
    {
        generating = true;
        bool done = false;
        while (!done)
        {
            done = GenerateStep();
            yield return new WaitForSeconds(0.05f);  // Slow down the process for visualization.
        }
        generating = false;
    }

    // Reset the maze by closing all walls and setting all rooms as unvisited.
    private void Reset()
    {
        for (int i = 0; i < numX; ++i)
        {
            for (int j = 0; j < numY; ++j)
            {
                rooms[i, j].SetDirFlag(Room.Directions.TOP, true);
                rooms[i, j].SetDirFlag(Room.Directions.RIGHT, true);
                rooms[i, j].SetDirFlag(Room.Directions.BOTTOM, true);
                rooms[i, j].SetDirFlag(Room.Directions.LEFT, true);
                rooms[i, j].visited = false;
            }
        }
    }

    // Update function to start the maze generation when the space bar is pressed.
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!generating)
            {
                CreateMaze();
            }
        }
    }
}
