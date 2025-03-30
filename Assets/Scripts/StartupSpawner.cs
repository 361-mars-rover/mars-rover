using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class StartupScript : MonoBehaviour
{
    public GameObject car;

    public GameObject camera;

    // Terrain generation variables (no prefab needed anymore)
    public GameObject marsTerrain;
    // // private Terrain terrain;
    // private TerrainCollider terrainCollider;
    // private TerrainData terrainData;

    public BoxCollider invisibleWall;
    public BoxCollider invisibleWall2;
    public BoxCollider invisibleWall3;
    public BoxCollider invisibleWall4;


    private const float SCALE_DENOMINATOR = 2.1814659085787088E+06f;
    private const float TILE_WIDTH = 256f;
    private float WMS_PIXEL_SIZE = 0.28e-3f;
    private float TerrainWidth;
    private float TerrainLength;

    private int spawnTileRow = 10;
    private int spawnTileCol = 10;

    // Terrain settings
    private float scaleFactor = 1f;
    public float heightScale = 0.1f;

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
        ELEVATION_RANGE = (MAX_ELEVATION - MIN_ELEVATION) * heightScale;
        car.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        TerrainWidth = GetTileSpan()/100;
        TerrainLength = TerrainWidth;

        Debug.Log("Chunk script started");
        Debug.Log("Getting initial tiles");

        Inititialize(spawnTileRow, spawnTileCol, TerrainLength, TerrainWidth);
        Vector3 chunkCenter = new Vector3(0 , 100000f, 0);
        Debug.Log($"Spawning car at position: {chunkCenter.x}, {chunkCenter.z}");
        // Vector3 chunkCenter = new Vector3(100f , 200000f, 1000f);
        InitializeInvisibleWalls(TerrainWidth, TerrainLength, 10f);

        StartCoroutine(SpawnCarDelay(chunkCenter));
    }

    void InitializeInvisibleWalls(float terrainWidth, float terrainLength, float wallHeight){
    invisibleWall.center = new Vector3(0, wallHeight / 2, terrainLength / 2);
    invisibleWall.size = new Vector3(terrainWidth, wallHeight, 1f);

    // South Wall
    invisibleWall2.center = new Vector3(0, wallHeight / 2, -terrainLength / 2);
    invisibleWall2.size = new Vector3(terrainWidth, wallHeight, 1f);

    // East Wall
    invisibleWall3.center = new Vector3(terrainWidth / 2, wallHeight / 2, 0);
    invisibleWall3.size = new Vector3(1f, wallHeight, terrainLength);

    // West Wall
    invisibleWall4.center = new Vector3(-terrainWidth / 2, wallHeight / 2, 0);
    invisibleWall4.size = new Vector3(1f, wallHeight, terrainLength);
    }
    

    private IEnumerator SpawnCarDelay(Vector3 chunkCenter)
    {
        while (!isLoaded)
        {
            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log("Terrain is now loaded");
        float height = marsTerrain.GetComponent<Terrain>().SampleHeight(chunkCenter);
        car.transform.position = new Vector3(chunkCenter.x, height + 2f, chunkCenter.z);

        // RaycastHit hit;
        // Ray ray = new Ray(chunkCenter, Vector3.down);
        // if (Physics.Raycast(ray, out hit))
        // {
        //     // Debug.Log("Printing hit");
        //     // Debug.Log(hit.point);
        //     // car.transform.position = hit.point + Vector3.up * 10f;
        //     // car.transform.position = new Vector3(-263.65f, 1971.145f, 69.5703f);
        // }
        // else
        // {
        //     Debug.Log("No hit");
        // }
    }

    void Update()
    {
        Vector3 pos = car.transform.position;
        // float terrainHeight = terrain.SampleHeight(pos);

        // float heightDiff = pos.y - terrainHeight;

        // Debug.Log($"Rover Y: {pos.y}, Terrain Y: {terrainHeight}, Difference: {heightDiff}");

        Ray ray = new Ray(car.transform.position + Vector3.up * 5f, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 20f))
        {
            float terrainHeight = hit.point.y;   // Collider height
            float visualHeight = marsTerrain.GetComponent<Terrain>().SampleHeight(transform.position);  // Heightmap height

            float diff = visualHeight - terrainHeight;

            // Debug.Log($"Collider Y: {terrainHeight}, Heightmap Y: {visualHeight}, Difference: {diff}");
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
        TerrainData terrainData = new TerrainData
        {
            heightmapResolution = 257,
            size = new Vector3(terrainLength, ELEVATION_RANGE, terrainLength)
        };

        marsTerrain.GetComponent<Terrain>().terrainData = terrainData;
        marsTerrain.GetComponent<TerrainCollider>().terrainData = terrainData;

        // Create the terrain GameObject from the terrainData.
        // GameObject terrainGO = Terrain.CreateTerrainGameObject(terrainData);
        // terrainGO.transform.position = new Vector3(-terrainLength / 2, 0, -terrainLength / 2);
        marsTerrain.transform.position = new Vector3(-terrainLength / 2, 0, -terrainLength / 2);
        // terrain = terrainGO.GetComponent<Terrain>();
        // terrainCollider = terrainGO.GetComponent<TerrainCollider>();

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
                float heightValue = pixel.r * ELEVATION_RANGE + MIN_ELEVATION;
                heights[y, x] = Mathf.Clamp01((heightValue - MIN_ELEVATION) / ELEVATION_RANGE);
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
        terrainLayer.tileSize = new Vector2(marsTerrain.GetComponent<Terrain>().terrainData.size.x, marsTerrain.GetComponent<Terrain>().terrainData.size.z);
        marsTerrain.GetComponent<Terrain>().terrainData.terrainLayers = new TerrainLayer[] { terrainLayer };
    }
}
