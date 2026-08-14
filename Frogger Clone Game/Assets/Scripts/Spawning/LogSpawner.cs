using UnityEngine;
using System.Collections.Generic;
using static LaneTypes;

public class LogSpawner : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject logPrefab;
    private List<RiverSpawner> laneSpawners = new List<RiverSpawner>();

    [Header("Blue Frog Settings")]
    [SerializeField] private ExtraLifeFrog blueFrogPrefab;
    [SerializeField] private float blueFrogInitialDelay = 5f;      // Delay before the very first spawn attempt
    [SerializeField] private float blueFrogRespawnCooldown = 8f;   // Delay after a frog is collected before trying again
    [SerializeField] private float blueFrogLifetime = 15f;        

    [Header("Left-Edge Detection")]
    [Tooltip("How far (in world units) before the leftmost grid column still counts as 'just entering'. E.g. 1.5 means the zone starts 1.5 units left of column 0 and ends exactly at column 0.")]
    [SerializeField] private float leftEdgeZoneWidth = 1.5f;

   
    private float leftEdgeX;

    private float cooldownTimer = 0f;
    private float lifetimeTimer = 0f;
    private ExtraLifeFrog currentBlueFrog = null;
    private GameObject lastUsedLog = null;

    private void Awake()
    {
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }

        CreateLaneSpawners();

        cooldownTimer = blueFrogInitialDelay;
        CalculateLeftEdge();
    }

    private void CalculateLeftEdge()
    {
        
        float leftmostColumnX = gridManager.GetWorldPosition(0, 0).x;

        // The zone runs from just off-screen left up to (and including) column 0,
        // so a log gets caught right as it enters the visible grid.
        leftEdgeX = leftmostColumnX - leftEdgeZoneWidth;
    }

    private void Update()
    {
        UpdateLogSpawning();

        // Case 1: We have an active frog and it's still uncollected.
        if (currentBlueFrog != null && !currentBlueFrog.IsCollected())
        {
            lifetimeTimer -= Time.deltaTime;
            if (lifetimeTimer <= 0f)
            {
                // Safety net: nobody grabbed it in time. Clear it out and
                // start the cooldown so a new one can appear later.
                Destroy(currentBlueFrog.gameObject);
                currentBlueFrog = null;
                cooldownTimer = blueFrogRespawnCooldown;
            }
            return;
        }

        // Case 2: The frog was just collected (or destroyed by the lifetime check above).
        if (currentBlueFrog != null && currentBlueFrog.IsCollected())
        {
            currentBlueFrog = null;
            cooldownTimer = blueFrogRespawnCooldown;
            return;
        }

        // Case 3: No active frog. Wait out the cooldown, then look for a log
        // that's genuinely in the left-edge zone (not just "anywhere on screen").
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        GameObject log = FindLogEnteringFromLeft();
        if (log != null)
        {
            SpawnBlueFrogOnLog(log);
        }
        // If no log is in the zone yet, we just keep checking next frame —
        // no timer to reset, since cooldownTimer is already at/below 0.
    }

    private void SpawnTestLogs()
    {
        for (int y = 0; y < gridManager.height; y++)
        {
            if (gridManager.GetLaneType(y) != LaneType.River)
            {
                continue;
            }

            int x = gridManager.width / 2;

            Vector2 spawnPosition = gridManager.GetWorldPosition(x, y);

            GameObject log = Instantiate(logPrefab, spawnPosition, Quaternion.identity);

            LogMovement logMovement = log.GetComponent<LogMovement>();

            if (logMovement != null)
            {
                logMovement.SetGridManager(gridManager);
                logMovement.SetMovement(gridManager.GetLaneSettings(y).speed, gridManager.GetLaneSettings(y).direction);
            }
        }
    }

    // Finds the log furthest left within the "just entering" zone, excluding
    // whichever log was used last time (so we never re-teleport onto the same log).
    private GameObject FindLogEnteringFromLeft()
    {
        GameObject[] allLogs = GameObject.FindGameObjectsWithTag("Log");
        if (allLogs.Length == 0)
        {
            return null;
        }

        GameObject best = null;
        float bestX = float.MaxValue;

        foreach (GameObject log in allLogs)
        {
            if (log == lastUsedLog)
            {
                continue; // don't immediately reuse the same log
            }

            float x = log.transform.position.x;
            if (x >= leftEdgeX && x <= leftEdgeX + leftEdgeZoneWidth && x < bestX)
            {
                bestX = x;
                best = log;
            }
        }

        return best;
    }

    private void SpawnBlueFrogOnLog(GameObject log)
    {
        if (blueFrogPrefab == null)
        {
            Debug.LogError("Blue frog prefab not assigned!");
            return;
        }

        ExtraLifeFrog blueFrog = Instantiate(blueFrogPrefab, log.transform.position, Quaternion.identity);
        blueFrog.SetLog(log);

        currentBlueFrog = blueFrog;
        lastUsedLog = log;
        lifetimeTimer = blueFrogLifetime;

        Debug.Log("Blue frog spawned on a log entering from the left.");
    }

    // Handy for testing: bypass the left-edge check and just use whatever log is available.
    public void ForceSpawnBlueFrog()
    {
        GameObject log = FindLogEnteringFromLeft();
        if (log == null)
        {
            GameObject[] allLogs = GameObject.FindGameObjectsWithTag("Log");
            if (allLogs.Length > 0)
            {
                log = allLogs[Random.Range(0, allLogs.Length)];
            }
        }

        if (log != null)
        {
            if (currentBlueFrog != null)
            {
                Destroy(currentBlueFrog.gameObject);
            }

            SpawnBlueFrogOnLog(log);
        }
    }

    private void CreateLaneSpawners()
    {
        for (int y = 0; y < gridManager.height; y++)
        {
            if (gridManager.GetLaneType(y) != LaneType.River)
            {
                continue;
            }

            laneSpawners.Add(new RiverSpawner(y));
        }
    }

    private void UpdateLogSpawning()
    {
        foreach (RiverSpawner lane in laneSpawners)
        {
            lane.timer += Time.deltaTime;

            var settings = gridManager.GetLaneSettings(lane.y);

            if (lane.nextSpawnTime < 0f)
            {
                lane.nextSpawnTime = Random.Range(
                    Mathf.Max(0.1f, settings.logSpawnInterval - settings.logSpawnIntervalVariation), settings.logSpawnInterval + settings.logSpawnIntervalVariation);
            }

            if (lane.timer >= lane.nextSpawnTime &&
                lane.activeLogs < settings.maxLogs)
            {
                SpawnLog(lane.y);

                lane.activeLogs++;

                float variation = settings.logSpawnIntervalVariation;

                float minInterval = Mathf.Max(0.1f, settings.logSpawnInterval - variation);

                float maxInterval = settings.logSpawnInterval + variation;

                lane.nextSpawnTime = Random.Range(minInterval, maxInterval);

                lane.timer = 0f;
            }
        }
    }

    private void SpawnLog(int y)
    {
        var settings = gridManager.GetLaneSettings(y);

        float leftEdge = gridManager.GetWorldPosition(0, y).x;
        float rightEdge = gridManager.GetWorldPosition(gridManager.width - 1, y).x;

        float spawnOffset = 1f;

        float spawnX;

        if (settings.logDirection > 0)
        {
            // Log moving right so enter from the left
            spawnX = leftEdge - spawnOffset;
        }
        else
        {
            spawnX = rightEdge + spawnOffset;
        }

        float spawnY = gridManager.GetWorldPosition(0, y).y;

        Vector2 spawnPosition = new Vector2(spawnX, spawnY);

        GameObject log = Instantiate(logPrefab, spawnPosition, Quaternion.identity);

        LogMovement logMovement = log.GetComponent<LogMovement>();

        if (logMovement != null)
        {
            logMovement.SetGridManager(gridManager);

            logMovement.SetMovement(settings.logSpeed, settings.logDirection);
        }
    }
}