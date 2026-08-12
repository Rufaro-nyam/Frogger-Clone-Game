using System.Collections.Generic;
using UnityEngine;
using static LaneTypes;

public class CarSpawner : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject carPrefab;

    private List<RoadSpawner> laneSpawners = new List<RoadSpawner>();

    private void Start()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        CreateLaneSpawners();
    }

    private void CreateLaneSpawners()
    {
        for (int y = 0; y < gridManager.height; y++)
        {
            if (gridManager.GetLaneType(y) != LaneType.Road)
                continue;

            laneSpawners.Add(new RoadSpawner(y));
        }
    }

    private void Update()
    {
        foreach (RoadSpawner lane in laneSpawners)
        {
            lane.timer += Time.deltaTime;

            float spawnInterval =
                gridManager.GetLaneSettings(lane.y).spawnInterval;

            if (lane.timer >= spawnInterval)
            {
                SpawnCar(lane.y);
                lane.timer = 0f;
            }
        }
    }

    private void SpawnCar(int y)
    {
        int x = gridManager.width / 2;

        Vector2 spawnPosition = gridManager.GetWorldPosition(x, y);

        GameObject car = Instantiate(
            carPrefab,
            spawnPosition,
            Quaternion.identity
        );

        CarMovement carMovement = car.GetComponent<CarMovement>();

        if (carMovement != null)
        {
            carMovement.SetGridManager(gridManager);

            var settings = gridManager.GetLaneSettings(y);

            carMovement.SetMovement(
                settings.speed,
                settings.direction
            );
        }
    }
}
