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

            var settings = gridManager.GetLaneSettings(lane.y);

            if (lane.timer >= lane.nextSpawnTime && lane.activeCars < settings.maxCars)
            {
                SpawnCar(lane.y);

                lane.activeCars++;

                float variation = settings.spawnIntervalVariation;

                lane.nextSpawnTime = Random.Range(
                    settings.spawnInterval - variation,
                    settings.spawnInterval + variation
                );

                lane.timer = 0f;
            }
        }
    }

    private void SpawnCar(int y)
    {
        var settings = gridManager.GetLaneSettings(y);

        float leftEdge = gridManager.GetWorldPosition(0, y).x;
        float rightEdge = gridManager.GetWorldPosition(gridManager.width - 1, y).x;

        float spawnOffset = 1f;

        float spawnX;

        if (settings.direction > 0)
        {
            // Car is moving right, so enter from the left
            spawnX = leftEdge - spawnOffset;
        }
        else
        {
            // Car is moving left, so enter from the right
            spawnX = rightEdge + spawnOffset;
        }

        float spawnY = gridManager.GetWorldPosition(0, y).y;

        Vector2 spawnPosition = new Vector2(spawnX, spawnY);

        GameObject car = Instantiate(carPrefab, spawnPosition, Quaternion.identity);

        CarMovement carMovement = car.GetComponent<CarMovement>();

        if (carMovement != null)
        {
            carMovement.SetGridManager(gridManager);

            carMovement.SetMovement(settings.speed, settings.direction);
        }
    }
}
