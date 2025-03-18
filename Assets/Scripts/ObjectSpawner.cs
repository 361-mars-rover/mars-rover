using UnityEngine;
using System.Collections;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject objectPrefab;  
    public int numberOfObjects = 1000; 
    public float spawnRadius = 500f;
    public float spawnDelay = 1f; 

    public float scaleMin;
    public float scaleMax;


    void Start()
    {
        StartCoroutine(DelayedSpawn());
    }

    IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(spawnDelay);
        SpawnObjectss();
    }

    void SpawnObjectss()
    {
        for (int i = 0; i < numberOfObjects; i++)
        {
            Vector3 randomPosition = GetRandomPositionNearTerrain();
            if (randomPosition != Vector3.zero)
            {
                GameObject newObject = Instantiate(objectPrefab, randomPosition, Quaternion.identity);   

                // Renderer rend = newObject.GetComponent<Renderer>();
                // rend.material.color = Color.red; 

                newObject.transform.rotation = Quaternion.Euler(
                    Random.Range(0, 360),
                    Random.Range(0, 360),
                    Random.Range(0, 360)
                );

                // float scaleFactor = Random.Range(1f, 10f);
                float scaleFactor = Random.Range(scaleMin, scaleMax);
                newObject.transform.localScale *= scaleFactor;
            }
        }
    }

    Vector3 GetRandomPositionNearTerrain()
    {
        // Generate position around the spawner
        Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(5f, spawnRadius);
        Vector3 spawnPosition = new Vector3(
            transform.position.x + randomCircle.x,
            0,
            transform.position.z + randomCircle.y
        );

        // Find the correct terrain chunk
        foreach (Terrain terrain in Terrain.activeTerrains)
        {
            // Critical null check added
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

        Debug.LogWarning("No valid terrain found for Objects spawn");
        return Vector3.zero;
    }
}