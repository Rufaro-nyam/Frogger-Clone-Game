using System;
using System.Collections.Generic;
using UnityEngine;
using static GridPositionTypes;
using static LaneTypes;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] public int width = 9;
    [SerializeField] public int height = 10;
    [SerializeField] private float cellSize = 1f;

    [Header("Map Rows")]
    [SerializeField] private GridRow[] rows;

    [Header("Debug Markers")]
    [SerializeField] private GameObject gridMarkerPrefab;

    private void Start()
    {
        CreateGridMarkers();
    }

    public Vector2 GetWorldPosition(int x, int y)
    {
        float xOffset = (width - 1) * cellSize / 2f;
        float yOffset = (height - 1) * cellSize / 2f;

        return new Vector2(
            transform.position.x + (x * cellSize) - xOffset,
            transform.position.y + (y * cellSize) - yOffset
        );
    }

    public Vector2Int GetGridPosition(Vector2 worldPosition)
    {
        float xOffset = (width - 1) * cellSize / 2f;
        float yOffset = (height - 1) * cellSize / 2f;

        int x = Mathf.RoundToInt(
            (worldPosition.x - transform.position.x + xOffset) / cellSize
        );

        int y = Mathf.RoundToInt(
            (worldPosition.y - transform.position.y + yOffset) / cellSize
        );

        return new Vector2Int(x, y);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        //vertical lines
        for (int x = 0; x < width; x++)
        {
            Vector2 start = GetWorldPosition(x, 0);
            Vector2 end = GetWorldPosition(x, height - 1);

            Gizmos.DrawLine(start, end);
        }

        //horizontal lines
        for (int y = 0; y < height; y++)
        {
            Vector2 start = GetWorldPosition(0, y);
            Vector2 end = GetWorldPosition(width - 1, y);

            Gizmos.DrawLine(start, end);
        }

        // Draw the actual grid and my positions
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = GetWorldPosition(x, y);

                Gizmos.DrawSphere(position, cellSize * 0.08f);
            }
        }

        DrawLanes();
        DrawPositions();
    }

    public LaneType GetLaneType(int y)
    {
        if (y < 0 || y >= rows.Length)
        {
            return LaneType.Safe;
        }

        return rows[y].laneType;
    }

    public GridPositionType GetPositionType(int x, int y)
    {
        if (y < 0 || y >= rows.Length)
        {
            return GridPositionType.River;
        }

        if (x < 0 || x >= rows[y].positions.Length)
        {
            return GridPositionType.River;
        }

        return rows[y].positions[x];
    }

    // Scans the configured rows and returns every cell marked as a Goal.
    // Used by GoalManager so goal slots don't need to be hand-duplicated
    // into a second list - the GridRow data you already set up is the
    // single source of truth.
    public List<Vector2Int> GetGoalPositions()
    {
        List<Vector2Int> goalPositions = new List<Vector2Int>();

        for (int y = 0; y < rows.Length; y++)
        {
            for (int x = 0; x < rows[y].positions.Length; x++)
            {
                if (rows[y].positions[x] == GridPositionType.Goal)
                {
                    goalPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        return goalPositions;
    }

    // Returns the y row of the first River lane, or -1 if none is configured.
    // Used as a fallback default for the extra-life frog's row.
    public int GetFirstRiverRow()
    {
        for (int y = 0; y < rows.Length; y++)
        {
            if (rows[y].laneType == LaneType.River)
            {
                return y;
            }
        }

        return -1;
    }

    private void DrawLanes()
    {
        for (int y = 0; y < height; y++)
        {
            LaneType laneType = GetLaneType(y);

            switch (laneType)
            {
                case LaneType.Safe:
                    Gizmos.color = Color.yellow;
                    break;

                case LaneType.Road:
                    Gizmos.color = Color.red;
                    break;

                case LaneType.River:
                    Gizmos.color = Color.blue;
                    break;

                case LaneType.Goal:
                    Gizmos.color = Color.green;
                    break;
            }

            Vector2 center = new Vector2(transform.position.x, GetWorldPosition(0, y).y);

            Vector3 size = new Vector3(
                (width) * cellSize,
                cellSize,
                0f
            );

            Gizmos.DrawWireCube(center, size);
        }
    }

    private void DrawPositions()
    {
        for (int y = 0; y < rows.Length; y++)
        {
            for (int x = 0; x < rows[y].positions.Length; x++)
            {
                GridPositionType positionType = rows[y].positions[x];

                switch (positionType)
                {
                    case GridPositionType.Safe:
                        Gizmos.color = Color.yellow;
                        break;

                    case GridPositionType.Road:
                        Gizmos.color = Color.red;
                        break;

                    case GridPositionType.River:
                        Gizmos.color = Color.blue;
                        break;

                    case GridPositionType.Goal:
                        Gizmos.color = Color.green;
                        break;
                }

                Vector2 position = GetWorldPosition(x, y);

                Gizmos.DrawSphere(
                    position,
                    cellSize * 0.15f
                );
            }
        }
    }

    private void CreateGridMarkers()
    {
        if (gridMarkerPrefab == null)
        {
            return;
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 position = GetWorldPosition(x, y);

                Instantiate(gridMarkerPrefab, position, Quaternion.identity, transform);
            }
        }
    }
}