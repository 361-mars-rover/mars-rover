using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MarsGlobalTerrain : MonoBehaviour {
    public Terrain terrain;
    private string baseURL = "https://trek.nasa.gov/tiles/Mars/EQ/Mars_Viking_MDIM21_ClrMosaic_global_232m/1.0.0/default/default028mm/0/0/0.jpg";
    
    [Header("Terrain Settings")]
    public float heightScale = 0.2f;
    public int blurIterations = 2;

    void Start() {
        if (terrain == null) {
            Debug.LogError("Terrain reference not set!");
            return;
        }
        ConfigureTerrainSize();
        StartCoroutine(DownloadHeightmap(0, 0, 0));
    }


    void ConfigureTerrainSize() {
        // Set terrain size to match Mars' elevation range
        terrain.terrainData.size = new Vector3(
            terrain.terrainData.size.x,
            29000f, // Max elevation range (21km - (-8km))
            terrain.terrainData.size.z
        );
    }

    IEnumerator DownloadHeightmap(int z, int x, int y) {
        string url = string.Format(baseURL, z, x, y);
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            ApplyHeightmap(texture);
        } else {
            Debug.LogError("Failed to download heightmap: " + request.error);
        }
    }

    void ApplyHeightmap(Texture2D texture) {
        TerrainData terrainData = terrain.terrainData;
        int resolution = terrainData.heightmapResolution;
        float[,] heights = new float[resolution, resolution];
        Color[] pixels = texture.GetPixels();

        float minElevation = -100f;
        float maxElevation = 500f;
        float elevationRange = maxElevation - minElevation;

        // Load raw heights
        for (int y = 0; y < resolution; y++) {
            int texY = (int)((y / (float)resolution) * texture.height);
            for (int x = 0; x < resolution; x++) {
                int texX = (int)((x / (float)resolution) * texture.width);
                Color pixel = pixels[texY * texture.width + texX];
                float heightValue = pixel.r * elevationRange + minElevation;
                heights[y, x] = Mathf.Clamp01(heightValue / terrainData.size.y * heightScale);
            }
        }

        // Apply smoothing
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
                        heights[y, x+1] + 
                        heights[y, x-1] + 
                        heights[y+1, x] + 
                        heights[y-1, x]
                    ) / 5f;
                    heights[y, x] = avg;
                }
            }
        }
        return heights;
    }
}
