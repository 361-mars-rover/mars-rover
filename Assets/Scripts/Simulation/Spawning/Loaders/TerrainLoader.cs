using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Loaders;

class TerrainLoader : Loader
{
    private string heightbaseURL = "https://trek.nasa.gov/tiles/Mars/EQ/Mars_MOLA_blend200ppx_HRSC_Shade_clon0dd_200mpp_lzw/1.0.0/default/default028mm";
    private string colorbaseURL =  "https://trek.nasa.gov/tiles/Mars/EQ/Mars_Viking_MDIM21_ClrMosaic_global_232m/1.0.0/default/default028mm";
    private GameObject marsTerrain;
    private int blurIterations = 2;

    private int row;
    private int col;

    private Transform simulationRoot;

    public class Factory : MonoBehaviourFactory{
        public static TerrainLoader Create(int row, int col, Transform simulationRoot, GameObject marsTerrain, GameObject gameObject = null){
            Debug.Log("Creating a spawner");
            TerrainLoader tl = Create<TerrainLoader>(gameObject);
            tl.row = row;
            tl.col = col;
            tl.simulationRoot = simulationRoot;
            tl.marsTerrain = marsTerrain;
            return tl;
        }
    }

    public override void Load()
    {
        Debug.Log("Calling the spawn method");
        // 1. Create a fresh TerrainData
        TerrainData terrainData = new TerrainData
        {
            heightmapResolution = TerrainInfo.HEIGHTMAP_RESOLUTION,
            size = new Vector3(TerrainInfo.TERRAIN_WIDTH, TerrainInfo.ELEVATION_RANGE,  TerrainInfo.TERRAIN_LENGTH)
        };
        // 2. Attach it to the existing marsTerrain
        marsTerrain.GetComponent<Terrain>().terrainData = terrainData;
        marsTerrain.GetComponent<TerrainCollider>().terrainData = terrainData;

        // 3. Parent the terrain under the simulation root
        marsTerrain.transform.SetParent(simulationRoot, false);

        // 4. Place the terrain so that its local center is offset
        //    e.g. if you want it centered at local (0,0,0), offset by half.
        marsTerrain.transform.localPosition = new Vector3(-TerrainInfo.TERRAIN_LENGTH / 2, 0, -TerrainInfo.TERRAIN_WIDTH / 2);

        // Start the async loading of height/color data
        StartCoroutine(DownloadHeightmapAndColor(row, col));
    }
        string GetDownloadURL(string baseURL, int row, int col)
    {
        return $"{baseURL}/{TerrainInfo.TILE_MATRIX_SET}/{row}/{col}.jpg";
    }

    IEnumerator DownloadHeightmapAndColor(int row, int col)
    {
        Debug.Log($"Row col: {row}, {col}");
        string heightURL = GetDownloadURL(heightbaseURL, row, col);
        string colorURL  = GetDownloadURL(colorbaseURL,  row, col);

        UnityWebRequest heightRequest = UnityWebRequestTexture.GetTexture(heightURL);
        UnityWebRequest colorRequest  = UnityWebRequestTexture.GetTexture(colorURL);
        yield return heightRequest.SendWebRequest();
        yield return colorRequest.SendWebRequest();

        if (heightRequest.result == UnityWebRequest.Result.Success)
        {
            Texture2D heightTexture = DownloadHandlerTexture.GetContent(heightRequest);
            Texture2D colorTexture  = DownloadHandlerTexture.GetContent(colorRequest);
            ApplyHeightmap(heightTexture);
            ApplyColorMap(colorTexture);
        }
        else
        {
            Debug.LogError("Failed to download heightmap: " + heightRequest.error);
        }
        Debug.Log("Setting isLoaded to true");
        isLoaded = true;
    }

        void ApplyHeightmap(Texture2D texture)
    {
        int resolution = marsTerrain.GetComponent<Terrain>().terrainData.heightmapResolution;
        float[,] heights = new float[resolution, resolution];
        Color[] pixels = texture.GetPixels();

        for (int y = 0; y < resolution; y++)
        {
            int texY = (int)((y / (float)resolution) * texture.height);
            for (int x = 0; x < resolution; x++)
            {
                int texX = (int)((x / (float)resolution) * texture.width);
                Color pixel = pixels[texY * texture.width + texX];
                float heightValue = pixel.r * TerrainInfo.ELEVATION_RANGE + TerrainInfo.MIN_ELEVATION;
                heights[y, x] = Mathf.Clamp01((heightValue - TerrainInfo.MIN_ELEVATION) / TerrainInfo.ELEVATION_RANGE);
            }
        }

        heights = SmoothHeights(heights, resolution, blurIterations);

        marsTerrain.GetComponent<Terrain>().terrainData.SetHeights(0, 0, heights);
        marsTerrain.GetComponent<TerrainCollider>().terrainData.SetHeights(0, 0, heights);
    }

    float[,] SmoothHeights(float[,] heights, int resolution, int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            for (int y = 1; y < resolution - 1; y++)
            {
                for (int x = 1; x < resolution - 1; x++)
                {
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

    void ApplyColorMap(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogError("Mars texture not set!");
            return;
        }

        TerrainLayer terrainLayer = new TerrainLayer();
        terrainLayer.diffuseTexture = texture;
        terrainLayer.tileSize = new Vector2(
            marsTerrain.GetComponent<Terrain>().terrainData.size.x,
            marsTerrain.GetComponent<Terrain>().terrainData.size.z
        );
        marsTerrain.GetComponent<Terrain>().terrainData.terrainLayers = new TerrainLayer[] { terrainLayer };
    }
}