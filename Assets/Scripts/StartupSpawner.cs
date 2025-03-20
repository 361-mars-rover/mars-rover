using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ChunkHandler : MonoBehaviour
{
    public GameObject car;

    public GameObject camera;

    // Terrain generation variables (no prefab needed anymore)
    private Terrain terrain;
    private TerrainCollider terrainCollider;
    private TerrainData terrainData;

    private const float SCALE_DENOMINATOR = 2.1814659085787088E+06f;
    private const float TILE_WIDTH = 256f;
    private float WMS_PIXEL_SIZE = 0.28e-3f;
    private float TerrainWidth;
    private float TerrainLength;

    private int spawnTileRow = 10;
    private int spawnTileCol = 10;

    // Terrain settings
    public float scaleFactor = 1f;
    public int blurIterations = 2;
    private const float MIN_ELEVATION = -8000f;  // Lowest point on Mars
    private const float MAX_ELEVATION = 21000f;  // Olympus Mons peak

    // Tile settings
    private int tileMatrixSet = 7;
    public int tileRow;
    public int tileCol;
    public bool isLoaded = false;
    private float ELEVATION_RANGE;

    private string heightbaseURL = "https://trek.nasa.gov/tiles/Mars/EQ/Mars_MOLA_blend200ppx_HRSC_Shade_clon0dd_200mpp_lzw/1.0.0/default/default028mm";
    private string colorbaseURL = "https://trek.nasa.gov/tiles/Mars/EQ/Mars_Viking_MDIM21_ClrMosaic_global_232m/1.0.0/default/default028mm";

    void Start()
    {
        ELEVATION_RANGE = (MAX_ELEVATION - MIN_ELEVATION) * scaleFactor;
        car.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        TerrainWidth = GetTileSpan();
        TerrainLength = TerrainWidth;

        Debug.Log("Chunk script started");
        Debug.Log("Getting initial tiles");

        Inititialize(spawnTileRow, spawnTileCol, TerrainLength, TerrainWidth);
        Vector3 chunkCenter = new Vector3(15, 20000f, 15);

        StartCoroutine(SpawnCarDelay(chunkCenter));
    }

    private IEnumerator SpawnCarDelay(Vector3 chunkCenter)
    {
        while (!isLoaded)
        {
            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log("Terrain is now loaded");

        RaycastHit hit;
        Ray ray = new Ray(chunkCenter, Vector3.down);
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log("Printing hit");
            Debug.Log(hit.point);
            car.transform.position = hit.point + Vector3.up * 10f;
        }
        else
        {
            Debug.Log("No hit");
        }
    }

    float GetPixelSpan()
    {
        return SCALE_DENOMINATOR * WMS_PIXEL_SIZE;
    }

    float GetTileSpan()
    {
        return TILE_WIDTH * GetPixelSpan() * scaleFactor;
    }

    Vector3 GetChunkCenterFromRowCol(int row, int col)
    {
        float x = TerrainWidth * row + TerrainWidth / 2;
        float z = TerrainWidth * col + TerrainWidth / 2;
        return new Vector3(x, 0, z);
    }

    public void Inititialize(int row, int col, float terrainLength, float terrainWidth)
    {
        terrainData = new TerrainData();
        terrainData.heightmapResolution = 513;
        terrainData.size = new Vector3(terrainLength, ELEVATION_RANGE, terrainLength);

        // Create the terrain GameObject from the terrainData.
        GameObject terrainGO = Terrain.CreateTerrainGameObject(terrainData);
        terrain = terrainGO.GetComponent<Terrain>();
        terrainCollider = terrainGO.GetComponent<TerrainCollider>();

        tileCol = col;
        tileRow = row;

        StartCoroutine(DownloadHeightmapAndColor(row, col));
    }

    string GetDownloadURL(string baseURL, int row, int col)
    {
        return $"{baseURL}/{tileMatrixSet}/{row}/{col}.jpg";
    }

    IEnumerator DownloadHeightmapAndColor(int row, int col)
    {        
        Debug.Log($"Row col: {row}, {col}");
        string heightURL = GetDownloadURL(heightbaseURL, row, col);
        Debug.Log(heightURL);
        string colorURL = GetDownloadURL(colorbaseURL, row, col);
        Debug.Log(colorURL);

        UnityWebRequest heightRequest = UnityWebRequestTexture.GetTexture(heightURL);
        UnityWebRequest colorRequest = UnityWebRequestTexture.GetTexture(colorURL);
        yield return heightRequest.SendWebRequest();
        yield return colorRequest.SendWebRequest();

        if (heightRequest.result == UnityWebRequest.Result.Success)
        {
            Texture2D heightTexture = DownloadHandlerTexture.GetContent(heightRequest);
            Texture2D colorTexture = DownloadHandlerTexture.GetContent(colorRequest);
            ApplyHeightmap(heightTexture);
            ApplyColorMap(colorTexture);
        }
        else
        {
            Debug.LogError("Failed to download heightmap: " + heightRequest.error);
        }
        isLoaded = true;
    }

    void ApplyHeightmap(Texture2D texture)
    {
        int resolution = terrainData.heightmapResolution;
        float[,] heights = new float[resolution, resolution];
        Color[] pixels = texture.GetPixels();

        for (int y = 0; y < resolution; y++)
        {
            int texY = (int)((y / (float)resolution) * texture.height);
            for (int x = 0; x < resolution; x++)
            {
                int texX = (int)((x / (float)resolution) * texture.width);
                Color pixel = pixels[texY * texture.width + texX];
                float heightValue = pixel.r * ELEVATION_RANGE + MIN_ELEVATION;
                heights[y, x] = Mathf.Clamp01((heightValue - MIN_ELEVATION) / ELEVATION_RANGE);
            }
        }

        heights = SmoothHeights(heights, resolution, blurIterations);

        terrainData.SetHeights(0, 0, heights);

        // Update terrain and collider data.
        terrain.terrainData = terrainData;
        terrainCollider.terrainData = terrainData;
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
        terrainLayer.tileSize = new Vector2(terrainData.size.x, terrainData.size.z);
        terrainData.terrainLayers = new TerrainLayer[] { terrainLayer };
    }
}
