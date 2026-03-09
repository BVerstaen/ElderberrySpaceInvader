using System.Collections.Generic;
using UnityEngine;

public class Wave : MonoBehaviour
{
    enum Move { Left = 0, Down = 1, Right = 2 }
    readonly Vector3[] directions = { Vector3.left, Vector3.down, Vector3.right };

    [SerializeField] private int rows = 5;
    [SerializeField] private int columns = 11;

    [SerializeField] private Invader invaderPrefab = null;

    // Initial bounds in which invaders are spawning.
    [SerializeField] private Vector2 bounds;

    // Difficulty progress depending on enemy left ratio
    [SerializeField] private AnimationCurve difficultyProgress = AnimationCurve.Linear(0, 0, 1, 1);

    // Speed min and max depending on difficulty progress
    [SerializeField] private float speedMin;
    [SerializeField] private float speedMax;

    // Random shoot rate min and max depending on difficulty progress
    [SerializeField] private Vector2 shootRandomMin = new(3f,5f);
    [SerializeField] private Vector2 shootRandomMax = new(1f, 3f);

    // A cozy time with no alien harm at start of the game. I guess Player shoot first.
    [SerializeField] private float timeBeforeFirstShoot = 5f;

    // Distance moved when moving downward
    [SerializeField] private float downStep = 1f;

    private Bounds Bounds => new Bounds(transform.position, new Vector3(bounds.x, bounds.y, 1000f));

    Move move = Move.Right;
    int moveCount = 0;

    float distance = 0f;

    float shootCooldown;

    struct Column { public int id; public List<Invader> invaders; }
    struct Row { public int id; public List<Invader> invaders; }

    List<Invader> invaders = new();
    List<Column> invaderPerColumn = new(); // Keeps track of invaders per column. A column will be removed if empty.
    List<Row> invaderPerRow = new(); // Keeps track of invaders per row. A row will be removed if empty.

    void Awake()
    {
        shootCooldown = timeBeforeFirstShoot;

        for (int i = 0; i < columns; i++)
        {
            invaderPerColumn.Add(new() { id = i, invaders= new() });
        }
        for (int i = 0; i < rows; i++)
        {
            invaderPerRow.Add(new() { id = i, invaders = new() });
        }

        // Spaw the invader grid
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Invader invader = GameObject.Instantiate<Invader>(invaderPrefab, GetPosition(i, j), Quaternion.identity, transform);
                invader.Initialize(new Vector2Int(i, j));
                invader.onDestroy += RemoveInvader;
                invaders.Add(invader);
                invaderPerColumn[i].invaders.Add(invader);
                invaderPerRow[j].invaders.Add(invader);
            }
        }
        
    }

    void Update()
    {
        UpdateMovement();
        UpdateShoot();
    }

    private void UpdateShoot()
    {
        shootCooldown -= Time.deltaTime;
        if (shootCooldown > 0) { return; }

        // Shoot rate depends on remaining invaders ratio
        float t = 1f - (invaders.Count - 1) / (float)((rows * columns) - 1);
        Vector2 shootRandom = Vector2.Lerp(shootRandomMin, shootRandomMax, difficultyProgress.Evaluate(t));

        // One column is selected to shoot a bullet. Only the invader at the bottom of that column can shoot.
        int columnIndex = Random.Range(0, invaderPerColumn.Count);
        invaderPerColumn[columnIndex].invaders[0].Shoot();

        shootCooldown += Random.Range(shootRandom.x, shootRandom.y);
    }

    void UpdateMovement()
    {
        if(invaders.Count <= 0) { return; }

        // Speed depends on remaining invaders ratio
        float t = 1f - (invaders.Count - 1) / (float)((rows * columns) - 1);
        float speed = Mathf.Lerp(speedMin, speedMax, difficultyProgress.Evaluate(t));

        Vector3 direction = directions[(int)move];
        float delta = speed * Time.deltaTime;
        distance += delta;

        switch (move)
        {
            case Move.Right:
                {
                    // Get the last non-empty column position
                    float right = GetColumnPosition(invaderPerColumn[^1].id);
                    float nextRight = right + delta;
                    // Check if position will be out of game bound after beeing moved
                    if (!GameManager.Instance.IsInBounds(nextRight, GameManager.DIRECTION.Right))
                    {
                        // Adjust "delta" to keep invaders exactly in bounds
                        delta = GameManager.Instance.KeepInBounds(nextRight, GameManager.DIRECTION.Right) - right;
                        BeginNextMove();
                    }
                    break;
                }
            case Move.Left:
                {
                    // Get the first non-empty column position
                    float left = GetColumnPosition(invaderPerColumn[0].id);
                    float nextLeft = left + delta;
                    // Check if position will be out of game bound after beeing moved
                    if (!GameManager.Instance.IsInBounds(nextLeft, GameManager.DIRECTION.Left))
                    {
                        // Adjust "delta" to keep invaders exactly in bounds
                        delta = GameManager.Instance.KeepInBounds(nextLeft, GameManager.DIRECTION.Left) - left;
                        BeginNextMove();
                    }
                    break;
                }
            case Move.Down:
                {
                    float bottom = GetRowPosition(invaderPerRow[0].id);
                    if (GameManager.Instance.IsBelowGameOver(bottom))
                    {
                        GameManager.Instance.PlayGameOver();
                    }

                    if(distance >= downStep)
                    {
                        // Adjust "delta" to place invaders exactly at end of downStep
                        delta -= (distance - downStep);
                        BeginNextMove();
                    }
                    break;
                }
        }

        // Move invaders with adjusted delta.
        transform.position = transform.position + direction * delta;
    }

    void BeginNextMove()
    {
        // "moveCount" keep tracks on the number of move steps invaders have already made to know if we need to go left or right when finishing going downward
        moveCount++;
        switch (move)
        {
            case Move.Down:
                move = (moveCount / 2) % 2 == 0 ? Move.Right : Move.Left; break;
            case Move.Right:
            case Move.Left:
            default: 
                move = Move.Down; break;
        }
        distance = 0f;
    }

    /// <summary>
    /// Removing an invader from the wave will remove it from "invaders", "invaderPerColumn" and "invaderPerRow". If a column or a row is empty, it will be removed.
    /// </summary>
    void RemoveInvader(Invader invader)
    {
        invaders.Remove(invader);

        int indexColumn = invaderPerColumn.FindIndex(x => x.id == invader.GridIndex.x);
        if(indexColumn != -1)
        {
            Column column = invaderPerColumn[indexColumn];
            column.invaders.Remove(invader);
            if (column.invaders.Count <= 0)
            {
                invaderPerColumn.RemoveAt(indexColumn);
            }
            else
            {
                invaderPerColumn[indexColumn] = column;
            }
        }

        int indexRow = invaderPerRow.FindIndex(x => x.id == invader.GridIndex.y);
        if (indexRow != -1)
        {
            Row row = invaderPerRow[indexRow];
            row.invaders.Remove(invader);
            if (row.invaders.Count <= 0)
            {
                invaderPerRow.RemoveAt(indexRow);
            }
            else
            {
                invaderPerRow[indexRow] = row;
            }
        }
    }

    // Get position of an invader in the bounding box according to it's index
    Vector3 GetPosition(int i, int j)
    {
        return new Vector3( GetColumnPosition(i), GetRowPosition(j), 0f );
    }

    // Get position of an invader in the bounding box according to it's column index
    float GetColumnPosition(int column)
    {
        return Mathf.Lerp(Bounds.min.x, Bounds.max.x, column / (float)(columns - 1));
    }

    // Get position of an invader in the bounding box according to it's row index
    float GetRowPosition(int row)
    {
        return Mathf.Lerp(Bounds.min.y, Bounds.max.y, row / (float)(rows - 1));
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(bounds.x, bounds.y, 0f));
    }
}
