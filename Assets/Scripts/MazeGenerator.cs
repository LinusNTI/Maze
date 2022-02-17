using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int width, height = 10;
    public bool useDoubleStep = true;
    public bool useRandomizedSolver = false;
    public float generationTraversalDelay = 0.1f;
    public float solvingTraversalDelay = 0.1f;
    private MazeCell[,] mazeCells;

    private System.Random r = new System.Random();

    public bool useDebugVisuals = true;
    private Vector3 curPos = Vector3.zero;
    private List<MazeCell> solvedPath = new List<MazeCell>();
    private bool backtracking = false;

    private GameObject playerMarker;

    // Start is called before the first frame update
    void Start()
    {
        playerMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        playerMarker.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);

        StartCoroutine(Run());
    }

    private void Update()
    {
        playerMarker.transform.position = curPos;
        playerMarker.GetComponent<Renderer>().material.SetColor("_Color", backtracking ? Color.blue : Color.red);
    }

    void GenerateSolvedPath()
    {
        foreach (var cell in solvedPath)
        {
            var newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newCube.transform.position = new Vector3(transform.position.x + cell.x, -.48f, transform.position.z + cell.y);
            newCube.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
        }

        foreach (var cell in mazeCells)
        {
            if (cell.hasBreadcrumb)
            {
                var newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newCube.transform.position = new Vector3(transform.position.x + cell.x, -.49f, transform.position.z + cell.y);
                newCube.GetComponent<Renderer>().material.SetColor("_Color", Color.cyan);
            }
        }
    }

    IEnumerator Run()
    {
        yield return Generate();
        yield return Solve();
    }

    IEnumerator Solve()
    {
        Debug.Log("Started solving...");

        // Select starting point
        MazeCell currentCell = new MazeCell(); // Start
        MazeCell end = new MazeCell();

        Debug.Log("Getting start position");
        List<MazeCell> possibleCells = new List<MazeCell>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mazeCells[y, x].visited)
                    possibleCells.Add(mazeCells[y, x]);
            }
        }
        currentCell = possibleCells[r.Next(0, possibleCells.Count)];

        possibleCells.Clear();

        Debug.Log("Getting end position");
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mazeCells[y, x].visited)
                    possibleCells.Add(mazeCells[y, x]);
            }
        }
        end = possibleCells[r.Next(0, possibleCells.Count)];

        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3(transform.position.x + end.x, 1f, transform.position.z + end.y);
        sphere.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);

        Debug.Log("Beginning the solve loop");
        bool isSolving = true;
        bool isBacktracking = false;
        List<MazeCell> stack = new List<MazeCell>();
        while (isSolving)
        {
            List<MazeCell> moves = GetValidSolveMoves(currentCell.x, currentCell.y);
            if (isBacktracking)
            {
                if (moves.Count > 0)
                {
                    isBacktracking = false;
                    stack.Add(currentCell);
                    currentCell = moves[useRandomizedSolver ? r.Next(0, moves.Count) : 0];
                    mazeCells[currentCell.y, currentCell.x].hasBreadcrumb = true;
                    curPos = new Vector3(transform.position.x + currentCell.x, -.5f, transform.position.z + currentCell.y);
                    if (currentCell.x == end.x && currentCell.y == end.y)
                    {
                        Debug.Log($"Solved Maze! Position: {currentCell.x}, {currentCell.y}");
                        stack.Add(currentCell);
                        isSolving = false;
                        solvedPath = stack;
                        GenerateSolvedPath();
                    }
                }
                else
                {
                    if (stack.Count > 0)
                    {
                        int lastIndex = stack.Count - 1;
                        currentCell = stack[lastIndex];
                        curPos = new Vector3(transform.position.x + currentCell.x, -.5f, transform.position.z + currentCell.y);
                        stack.RemoveAt(lastIndex);
                    }
                    else
                    {
                        if (moves.Count > 0)
                        {
                            isBacktracking = false;
                            stack.Add(currentCell);
                            currentCell = moves[useRandomizedSolver ? r.Next(0, moves.Count) : 0];
                            mazeCells[currentCell.y, currentCell.x].hasBreadcrumb = true;
                            curPos = new Vector3(transform.position.x + currentCell.x, -.5f, transform.position.z + currentCell.y);
                            if (currentCell.x == end.x && currentCell.y == end.y)
                            {
                                Debug.Log($"Solved Maze! Position: {currentCell.x}, {currentCell.y}");
                                stack.Add(currentCell);
                                isSolving = false;
                                solvedPath = stack;
                                GenerateSolvedPath();
                            }
                        }
                        else
                        {
                            Debug.Log("Maze was unsolvable, exiting...");
                            isSolving = false;
                        }
                    }
                }
            }
            else
            {
                if (moves.Count > 0)
                {
                    isBacktracking = false;
                    stack.Add(currentCell);
                    currentCell = moves[useRandomizedSolver ? r.Next(0, moves.Count) : 0];
                    mazeCells[currentCell.y, currentCell.x].hasBreadcrumb = true;
                    curPos = new Vector3(transform.position.x + currentCell.x, -.5f, transform.position.z + currentCell.y);
                    if (currentCell.x == end.x && currentCell.y == end.y)
                    {
                        Debug.Log($"Solved Maze! Position: {currentCell.x}, {currentCell.y}");
                        stack.Add(currentCell);
                        isSolving = false;
                        solvedPath = stack;
                        GenerateSolvedPath();
                    }
                }
                else
                {
                    isBacktracking = true;
                }
            }

            if (solvingTraversalDelay != 0f)
            {
                yield return new WaitForSeconds(solvingTraversalDelay);
            }
        }

        yield return new WaitForSeconds(.1f);
    }

    IEnumerator Generate()
    {
        mazeCells = new MazeCell[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                mazeCells[y, x] = new MazeCell(x, y);
            }
        }

        // Select random cell
        int rY = r.Next(0, height - 1);
        int rX = r.Next(0, width - 1);
        MazeCell currentCell = (MazeCell)CellAt(rX, rY);

        // Generate background platform
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.transform.parent = gameObject.transform;
        platform.transform.localPosition = new Vector3((width / 2), -.55f, (height / 2));
        platform.transform.localScale = new Vector3(width+1, 1, height+1);
        platform.GetComponent<Renderer>().material.SetColor("_Color", Color.red);

        GameObject camObj = Camera.main.gameObject;
        Vector3 camPos = camObj.transform.position;
        camPos = new Vector3((width / 2), camPos.y, (height / 2));

        camObj.transform.position = camPos;

        List<MazeCell> stack = new List<MazeCell>();
        bool isBacktracking = false;
        bool isRunning = true;
        while (isRunning)
        {
            List<MazeCell> moves = GetValidMoves(currentCell.x, currentCell.y);

            backtracking = isBacktracking;
            if (isBacktracking)
            {
                Debug.Log("We are backtracking");
                if (moves.Count > 0)
                {
                    Debug.Log("Valid move found, moving and pushing old move to stack, disabling backtracking");
                    stack.Add(currentCell);
                    var newMove = moves[r.Next(0, moves.Count)];
                    if (useDoubleStep)
                    {
                        var diffX = newMove.x - currentCell.x;
                        var diffY = newMove.y - currentCell.y;
                        var y = currentCell.y + (diffY != 0 ? diffY / 2 : 0);
                        var x = currentCell.x + (diffX != 0 ? diffX / 2 : 0);
                        mazeCells[y, x].visited = true;
                        GameObject.CreatePrimitive(PrimitiveType.Cube).transform.position = new Vector3(transform.position.x + x, -.5f, transform.position.z + y);
                    }
                    currentCell = newMove;
                    curPos = new Vector3(transform.position.x + currentCell.x, -.5f, transform.position.z + currentCell.y);
                    mazeCells[currentCell.y, currentCell.x].visited = true;
                    GameObject.CreatePrimitive(PrimitiveType.Cube).transform.position = new Vector3(transform.position.x + currentCell.x, -.5f, transform.position.z + currentCell.y);
                    isBacktracking = false;
                }
                else
                {
                    Debug.Log("No valid move found;");
                    if (stack.Count > 0)
                    {
                        Debug.Log("Found move on stack, backtracking...");
                        int lastIndex = stack.Count - 1;
                        currentCell = stack[lastIndex];
                        curPos = new Vector3(transform.position.x + currentCell.x, -.5f, transform.position.z + currentCell.y);
                        stack.RemoveAt(lastIndex);
                    }
                    else
                    {
                        Debug.Log("There are no more moves on the stack, exiting...");
                        isRunning = false;
                    }
                }
            }
            else
            {
                Debug.Log("Not Backtracking");
                if (moves.Count > 0)
                {
                    Debug.Log("Valid move found, moving and pushing old cell to stack");
                    stack.Add(currentCell);
                    var newMove = moves[r.Next(0, moves.Count)];
                    if (useDoubleStep)
                    {
                        var diffX = newMove.x - currentCell.x;
                        var diffY = newMove.y - currentCell.y;
                        var y = currentCell.y + (diffY != 0 ? diffY / 2 : 0);
                        var x = currentCell.x + (diffX != 0 ? diffX / 2 : 0);
                        mazeCells[y, x].visited = true;
                        GameObject.CreatePrimitive(PrimitiveType.Cube).transform.position = new Vector3(transform.position.x + x, -.5f, transform.position.z + y);
                    }
                    currentCell = newMove;
                    curPos = new Vector3(transform.position.x + currentCell.x, -.5f, transform.position.z + currentCell.y);
                    mazeCells[currentCell.y, currentCell.x].visited = true;
                    GameObject.CreatePrimitive(PrimitiveType.Cube).transform.position = new Vector3(transform.position.x + currentCell.x, -.5f, transform.position.z + currentCell.y);
                }
                else
                {
                    Debug.Log("Dead-end, enabling backtrack");
                    isBacktracking = true;
                }
            }

            if (generationTraversalDelay != 0f)
            {
                yield return new WaitForSeconds(generationTraversalDelay);
            }
        }
    }

    MazeCell? CellAt(int x, int y)
    {
        if (x < 0 || y < 0 || x > width - 1 || y > height - 1)
            return null;
        return mazeCells[y, x];
    }

    bool CellIsClear(int x, int y)
    {
        object top = CellAt(x, y + 1);
        object bottom = CellAt(x, y - 1);
        object left = CellAt(x - 1, y);
        object right = CellAt(x + 1, y);

        int validCells = 0;
        if (top == null || !((MazeCell)top).visited)
            validCells++;
        if (bottom == null || !((MazeCell)bottom).visited)
            validCells++;
        if (left == null || !((MazeCell)left).visited)
            validCells++;
        if (right == null || !((MazeCell)right).visited)
            validCells++;

        return validCells >= 3;
    }

    List<MazeCell> GetValidMoves(int x, int y)
    {
        List<MazeCell> validMoves = new List<MazeCell>();

        if (!useDoubleStep)
        {
            object top = CellAt(x, y + 1);
            object bottom = CellAt(x, y - 1);
            object left = CellAt(x - 1, y);
            object right = CellAt(x + 1, y);

            if (top != null && !((MazeCell)top).visited && CellIsClear(x, y + 1))
                validMoves.Add((MazeCell)top);
            if (bottom != null && !((MazeCell)bottom).visited && CellIsClear(x, y - 1))
                validMoves.Add((MazeCell)bottom);
            if (left != null && !((MazeCell)left).visited && CellIsClear(x - 1, y))
                validMoves.Add((MazeCell)left);
            if (right != null && !((MazeCell)right).visited && CellIsClear(x + 1, y))
                validMoves.Add((MazeCell)right);
        }
        else
        {
            object top = CellAt(x, y + 2);
            object bottom = CellAt(x, y - 2);
            object left = CellAt(x - 2, y);
            object right = CellAt(x + 2, y);

            if (top != null && !((MazeCell)top).visited && CellIsClear(x, y + 2))
                validMoves.Add((MazeCell)top);
            if (bottom != null && !((MazeCell)bottom).visited && CellIsClear(x, y - 2))
                validMoves.Add((MazeCell)bottom);
            if (left != null && !((MazeCell)left).visited && CellIsClear(x - 2, y))
                validMoves.Add((MazeCell)left);
            if (right != null && !((MazeCell)right).visited && CellIsClear(x + 2, y))
                validMoves.Add((MazeCell)right);
        }

        return validMoves;
    }

    List<MazeCell> GetValidSolveMoves(int x, int y)
    {
        List<MazeCell> validMoves = new List<MazeCell>();

        object top = CellAt(x, y + 1);
        object bottom = CellAt(x, y - 1);
        object left = CellAt(x - 1, y);
        object right = CellAt(x + 1, y);

        if (top != null && ((MazeCell)top).visited && !((MazeCell)top).hasBreadcrumb)
            validMoves.Add((MazeCell)top);
        if (bottom != null && ((MazeCell)bottom).visited && !((MazeCell)bottom).hasBreadcrumb)
            validMoves.Add((MazeCell)bottom);
        if (left != null && ((MazeCell)left).visited && !((MazeCell)left).hasBreadcrumb)
            validMoves.Add((MazeCell)left);
        if (right != null && ((MazeCell)right).visited && !((MazeCell)right).hasBreadcrumb)
            validMoves.Add((MazeCell)right);

        return validMoves;
    }

    private void OnDrawGizmos()
    {
        if (!useDebugVisuals)
            return;

        Gizmos.color = Color.green;
        for (int i = 0; i < solvedPath.Count; i++)
        {
            var pos = new Vector3(transform.position.x + solvedPath[i].x, -.49f, transform.position.z + solvedPath[i].y);
            Gizmos.DrawCube(pos, Vector3.one);
        }

        Gizmos.color = backtracking ? Color.blue : Color.red;
        Gizmos.DrawCube(curPos, new Vector3(1.1f, 1.1f, 1.1f));
    }
}

struct MazeCell
{
    public int x, y;
    public bool visited;
    public bool hasBreadcrumb;

    public MazeCell(int x, int y)
    {
        this.x = x;
        this.y = y;
        visited = false;
        hasBreadcrumb = false;
    }
}
