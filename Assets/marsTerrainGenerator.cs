using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MarsGlobalTerrain : MonoBehaviour {

    //terrain type input
    public Terrain terrain;
    // mars API url (in future make x and y variables to change depending on cars location)
    private string baseURL = "https://trek.nasa.gov/tiles/Mars/EQ/Mars_MOLA_blend200ppx_HRSC_Shade_clon0dd_200mpp_lzw/1.0.0/default/default028mm/8/102/214.jpg";
    
    [Header("Terrain Settings")]

    // temp scales to get map to look good, to be changed
    public float heightScale = 1f;
    public int blurIterations = 2;
    // Real size of tiles
    private const float TILE_SIZE_KM = 100;
    private const float MIN_ELEVATION = -8000f;  // Lowest point on Mars
    private const float MAX_ELEVATION = 21000f;  // Olympus Mons peak
    private const float ELEVATION_RANGE = MAX_ELEVATION - MIN_ELEVATION;

    // start function, checks terrain input and calls other functions
    void Start() {
        if (terrain == null) {
            Debug.LogError("Terrain reference not set!");
            return;
        }
        ConfigureTerrainSize();
        StartCoroutine(DownloadHeightmap(0, 0, 0));
    }


    // this changes the terrain size to map the mars real size
    void ConfigureTerrainSize() {
        // Convert tile size from km to meters (1 km = 1000 meters)
        float sizeMeters = TILE_SIZE_KM * 1000f;

        // Set terrain size to match real Mars dimensions
        terrain.terrainData.size = new Vector3(sizeMeters, ELEVATION_RANGE, sizeMeters);
    }

    // z, x, y currently usuless (for rendering later)
    IEnumerator DownloadHeightmap(int z, int x, int y) {
        string url = string.Format(baseURL, z, x, y);
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        //applies API texture to terrain
        if (request.result == UnityWebRequest.Result.Success) {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            ApplyHeightmap(texture);
        } else {
            Debug.LogError("Failed to download heightmap: " + request.error);
        }
    }

    
    void ApplyHeightmap(Texture2D texture) {
        // This block of code gets our terrain inputs current data
        TerrainData terrainData = terrain.terrainData;
        int resolution = terrainData.heightmapResolution;
        float[,] heights = new float[resolution, resolution];
        //each pixel color represents the height
        Color[] pixels = texture.GetPixels();

        // loop through the resolution grid pixel by pixel
        for (int y = 0; y < resolution; y++) {
            int texY = (int)((y / (float)resolution) * texture.height);
            for (int x = 0; x < resolution; x++) {
                int texX = (int)((x / (float)resolution) * texture.width);
                Color pixel = pixels[texY * texture.width + texX];

                float heightValue = pixel.r * ELEVATION_RANGE + MIN_ELEVATION;

                // Normalize to Unity terrain height range
                heights[y, x] = Mathf.Clamp01((heightValue - MIN_ELEVATION) / ELEVATION_RANGE * heightScale);
            }
        }

        heights = SmoothHeights(heights, resolution, blurIterations);

        terrainData.SetHeights(0, 0, heights);
    }

    float[,] SmoothHeights(float[,] heights, int resolution, int iterations) {
        for (int i = 0; i < iterations; i++) {
            for (int y = 1; y < resolution - 1; y++) {
                for (int x = 1; x < resolution - 1; x++) {
                    // Simple box blur
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