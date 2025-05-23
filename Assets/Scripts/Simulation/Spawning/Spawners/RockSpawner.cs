// using UnityEngine;
// using System.Collections;

// class RockSpawner : MonoBehaviour
// {
//     public GameObject rockPrefab;  
//     private int numberOfRocks = 500; 
//     private float spawnRadius = 2000f;
//     private float spawnDelay = 1f; 

//     void Start()
//     {
//         StartCoroutine(DelayedSpawn());
//     }

//     IEnumerator DelayedSpawn()
//     {
//         yield return new WaitForSeconds(spawnDelay);
//         SpawnRocks();
//     }

//     void SpawnRocks()
//     {
//         for (int i = 0; i < numberOfRocks; i++)
//         {
//             Vector3 randomPosition = GetRandomPositionNearTerrain();
//             if (randomPosition != Vector3.zero)
//             {
//                 GameObject rock = Instantiate(rockPrefab, randomPosition, Quaternion.identity);
//                 rock.transform.SetParent(transform.parent);    

//                 rock.transform.rotation = Quaternion.Euler(
//                     Random.Range(0, 360),
//                     Random.Range(0, 360),
//                     Random.Range(0, 360)
//                 );

//                 float scaleFactor = Random.Range(0.01f, 4f);
//                 rock.transform.localScale *= scaleFactor;
//             }
//         }
//     }

//     Vector3 GetRandomPositionNearTerrain()
//     {
//         // Generate position around the spawner
//         Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(5f, spawnRadius);
//         Vector3 spawnPosition = new Vector3(
//             transform.position.x + randomCircle.x,
//             0,
//             transform.position.z + randomCircle.y
//         );

//         // Find the correct terrain chunk
//         foreach (Terrain terrain in Terrain.activeTerrains)
//         {
//             // Critical null check added
//             if (terrain == null || terrain.terrainData == null) continue;

//             Vector3 terrainPos = terrain.transform.position;
//             Vector3 terrainSize = terrain.terrainData.size;
            
//             if (spawnPosition.x >= terrainPos.x && 
//                 spawnPosition.x <= terrainPos.x + terrainSize.x &&
//                 spawnPosition.z >= terrainPos.z && 
//                 spawnPosition.z <= terrainPos.z + terrainSize.z)
//             {
//                 spawnPosition.y = terrain.SampleHeight(spawnPosition);
//                 return spawnPosition;
//             }
//         }

//         // Debug.LogWarning("No valid terrain found for rock spawn");
//         return Vector3.zero;
//     }
// }

using UnityEngine;
using System.Collections;

class RockSpawner : Spawner
{
    public GameObject rockPrefab; 
    public GameObject car;
    private float exclusionRadius = 50f; 
    private int numberOfRocks = 500; 
    private float spawnRadius = 2000f;
    private float spawnDelay = 1f; 

    public override void Spawn()
    {
        Vector3 carPosition = car.transform.position;

        for (int i = 0; i < numberOfRocks; i++)
        {
            Vector3 randomPosition = GetRandomPositionNearTerrain();
            if (randomPosition != Vector3.zero)
            {
                if (Vector3.Distance(randomPosition, carPosition) < exclusionRadius)
                {
                    i--; 
                    continue;
                }

                GameObject rock = Instantiate(rockPrefab, randomPosition, Quaternion.identity);
                rock.transform.SetParent(transform.parent);    

                rock.transform.rotation = Quaternion.Euler(
                    Random.Range(0, 360),
                    Random.Range(0, 360),
                    Random.Range(0, 360)
                );

                float scaleFactor = Random.Range(0.01f, 4f);
                rock.transform.localScale *= scaleFactor;
            }
        }
    }

    void Start()
    {
        StartCoroutine(DelayedSpawn());
    }

    IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(spawnDelay);
        Spawn();
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

        // Debug.LogWarning("No valid terrain found for rock spawn");
        return Vector3.zero;
    }
}