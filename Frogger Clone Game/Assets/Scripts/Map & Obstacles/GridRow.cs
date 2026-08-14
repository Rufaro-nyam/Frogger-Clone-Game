using UnityEngine;
using static GridPositionTypes;
using static LaneTypes;

[System.Serializable]
public class GridRow
{
    public LaneType laneType;
    public GridPositionType[] positions;

    [Header("Road Settings")]
    public int trafficDirection = 1;
    public float trafficSpeed = 2f;
    public float spawnInterval = 3f;
    public float spawnIntervalVariation = 1f;
    public int maxCars = 2;

    [Header("River Settings")]
    public int logDirection = 1;
    public float logSpeed = 2f;
    public float logSpawnInterval = 3f;
    public float logSpawnIntervalVariation = 1f;
    public int maxLogs = 2;
}  

