using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class MarsGlobalTerrain : MonoBehaviour {

    // References
    public Terrain terrain; // Current terrain tile
    public Transform car;   // Car object

    // API settings
    private string baseURL = "https://trek.nasa.gov/tiles/Mars/EQ/Mars_MOLA_blend200ppx_HRSC_Shade_clon0dd_200mpp_lzw/1.0.0/default/default028mm/8/{0}/{1}.jpg";

    [Header("Terrain Settings")]
    public float heightScale = 1f;       // Scale for terrain height
    public int blurIterations = 2;       // Number of smoothing iterations

    [Header("Car Starting Position")]
    [Range(0, 1)] public float startX = 0.1f; // Starting X position (normalized)
    [Range(0, 1)] public float startZ = 0.1f; // Starting Z position (normalized)

    // Constants
    private const float TILE_SIZE_KM = 100; // Size of each tile in kilometers
    private const float MIN_ELEVATION = -8000f; // Lowest point on Mars
    private const float MAX_ELEVATION = 21000f; // Highest point on Mars
    private const float ELEVATION_RANGE = MAX_ELEVATION - MIN_ELEVATION;

    // Tile coordinates
    private int x = 102; // Current X coordinate
    private int y = 214; // Current Y coordinate
    private int z = 8;   // Zoom level (fixed)

    void Start() {
        if (terrain == null) {
            Debug.LogError("Terrain reference not set!");
            return;
        }
        if (car == null) {
            Debug.LogError("Car reference not set!");
            return;
        }

        // Configure the initial terrain
        ConfigureTerrainSize(terrain.terrainData);

        // Download and apply the heightmap for the initial tile
        StartCoroutine(DownloadHeightmap(terrain, z, x, y));

        // Position the car on the initial tile
        PositionCarAtStart();
    }

    void Update() {
        if (car != null) {
            CheckCarPosition();
        }
    }

    void CheckCarPosition() {
        Vector3 carPosition = car.position;
        Vector3 terrainPosition = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;

        // Check if the car is near the edge of the current tile
        if (carPosition.x > terrainPosition.x + terrainSize.x * 0.9f) {
            //LoadNextTile(1, 0); // Load tile to the right
        } else if (carPosition.x < terrainPosition.x + terrainSize.x * 0.1f) {
            //LoadNextTile(-1, 0); // Load tile to the left
        }

        if (carPosition.z > terrainPosition.z + terrainSize.z * 0.9f) {
            //LoadNextTile(0, 1); // Load tile above
        } else if (carPosition.z < terrainPosition.z + terrainSize.z * 0.1f) {
            //LoadNextTile(0, -1); // Load tile below
        }
    }

    void LoadNextTile(int deltaX, int deltaY) {
        // Update the tile coordinates
        x += deltaX;
        y += deltaY;

        // Clamp x and y to valid ranges (0-512 for x, 0-256 for y)
        x = Mathf.Clamp(x, 0, 512);
        y = Mathf.Clamp(y, 0, 256);

        // Create a new terrain for the next tile
        CreateNewTerrainTile(x, y, deltaX, deltaY);
    }

    void CreateNewTerrainTile(int newX, int newY, int deltaX, int deltaY) {
        // Create a new GameObject for the terrain
        GameObject newTerrainObj = new GameObject("TerrainTile");
        Terrain newTerrain = newTerrainObj.AddComponent<Terrain>();
        TerrainCollider newTerrainCollider = newTerrainObj.AddComponent<TerrainCollider>();

        // Configure the new terrain
        newTerrain.terrainData = new TerrainData();
        newTerrainCollider.terrainData = newTerrain.terrainData;
        ConfigureTerrainSize(newTerrain.terrainData);

        // Position the new terrain adjacent to the current one
        Vector3 newPosition = terrain.transform.position;
        newPosition.x += deltaX * TILE_SIZE_KM * 1000f;
        newPosition.z += deltaY * TILE_SIZE_KM * 1000f;
        newTerrainObj.transform.position = newPosition;

        // Download and apply the heightmap for the new terrain
        StartCoroutine(DownloadHeightmap(newTerrain, z, newX, newY));

        // Update the car's position to match the new terrain
        PositionCarOnNewTerrain(newTerrain);
    }

    IEnumerator DownloadHeightmap(Terrain targetTerrain, int z, int x, int y) {
        string url = string.Format(baseURL, x, y);
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            ApplyHeightmap(targetTerrain, texture);
        } else {
            Debug.LogError("Failed to download heightmap: " + request.error);
        }
    }

    void ApplyHeightmap(Terrain targetTerrain, Texture2D texture) {
        TerrainData terrainData = targetTerrain.terrainData;
        int resolution = terrainData.heightmapResolution;
        float[,] heights = new float[resolution, resolution];
        Color[] pixels = texture.GetPixels();

        for (int y = 0; y < resolution; y++) {
            int texY = (int)((y / (float)resolution) * texture.height);
            for (int x = 0; x < resolution; x++) {
                int texX = (int)((x / (float)resolution) * texture.width);
                Color pixel = pixels[texY * texture.width + texX];

                float heightValue = pixel.r * ELEVATION_RANGE + MIN_ELEVATION;
                heights[y, x] = Mathf.Clamp01((heightValue - MIN_ELEVATION) / ELEVATION_RANGE * heightScale);
            }
        }

        heights = SmoothHeights(heights, resolution, blurIterations);
        terrainData.SetHeights(0, 0, heights);
    }

    void ConfigureTerrainSize(TerrainData terrainData) {
        float sizeMeters = TILE_SIZE_KM * 1000f;
        terrainData.size = new Vector3(sizeMeters, ELEVATION_RANGE, sizeMeters);
    }

    void PositionCarAtStart() {
        if (car == null) {
            Debug.LogError("Car reference not set!");
            return;
        }

        // Get the terrain's size in world units
        Vector3 terrainSize = terrain.terrainData.size;

        // Calculate the car's starting position in world coordinates
        float carX = terrain.transform.position.x + startX * terrainSize.x;
        float carZ = terrain.transform.position.z + startZ * terrainSize.z;

        // Get the height at the starting position to place the car on the terrain
        float carY = terrain.SampleHeight(new Vector3(carX, 0, carZ)) + terrain.transform.position.y;

        // Set the car's position
        car.position = new Vector3(carX + 20, carY + 5, carZ + 20);

        Debug.Log($"Car positioned at: X={carX}, Y={carY}, Z={carZ}");
    }

    void PositionCarOnNewTerrain(Terrain newTerrain) {
        if (car == null) {
            Debug.LogError("Car reference not set!");
            return;
        }

        // Get the new terrain's position and size
        Vector3 newTerrainPosition = newTerrain.transform.position;
        Vector3 newTerrainSize = newTerrain.terrainData.size;

        // Calculate the car's position relative to the new terrain
        float carX = newTerrainPosition.x + startX * newTerrainSize.x;
        float carZ = newTerrainPosition.z + startZ * newTerrainSize.z;

        // Get the height at the new position
        float carY = newTerrain.SampleHeight(new Vector3(carX, 0, carZ)) + newTerrain.transform.position.y;

        // Set the car's position
        car.position = new Vector3(carX, carY, carZ);
    }

    float[,] SmoothHeights(float[,] heights, int resolution, int iterations) {
        for (int i = 0; i < iterations; i++) {
            for (int y = 1; y < resolution - 1; y++) {
                for (int x = 1; x < resolution - 1; x++) {
                    float avg = (
                        heights[y, x] +
                        heights[y, x + 1] +
                        heights[y, x - 1] +
                        heights[y + 1, x] +
                        heights[y - 1, x]
                    ) / 5f;
                    heights[y, x] = avg;
                }
            }
        }
        return heights;
    }
}