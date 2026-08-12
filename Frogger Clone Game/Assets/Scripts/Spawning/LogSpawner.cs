using UnityEngine;
using static LaneTypes;

public class LogSpawner : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject logPrefab;

    private void Awake()
    {
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();

        }

        SpawnTestLogs();
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
}
