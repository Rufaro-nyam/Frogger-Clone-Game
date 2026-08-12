using UnityEngine;

public class RoadSpawner
{
    public int y;
    public float timer;

    public RoadSpawner(int laneY)
    {
        y = laneY;
        timer = 0f;
    }
}
