using UnityEngine;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    [Header("Assignments")]
    public List<GameObject> obstaclePrefabs = new List<GameObject>(); 
    public List<GameObject> triggerPrefabs = new List<GameObject>(); // SpeedUp, SlowDown, Stop, etc.

    [Header("Settings")]
    public int numberOfObstacles = 3; 
    public float worldLaneDistance = 1f;
    [Range(0f, 1f)] public float triggerChance = 0.3f; // 30% chance to spawn a trigger instead of an obstacle

    void Start()
    {
        // Removed Start to allow GameManager to control spawning
    }

    public void SpawnObstacles()
    {
        Vector3 parentScale = transform.localScale;
        List<int> occupiedLanes = new List<int>();

        for (int i = 0; i < numberOfObstacles; i++)
        {
            float randomXRatio = Random.Range(-0.4f, 0.4f); 
            
            // Try to find a free lane
            int lane = 0;
            int attempts = 0;
            bool laneFound = false;

            while (attempts < 10)
            {
                lane = Random.Range(-1, 2);
                if (!occupiedLanes.Contains(lane))
                {
                    laneFound = true;
                    break;
                }
                attempts++;
            }

            if (!laneFound) continue; // Skip this obstacle if no lane found
            
            occupiedLanes.Add(lane);
            
            float localZ = (lane * worldLaneDistance) / parentScale.z;

            // Decide whether to spawn an obstacle or a trigger
            GameObject prefabToSpawn = null;

            if (Random.value < triggerChance && triggerPrefabs.Count > 0)
            {
                prefabToSpawn = triggerPrefabs[Random.Range(0, triggerPrefabs.Count)];
            }
            else if (obstaclePrefabs.Count > 0)
            {
                prefabToSpawn = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
            }

            if (prefabToSpawn != null)
            {
                Vector3 spawnLocalPos = new Vector3(randomXRatio, 1f / parentScale.y, localZ);

                GameObject spawnedObj = Instantiate(prefabToSpawn, transform);
                spawnedObj.transform.localPosition = spawnLocalPos;

                // Revert strict 1x1x1 scale relative to parent
                spawnedObj.transform.localScale = new Vector3(
                    1f / parentScale.x,
                    1f / parentScale.y,
                    1f / parentScale.z 
                );
            }
        }
    }
}