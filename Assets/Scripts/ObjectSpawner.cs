using UnityEngine;
using System.Collections;

public class ObjectSpawner : MonoBehaviour
{
    // Existing variables
    public GameObject objectPrefab;
    public int numberOfObjects = 1000;
    public float spawnRadius = 500f;
    public float spawnDelay = 1f;
    public float scaleMin;
    public float scaleMax;
    
    // New mineral system variables
    public Texture2D mineralTexture;
    public float spawnThreshold = 0.5f;
    public AnimationCurve densityCurve;
    public float maxSpawnAttempts = 10000;

    private Terrain targetTerrain;
    private Vector3 terrainPosition;
    private Vector3 terrainSize;

    IEnumerator Start()
    {
        // Wait for terrain and texture initialization
        while (mineralTexture == null || Terrain.activeTerrain == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        targetTerrain = Terrain.activeTerrain;
        terrainPosition = targetTerrain.transform.position;
        terrainSize = targetTerrain.terrainData.size;

        Vector3 centerPos = terrainPosition + terrainSize/2;
        centerPos.y = targetTerrain.SampleHeight(centerPos);
        SpawnMineralObject(centerPos);
        
        // Test spawn at random position
        Vector3 randomPos = GetRandomPositionNearTerrain();
        if(randomPos != Vector3.zero) SpawnMineralObject(randomPos);
        
        StartCoroutine(DelayedSpawn());
    }

    IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(spawnDelay);
        SpawnMinerals();
    }

    void SpawnMinerals()
    {
        if (mineralTexture == null)
        {
            Debug.LogError("Mineral texture is not assigned!");
            return;
        }

        if (targetTerrain == null)
        {
            Debug.LogError("No active terrain found!");
            return;
        }
        int spawnedCount = 0;
        int attempts = 0;

        while (spawnedCount < numberOfObjects && attempts < maxSpawnAttempts)
        {
            Vector3 randomPosition = GetRandomPositionNearTerrain();
            if (randomPosition != Vector3.zero && IsGoodMineralPosition(randomPosition))
            {
                SpawnMineralObject(randomPosition);
                spawnedCount++;
            }
            attempts++;
        }
        Debug.Log($"Spawned {spawnedCount} minerals");
    }

    // THIS IS YOUR EXISTING POSITION METHOD - CRUCIAL FOR TERRAIN MATCHING
    Vector3 GetRandomPositionNearTerrain()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(5f, spawnRadius);
        Vector3 spawnPosition = new Vector3(
            transform.position.x + randomCircle.x,
            0,
            transform.position.z + randomCircle.y
        );

        foreach (Terrain terrain in Terrain.activeTerrains)
        {
            if (terrain == null || terrain.terrainData == null) continue;

            Vector3 terrainPos = terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;
            
            if (spawnPosition.x >= terrainPos.x && 
                spawnPosition.x <= terrainPos.x + terrainSize.x &&
                spawnPosition.z >= terrainPos.z && 
                spawnPosition.z <= terrainPos.z + terrainSize.z)
            {
                spawnPosition.y = terrain.SampleHeight(spawnPosition);
                return spawnPosition;
            }
        }
        return Vector3.zero;
    }

    bool IsGoodMineralPosition(Vector3 worldPos)
    {
        if (worldPos.x < terrainPosition.x || worldPos.x > terrainPosition.x + terrainSize.x ||
            worldPos.z < terrainPosition.z || worldPos.z > terrainPosition.z + terrainSize.z)
        {
            Debug.LogWarning($"Position out of bounds: {worldPos}");
            return false;
        }

        Vector2 uv = new Vector2(
            Mathf.Clamp01((worldPos.x - terrainPosition.x) / terrainSize.x),
            Mathf.Clamp01((worldPos.z - terrainPosition.z) / terrainSize.z)
        );

        // Debug sample values
        Color pixel = mineralTexture.GetPixelBilinear(uv.x, uv.y);
        Debug.Log($"Sampled color at {uv}: {pixel}");
        float concentration = pixel.r;

        // Use animation curve to map concentration to probability
        float spawnProbability = densityCurve.Evaluate(concentration);
        
        return Random.value < spawnProbability && concentration > spawnThreshold;
    }

    void SpawnMineralObject(Vector3 position)
    {
        GameObject newObject = Instantiate(objectPrefab, position, Quaternion.identity);
        
        // Set random rotation
        newObject.transform.rotation = Quaternion.Euler(
            Random.Range(0, 360),
            Random.Range(0, 360),
            Random.Range(0, 360)
        );

        // Set scale based on mineral concentration
        float mineralIntensity = GetMineralIntensity(position);
        float scaleFactor = Mathf.Lerp(scaleMin, scaleMax, mineralIntensity);
        newObject.transform.localScale = Vector3.one * scaleFactor;
    }

    float GetMineralIntensity(Vector3 worldPos)
    {
        Vector2 uv = new Vector2(
            (worldPos.x - terrainPosition.x) / terrainSize.x,
            (worldPos.z - terrainPosition.z) / terrainSize.z
        );
        return mineralTexture.GetPixelBilinear(uv.x, uv.y).r;
    }

    // Keep existing GetRandomPositionNearTerrain method
}