using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static GridPositionTypes;
using static LaneTypes;

public class ExtraLifeFrog : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private FrogController player;

    [Header("Log Selection")]
    [Tooltip("The log this blue frog sits on. If not assigned, it will find a random log.")]
    [SerializeField] private GameObject targetLog;
    [Tooltip("Offset from the log's position (usually 0,0)")]
    [SerializeField] private Vector2 offsetFromLog = Vector2.zero;

    [Header("Merge Detection")]
    [Tooltip("How close (world units) the player frog needs to be to merge with this one.")]
    [SerializeField] private float mergeDistance = 0.5f;

    [Header("Timing")]
    [Tooltip("Seconds to wait before the blue frog first appears at game start.")]
    [SerializeField] private float initialDelay = 5f;
    [Tooltip("Seconds to wait before appearing on a new log after being collected.")]
    [SerializeField] private float respawnDelay = 8f;

    [Header("Visual")]
    [Tooltip("Child object representing the visible frog. Hidden while waiting to respawn.")]
    [SerializeField] private GameObject visual;

    [Header("Audio")]
    [SerializeField] private AudioSource collectSound;

    [Header("Debug Visualization")]
    [Tooltip("Show merge distance circle in Scene view")]
    [SerializeField] private bool showGizmo = true;
    [Tooltip("Color of the merge distance circle")]
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 1f, 0.3f);

    private bool collected = false;
    private float respawnTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private bool isRespawning = false;
    private bool hasSpawned = false;
    private List<GameObject> riverLogs = new List<GameObject>();
    private Vector2Int currentGridPosition;

    private void Awake()
    {
        // Ensure the GameObject is active
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning("ExtraLifeFrog: GameObject was inactive, activating it...");
            gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        // Ensure the GameObject stays active
        gameObject.SetActive(true);

        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }

        if (player == null)
        {
            player = FindObjectOfType<FrogController>();
        }

        if (gridManager == null)
        {
            Debug.LogError("ExtraLifeFrog: no GridManager found in the scene!");
            enabled = false;
            return;
        }

        // Cache sprite renderer
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Find river logs
        FindRiverLogs();

        // Find a log but hide the frog initially
        if (targetLog == null)
        {
            FindNewLog();
        }
        else
        {
            SnapToLog(targetLog);
        }

        // Hide visual at start (will appear after delay)
        ShowVisual(false);
        Debug.Log($"ExtraLifeFrog: Will appear in {initialDelay} seconds");

        // Start the initial delay coroutine
        StartCoroutine(InitialSpawnDelay());
    }

    private void FindRiverLogs()
    {
        riverLogs.Clear();

        // Find all logs in the scene
        GameObject[] allLogs = GameObject.FindGameObjectsWithTag("Log");

        foreach (GameObject log in allLogs)
        {
            // Get the grid position of this log
            Vector2Int gridPos = gridManager.GetGridPosition(log.transform.position);

            // Check if this log is on a River tile
            GridPositionType tileType = gridManager.GetPositionType(gridPos.x, gridPos.y);
            LaneType laneType = gridManager.GetLaneType(gridPos.y);

            if (tileType == GridPositionType.River || laneType == LaneType.River)
            {
                riverLogs.Add(log);
                Debug.Log($"ExtraLifeFrog: Found river log '{log.name}' at grid position ({gridPos.x}, {gridPos.y})");
            }
        }

        Debug.Log($"ExtraLifeFrog: Found {riverLogs.Count} logs on River tiles");
    }

    private IEnumerator InitialSpawnDelay()
    {
        // Wait for the initial delay
        float elapsed = 0f;
        while (elapsed < initialDelay)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Show the frog
        ShowVisual(true);
        hasSpawned = true;
        Debug.Log($"ExtraLifeFrog: Appeared after {initialDelay} seconds!");
    }

    private void Update()
    {
        // Don't do anything until the frog has spawned
        if (!hasSpawned)
        {
            return;
        }

        // Ensure GameObject stays active
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning("ExtraLifeFrog: GameObject was deactivated, reactivating...");
            gameObject.SetActive(true);
            return;
        }

        if (collected)
        {
            HandleRespawnCountdown();
            return;
        }

        if (targetLog != null)
        {
            FollowLog();
        }
        else
        {
            FindNewLog();
        }

        CheckMergeWithPlayer();
    }

    private void FollowLog()
    {
        if (targetLog == null) return;

        // Get the log's grid position
        Vector2Int logGridPos = gridManager.GetGridPosition(targetLog.transform.position);

        // Snap to the exact grid position (just like the player frog)
        Vector2 exactPos = gridManager.GetWorldPosition(logGridPos.x, logGridPos.y);
        transform.position = exactPos + offsetFromLog;

        // Update current grid position
        currentGridPosition = logGridPos;
    }

    private void FindNewLog()
    {
        Debug.Log("ExtraLifeFrog: Searching for logs on River tiles...");

        // Refresh river logs list
        FindRiverLogs();

        if (riverLogs.Count == 0)
        {
            Debug.LogWarning("ExtraLifeFrog: No logs found on River tiles! Make sure your logs are on River lanes.");
            return;
        }

        // Pick a random log from river logs
        int randomIndex = Random.Range(0, riverLogs.Count);
        targetLog = riverLogs[randomIndex];

        // Get grid position for logging
        Vector2Int gridPos = gridManager.GetGridPosition(targetLog.transform.position);
        Debug.Log($"ExtraLifeFrog: Selected river log #{randomIndex}: {targetLog.name} at grid ({gridPos.x}, {gridPos.y})");

        SnapToLog(targetLog);
    }

    private void SnapToLog(GameObject log)
    {
        if (log == null)
        {
            Debug.LogWarning("ExtraLifeFrog: SnapToLog called with null log!");
            return;
        }

        targetLog = log;

        // Get the log's grid position
        Vector2Int gridPos = gridManager.GetGridPosition(log.transform.position);

        // Snap to the exact grid position (just like the player frog)
        Vector2 exactPos = gridManager.GetWorldPosition(gridPos.x, gridPos.y);
        transform.position = exactPos + offsetFromLog;

        // Store the grid position
        currentGridPosition = gridPos;

        Debug.Log($"ExtraLifeFrog: Snapped to grid position ({gridPos.x}, {gridPos.y}) at world position {transform.position}");
    }

    private void CheckMergeWithPlayer()
    {
        if (player == null || player.IsMoving || collected)
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance <= mergeDistance)
        {
            Collect();
        }
    }

    private void Collect()
    {
        if (collected) return;

        collected = true;
        respawnTimer = 0f;
        isRespawning = true;

        // Ensure GameObject stays active
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning("ExtraLifeFrog: GameObject was inactive during collect, reactivating...");
            gameObject.SetActive(true);
        }

        if (LivesManager.Instance != null)
        {
            LivesManager.Instance.AddLife(1);
            Debug.Log("ExtraLifeFrog collected - +1 life!");
        }

        if (collectSound != null)
        {
            collectSound.Play();
        }

        // Hide visual - NEVER deactivate the GameObject!
        ShowVisual(false);
        Debug.Log("ExtraLifeFrog: Visual hidden!");

        Debug.Log($"ExtraLifeFrog collected! Will respawn in {respawnDelay} seconds.");
    }

    private void ShowVisual(bool show)
    {
        // Method 1: Use the visual child if assigned
        if (visual != null)
        {
            visual.SetActive(show);
            return;
        }

        // Method 2: Use SpriteRenderer if available
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = show;
            return;
        }

        // Method 3: Fallback - try to find any SpriteRenderer in children
        SpriteRenderer childSR = GetComponentInChildren<SpriteRenderer>();
        if (childSR != null)
        {
            childSR.enabled = show;
            return;
        }

        Debug.LogWarning($"ExtraLifeFrog: No visual or SpriteRenderer found! Cannot {(show ? "show" : "hide")} the frog.");
    }

    private void HandleRespawnCountdown()
    {
        // Ensure GameObject stays active
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning("ExtraLifeFrog: GameObject was deactivated during countdown, reactivating...");
            gameObject.SetActive(true);
            return;
        }

        // Increment timer
        respawnTimer += Time.deltaTime;

        // Log every second
        if (respawnTimer % 1f < Time.deltaTime)
        {
            float remaining = respawnDelay - respawnTimer;
            Debug.Log($"ExtraLifeFrog: Respawn in {remaining:F1} seconds");
        }

        // Check if enough time has passed
        if (respawnTimer < respawnDelay)
        {
            return;
        }

        Debug.Log($"ExtraLifeFrog: Respawn timer reached {respawnDelay} seconds! Attempting to respawn...");

        // Find a new log on a River tile
        FindNewLog();

        if (targetLog == null)
        {
            Debug.LogError("ExtraLifeFrog: Failed to find a log on a River tile! Retrying in 1 second...");
            respawnTimer = respawnDelay - 1f;
            return;
        }

        // Reset collected state
        collected = false;
        isRespawning = false;

        // Show visual again
        ShowVisual(true);
        Debug.Log("ExtraLifeFrog: Visual shown again!");

        Debug.Log($"ExtraLifeFrog respawned on a new log: {targetLog.name}");
    }

    public void SetLog(GameObject log)
    {
        if (log != null)
        {
            // Ensure GameObject is active
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            SnapToLog(log);
            collected = false;
            isRespawning = false;
            respawnTimer = 0f;
            ShowVisual(true);
        }
    }

    public void ForceCollect()
    {
        Collect();
    }

    public void ForceRespawn()
    {
        Debug.Log("ExtraLifeFrog: Force respawn called!");
        collected = false;
        isRespawning = false;
        respawnTimer = respawnDelay;
        ShowVisual(true);
        FindNewLog();
    }

    private void OnEnable()
    {
        // Called when the GameObject is enabled
        Debug.Log("ExtraLifeFrog: OnEnable called - GameObject is active");
    }

    private void OnDrawGizmos()
    {
        if (!showGizmo) return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, mergeDistance);

        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.1f);
        Gizmos.DrawSphere(transform.position, mergeDistance);

        if (Application.isPlaying && player != null && !collected && hasSpawned)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);

            if (distance <= mergeDistance)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, player.transform.position);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, player.transform.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;

        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.5f);
        Gizmos.DrawWireSphere(transform.position, mergeDistance + 0.2f);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * (mergeDistance + 0.5f),
            collected ? $"COLLECTED - Respawn in: {(respawnDelay - respawnTimer):F1}s" : $"Merge Distance: {mergeDistance}"
        );
#endif
    }
}