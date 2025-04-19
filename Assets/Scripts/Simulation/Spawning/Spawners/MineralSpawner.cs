// using UnityEngine;
// using System.Collections;
// using UnityEngine.Networking;

// public class MineralSpawner : MonoBehaviour
// {
//     [Header("Spawning Settings")]
//     public GameObject rockPrefab;
//     public int numberOfRocks = 1000;
//     public float spawnRadius = 500f;
//     public float spawnDelay = 1f;

//     [Header("Mineral Data")]
//     public Texture2D mineralTexture;
//     private Terrain targetTerrain;
//     private Vector3 terrainPosition;
//     private Vector3 terrainSize;

//     [Header("Spawn Probability")]
//     public AnimationCurve spawnProbabilityCurve = new AnimationCurve(
//         new Keyframe(0f, 0f),
//         new Keyframe(0.5f, 0.1f),
//         new Keyframe(1f, 1f)
//     );

//     IEnumerator Start()
//     {
//         // Wait for terrain to initialize
//         while (Terrain.activeTerrain == null)
//             yield return null;

//         // Get terrain reference
//         targetTerrain = Terrain.activeTerrain;
//         terrainPosition = targetTerrain.transform.position;
//         terrainSize = targetTerrain.terrainData.size;

//         // Download mineral texture first
//         yield return StartCoroutine(DownloadMineralTexture());

//         // Start spawning
//         StartCoroutine(DelayedSpawn());
//     }

//     IEnumerator DownloadMineralTexture()
//     {
//         string mineralURL = "https://trek.nasa.gov/tiles/Mars/EQ/TES_Plagioclase/1.0.0/default/default028mm/0/0/1.png";
//         using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(mineralURL))
//         {
//             yield return request.SendWebRequest();
            
//             if (request.result == UnityWebRequest.Result.Success)
//             {
//                 mineralTexture = DownloadHandlerTexture.GetContent(request);
//                 Debug.Log($"Loaded mineral texture: {mineralTexture.width}x{mineralTexture.height}");
//             }
//             else
//             {
//                 Debug.LogError($"Failed to load mineral texture: {request.error}");
//             }
//         }
//     }

//     IEnumerator DelayedSpawn()
//     {
//         yield return new WaitForSeconds(spawnDelay);
//         SpawnRocks();
//     }

//     void SpawnRocks()
//     {
//         if (mineralTexture == null)
//         {
//             Debug.LogError("No mineral texture loaded!");
//             return;
//         }

//         int spawned = 0;
//         int attempts = 0;
//         int maxAttempts = numberOfRocks * 5;

//         while (spawned < numberOfRocks && attempts < maxAttempts)
//         {
//             Vector3 randomPosition = GetRandomTerrainPosition();
            
//             if (randomPosition != Vector3.zero && CheckMineralValue(randomPosition))
//             {
//                 CreateRock(randomPosition);
//                 spawned++;
//             }
//             attempts++;
//         }

//         Debug.Log($"Spawned {spawned} rocks (attempts: {attempts})");
//     }

//     bool CheckMineralValue(Vector3 worldPosition)
//         {
//                 Vector2 uv = new Vector2(
//             (worldPosition.x - terrainPosition.x) / terrainSize.x,
//             (worldPosition.z - terrainPosition.z) / terrainSize.z
//         );
        
//         Color pixel = mineralTexture.GetPixelBilinear(uv.x, uv.y);
        
//         // Decode TES parameters (0-1 range)
//         float pyroxeneConcentration = pixel.r * 0.2f; // Scaled to 0-0.20 actual concentration
//         float albedoMask = pixel.g; // 1 = low albedo (dark regions)
        
//         // Apply scientific constraints from the paper
//         bool isEquatorialLowAlbedo = albedoMask > 0.7f; // Dark regions threshold
//         bool aboveDetectionLimit = pyroxeneConcentration >= 0.05f;
        
//         if (!isEquatorialLowAlbedo || !aboveDetectionLimit) 
//             return false;
            
//         // Convert concentration to spawn probability
//         float spawnProbability = Mathf.InverseLerp(0.05f, 0.2f, pyroxeneConcentration);
//         return Random.value < spawnProbability;
//     }

//     void CreateRock(Vector3 position)
//     {
//         GameObject rock = Instantiate(rockPrefab, position, Quaternion.identity);
//         rock.transform.SetParent(transform.parent);
//         rock.transform.rotation = Random.rotation;
//         rock.transform.localScale = Vector3.one * 0.2f;
//     }

//     Vector3 GetRandomTerrainPosition()
//     {
//         float randomX = Random.Range(terrainPosition.x, terrainPosition.x + terrainSize.x);
//         float randomZ = Random.Range(terrainPosition.z, terrainPosition.z + terrainSize.z);
        
//         Vector3 spawnPosition = new Vector3(
//             randomX,
//             0,
//             randomZ
//         );

//         spawnPosition.y = targetTerrain.SampleHeight(spawnPosition);
//         return spawnPosition;
//     }
// }

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MineralSpawner : Spawner
{
    [Header("Spawning Settings")]
    public GameObject rockPrefab;
    public int numberOfRocks = 1000;
    public float spawnRadius = 500f;
    public float spawnDelay = 1f;

    [Header("Mineral Data")]
    public Texture2D mineralTexture;
    private Terrain targetTerrain;
    private Vector3 terrainPosition;
    private Vector3 terrainSize;

    [Header("Spawn Probability")]
    public AnimationCurve spawnProbabilityCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 0.1f),
        new Keyframe(1f, 1f)
    );

    public override void Spawn()
    {
        if (mineralTexture == null)
        {
            Debug.LogError("No mineral texture loaded!");
            return;
        }

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = numberOfRocks * 5;

        while (spawned < numberOfRocks && attempts < maxAttempts)
        {
            Vector3 randomPosition = GetRandomTerrainPosition();
            
            if (randomPosition != Vector3.zero && CheckMineralValue(randomPosition))
            {
                CreateRock(randomPosition);
                spawned++;
            }
            attempts++;
        }

        Debug.Log($"Spawned {spawned} rocks (attempts: {attempts})");
    }


    IEnumerator Start()
    {
        // Wait for terrain to initialize
        while (Terrain.activeTerrain == null)
            yield return null;

        // Get terrain reference
        targetTerrain = Terrain.activeTerrain;
        terrainPosition = targetTerrain.transform.position;
        terrainSize = targetTerrain.terrainData.size;

        // Download mineral texture first
        yield return StartCoroutine(DownloadMineralTexture());

        // Start spawning
        StartCoroutine(DelayedSpawn());
    }

    private IEnumerator DownloadMineralTexture()
    {
        string mineralURL = "https://trek.nasa.gov/tiles/Mars/EQ/TES_Plagioclase/1.0.0/default/default028mm/0/0/1.png";
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(mineralURL))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                mineralTexture = DownloadHandlerTexture.GetContent(request);
                Debug.Log($"Loaded mineral texture: {mineralTexture.width}x{mineralTexture.height}");
            }
            else
            {
                Debug.LogError($"Failed to load mineral texture: {request.error}");
            }
        }
    }

    private IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(spawnDelay);
        Spawn();
    }
    private bool CheckMineralValue(Vector3 worldPosition)
        {
                Vector2 uv = new Vector2(
            (worldPosition.x - terrainPosition.x) / terrainSize.x,
            (worldPosition.z - terrainPosition.z) / terrainSize.z
        );
        
        Color pixel = mineralTexture.GetPixelBilinear(uv.x, uv.y);
        
        // Decode TES parameters (0-1 range)
        float pyroxeneConcentration = pixel.r * 0.2f; // Scaled to 0-0.20 actual concentration
        float albedoMask = pixel.g; // 1 = low albedo (dark regions)
        
        // Apply scientific constraints from the paper
        bool isEquatorialLowAlbedo = albedoMask > 0.7f; // Dark regions threshold
        bool aboveDetectionLimit = pyroxeneConcentration >= 0.05f;
        
        if (!isEquatorialLowAlbedo || !aboveDetectionLimit) 
            return false;
            
        // Convert concentration to spawn probability
        float spawnProbability = Mathf.InverseLerp(0.05f, 0.2f, pyroxeneConcentration);
        return Random.value < spawnProbability;
    }

    private void CreateRock(Vector3 position)
    {
        GameObject rock = Instantiate(rockPrefab, position, Quaternion.identity);
        rock.transform.SetParent(transform.parent);
        rock.transform.rotation = Random.rotation;
        rock.transform.localScale = Vector3.one * 0.2f;
    }

    private Vector3 GetRandomTerrainPosition()
    {
        float randomX = Random.Range(terrainPosition.x, terrainPosition.x + terrainSize.x);
        float randomZ = Random.Range(terrainPosition.z, terrainPosition.z + terrainSize.z);
        
        Vector3 spawnPosition = new Vector3(
            randomX,
            0,
            randomZ
        );

        spawnPosition.y = targetTerrain.SampleHeight(spawnPosition);
        return spawnPosition;
    }
}