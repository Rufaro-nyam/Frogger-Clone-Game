using UnityEngine;

public class RiverSpawner
{
    public int y;
    public float timer;
    public float nextSpawnTime;
    public int activeLogs;

    public RiverSpawner(int laneY)
    {
        y = laneY;
        timer = 0f;
        nextSpawnTime = -1f;
        activeLogs = 0;
    }
}
