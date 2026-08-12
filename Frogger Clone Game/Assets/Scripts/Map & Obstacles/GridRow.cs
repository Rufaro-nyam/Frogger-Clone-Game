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
}  

