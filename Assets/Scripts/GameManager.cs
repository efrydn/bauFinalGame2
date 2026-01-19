using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject mapPrefab;
    public GameObject winPrefab; // Manually assign this!
    public Transform playerTransform;
    
    private float spawnX = 0.0f;      
    [Header("Generation Settings")]
    public float tileLength = 50.0f; 
    public int numberOfTiles = 4;   
    
    [Header("Level Settings")]
    public bool infiniteMode = true; // Default to true for infinite runner
    public int totalTilesToSpawn = 10;
    public int safeTilesCount = 3; 
    private int spawnedTilesCount = 0;
    private bool levelCompleted = false;
    
    private List<GameObject> activeTiles = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < numberOfTiles; i++)
        {
            if (infiniteMode || spawnedTilesCount < totalTilesToSpawn)
            {
                SpawnTile(false);
            }
        }
    }

    // Update is unchanged...

    void Update()
    {
       if (!levelCompleted && playerTransform.position.x > spawnX - (numberOfTiles * tileLength) + 30)
        {
            if (infiniteMode || spawnedTilesCount < totalTilesToSpawn)
            {
                SpawnTile(false);
                DeleteTile();
            }
            else if (!infiniteMode && spawnedTilesCount == totalTilesToSpawn)
            {
                SpawnTile(true); // Spawn Win Tile
                DeleteTile();
                levelCompleted = true; // Stop spawning
            }
        }
    }

    void SpawnTile(bool isWinTile)
    {
        GameObject prefabToSpawn = isWinTile && winPrefab != null ? winPrefab : mapPrefab;
        if (prefabToSpawn == null) return; // Safety check

        GameObject go = Instantiate(prefabToSpawn, Vector3.right * spawnX, Quaternion.identity); 
        activeTiles.Add(go);
        
        // Handle Tile Generation
        if (!isWinTile)
        {
            TileManager tileManager = go.GetComponent<TileManager>();
            if (tileManager != null)
            {
                // Only spawn obstacles if passed the safe zone
                if (spawnedTilesCount >= safeTilesCount)
                {
                    tileManager.SpawnObstacles();
                }
            }
        }
        
        spawnX += tileLength;
        spawnedTilesCount++;
    }

    void DeleteTile()
    {
        if (activeTiles.Count > 0)
        {
            Destroy(activeTiles[0]);
            activeTiles.RemoveAt(0);
        }
    }
}