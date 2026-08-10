using UnityEngine;
using static GridPositionTypes;
using static LaneTypes;

[System.Serializable]
public class GridRow
{
    public LaneType laneType;
    public GridPositionType[] positions;
}  

