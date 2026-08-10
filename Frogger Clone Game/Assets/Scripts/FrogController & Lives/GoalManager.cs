using System;
using System.Collections.Generic;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;

    [Header("Goal Marker")]
    [Tooltip("Prefab instantiated in a goal slot once a frog reaches it - the frog that 'remains there'.")]
    [SerializeField] private GameObject goalMarkerPrefab;

    [Header("UI (optional)")]
    [Tooltip("Shown when all 5 goal slots are filled.")]
    [SerializeField] private GameObject winPanel;

    // Goal cell -> filled state. Built automatically from GridManager's
    // configured rows, so the 5 slots never need to be re-typed here.
    private Dictionary<Vector2Int, bool> goalSlots = new Dictionary<Vector2Int, bool>();

    // Track spawned markers to prevent duplicates
    private Dictionary<Vector2Int, GameObject> spawnedMarkers = new Dictionary<Vector2Int, GameObject>();

    public event Action OnAllSlotsFilled;

    public int TotalSlots => goalSlots.Count;
    public int FilledSlots { get; private set; }

    // Prevent win from triggering multiple times
    private bool hasTriggeredWin = false;

    private void Awake()
    {
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }
    }

    private void Start()
    {
        if (gridManager == null)
        {
            Debug.LogError("GoalManager: no GridManager found in the scene!");
            return;
        }

        List<Vector2Int> goalPositions = gridManager.GetGoalPositions();

        foreach (Vector2Int pos in goalPositions)
        {
            goalSlots[pos] = false;
        }

        FilledSlots = 0;
        hasTriggeredWin = false;

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        Debug.Log($"GoalManager: tracking {goalSlots.Count} goal slot(s)");
    }

    // Attempts to fill the goal slot at the given grid position.
    // Returns true if it was empty and is now filled, false if that
    // position isn't a goal slot or was already taken.
    public bool TryFillGoal(Vector2Int gridPosition)
    {
        if (!goalSlots.ContainsKey(gridPosition))
        {
            Debug.LogWarning($"GoalManager: {gridPosition} is not a registered goal slot");
            return false;
        }

        if (goalSlots[gridPosition])
        {
            // Already occupied - the caller should treat this as a death.
            return false;
        }

        // Mark as filled BEFORE spawning to prevent duplicate spawns
        goalSlots[gridPosition] = true;
        FilledSlots++;

        SpawnMarker(gridPosition);

        // Check if all goals are filled
        if (FilledSlots >= goalSlots.Count && !hasTriggeredWin)
        {
            TriggerWin();
        }

        Debug.Log($"Goal progress: {FilledSlots}/{goalSlots.Count} filled");
        return true;
    }

    private void SpawnMarker(Vector2Int gridPosition)
    {
        // Prevent duplicate markers
        if (spawnedMarkers.ContainsKey(gridPosition) && spawnedMarkers[gridPosition] != null)
        {
            Debug.LogWarning($"Marker already exists at {gridPosition}, destroying duplicate");
            Destroy(spawnedMarkers[gridPosition]);
            spawnedMarkers.Remove(gridPosition);
        }

        Vector2 worldPosition = gridManager.GetWorldPosition(gridPosition.x, gridPosition.y);

        if (goalMarkerPrefab != null)
        {
            GameObject marker = Instantiate(goalMarkerPrefab, worldPosition, Quaternion.identity, transform);
            spawnedMarkers[gridPosition] = marker;
            Debug.Log($"Spawned marker at {gridPosition} (Total: {FilledSlots}/{goalSlots.Count})");
        }
        else
        {
            Debug.LogWarning("GoalManager: no goalMarkerPrefab assigned - filled slot has no visual.");
        }
    }

    private void TriggerWin()
    {
        if (hasTriggeredWin) return;

        hasTriggeredWin = true;
        Debug.Log("GoalManager: ALL GOAL SLOTS FILLED - YOU WIN!");

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        OnAllSlotsFilled?.Invoke();
    }

    // Check if a specific goal is filled
    public bool IsGoalFilled(Vector2Int gridPosition)
    {
        if (goalSlots.ContainsKey(gridPosition))
        {
            return goalSlots[gridPosition];
        }
        return false;
    }

    // Get all filled goal positions
    public List<Vector2Int> GetFilledGoals()
    {
        List<Vector2Int> filled = new List<Vector2Int>();
        foreach (var kvp in goalSlots)
        {
            if (kvp.Value)
            {
                filled.Add(kvp.Key);
            }
        }
        return filled;
    }

    // Reset goals (for restarting the game)
    public void ResetGoals()
    {
        // Destroy all markers
        foreach (var marker in spawnedMarkers.Values)
        {
            if (marker != null)
                Destroy(marker);
        }
        spawnedMarkers.Clear();

        // Reset dictionary
        List<Vector2Int> keys = new List<Vector2Int>(goalSlots.Keys);
        foreach (Vector2Int key in keys)
        {
            goalSlots[key] = false;
        }

        FilledSlots = 0;
        hasTriggeredWin = false;

        if (winPanel != null)
            winPanel.SetActive(false);

        Debug.Log("Goals reset");
    }
}