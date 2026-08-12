using UnityEngine;
using static LaneTypes;

public class CarSpawner : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject carPrefab;

    private void Awake()
    {
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();

        }

        SpawnTestCars();
    }

    private void SpawnTestCars()
    {
        for (int y = 0; y < gridManager.height; y++)
        {
            if (gridManager.GetLaneType(y) != LaneType.Road)
            {
                continue;
            }

            // Spawn one car in the middle of each Road lane
            int x = gridManager.width / 2;

            Vector2 spawnPosition = gridManager.GetWorldPosition(x, y);

            GameObject car = Instantiate(carPrefab, spawnPosition, Quaternion.identity
);

            CarMovement carMovement = car.GetComponent<CarMovement>();


            if (carMovement != null)
            {
                carMovement.SetGridManager(gridManager);
                carMovement.SetMovement(gridManager.GetLaneSettings(y).speed, gridManager.GetLaneSettings(y).direction);
            }
        }
    }
}
