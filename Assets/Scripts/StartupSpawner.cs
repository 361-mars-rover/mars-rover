using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class StartupSpawner : MonoBehaviour
{
    public GameObject car;
    public GameObject camera;
    public GameObject marsTerrain;
    public GameObject dustCloudPrefab;
    public Texture2D mineralTexture;
    public ObjectSpawner mineralSpawner;

    // Terrain generation variables
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

    private float scaleFactor = 1f;
    private float heightScale = 0.0025f;
    public int blurIterations = 2;
    private const float MIN_ELEVATION = -8000f;
    private const float MAX_ELEVATION = 21000f;
    private float ELEVATION_RANGE;

    private int tileMatrixSet = 7;

    public bool terrainIsLoaded = false;
    private bool dustIsLoaded = false;

    private float cloudHeight = 250f; // Height above terrain
    private float cloudScrollSpeed = 0.005f;

    private Texture2D dustTexture;
    private GameObject cloudInstance;
    private Color dust_coloring;

    // NASA WMS base URLs
    private string heightbaseURL = "https://trek.nasa.gov/tiles/Mars/EQ/Mars_MOLA_blend200ppx_HRSC_Shade_clon0dd_200mpp_lzw/1.0.0/default/default028mm";
    private string colorbaseURL =  "https://trek.nasa.gov/tiles/Mars/EQ/Mars_Viking_MDIM21_ClrMosaic_global_232m/1.0.0/default/default028mm";

    // This is our local "root" for the entire simulation
    private Transform simulationRoot;

    public void SetRowCol(int row, int col){
        spawnTileRow = row;
        spawnTileCol = col;
    }

    void Start()
    {
        // 1. Store a reference to the SimulationPrefab’s transform
        simulationRoot = this.transform;

        ELEVATION_RANGE = (MAX_ELEVATION - MIN_ELEVATION) * heightScale;
        car.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

        // 2. Compute how large each tile should be in meters (or Unity units).
        TerrainWidth  = GetTileSpan() / 100;
        TerrainLength = TerrainWidth;
        Debug.Log($"Terrain width: {TerrainWidth}");

        Debug.Log("Chunk script started");
        Debug.Log("Getting initial tiles");

        // 3. Initialize terrain in local coordinates
        Inititialize(spawnTileRow, spawnTileCol, TerrainLength, TerrainWidth);

        // Example local chunk center for spawning the car
        Vector3 chunkCenter = new Vector3(0, 100000f, 0); // Local coords
        Debug.Log($"Spawning car at local position: {chunkCenter}");

        // Start the dust texture download and final car spawn
        StartCoroutine(DownloadDustTexture(spawnTileRow, spawnTileCol));
        StartCoroutine(SpawnCarDelay(chunkCenter));
    }

    private IEnumerator SpawnCarDelay(Vector3 chunkCenter)
    {
        // Wait until terrain & dust are fully loaded
        while (!terrainIsLoaded || !dustIsLoaded)
        {
            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log("Terrain and dust are now loaded");
        float height = marsTerrain.GetComponent<Terrain>().SampleHeight(chunkCenter);

        // 4. Parent the car to the simulation root, then set local position
        car.transform.SetParent(simulationRoot, false);

        // We assume chunkCenter is local. So if we want the car’s Y offset
        // to be height + 2 in local coords, do this:
        Vector3 carSpawnPosition = new Vector3(chunkCenter.x, height + 2f, chunkCenter.z);
        car.transform.localPosition = carSpawnPosition;

        car.SetActive(true);
    }

    IEnumerator DownloadDustTexture(int row, int col)
    {
        string dustURL = $"https://trek.nasa.gov/tiles/Mars/EQ/TES_Dust/1.0.0/default/default028mm/{0}/{0}/{0}.png";
        UnityWebRequest dustRequest = UnityWebRequestTexture.GetTexture(dustURL);
        yield return dustRequest.SendWebRequest();

        if (dustRequest.result == UnityWebRequest.Result.Success)
        {
            dustTexture = DownloadHandlerTexture.GetContent(dustRequest);
            CreateCloudLayer();
        }
        else
        {
            Debug.LogError("Failed to download dust texture: " + dustRequest.error);
        }
    }

    void Update()
    {
        // Example: sampling a pixel color from dustTexture based on the car’s local pos
        Vector3 localPos = car.transform.localPosition;
        // You’d need valid bounds checking here; this is just a placeholder example
        if (dustTexture != null && localPos.x >= 0 && localPos.x < dustTexture.width
                                 && localPos.y >= 0 && localPos.y < dustTexture.height)
        {
            dust_coloring = dustTexture.GetPixel((int)localPos.x, (int)localPos.y);
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
        float x = TerrainWidth * row  + TerrainWidth / 2;
        float z = TerrainWidth * col  + TerrainWidth / 2;
        return new Vector3(x, 0, z); // This is local, if we treat everything local
    }

    public void Inititialize(int row, int col, float terrainLength, float terrainWidth)
    {
        // 1. Create a fresh TerrainData
        TerrainData terrainData = new TerrainData
        {
            heightmapResolution = 257,
            size = new Vector3(terrainLength, ELEVATION_RANGE, terrainLength)
        };

        // 2. Attach it to the existing marsTerrain
        marsTerrain.GetComponent<Terrain>().terrainData = terrainData;
        marsTerrain.GetComponent<TerrainCollider>().terrainData = terrainData;

        // 3. Parent the terrain under the simulation root
        marsTerrain.transform.SetParent(simulationRoot, false);

        // 4. Place the terrain so that its local center is offset
        //    e.g. if you want it centered at local (0,0,0), offset by half.
        marsTerrain.transform.localPosition = new Vector3(-terrainLength / 2, 0, -terrainWidth / 2);

        // Start the async loading of height/color data
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
        terrainIsLoaded = true;
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
        terrainLayer.tileSize = new Vector2(
            marsTerrain.GetComponent<Terrain>().terrainData.size.x,
            marsTerrain.GetComponent<Terrain>().terrainData.size.z
        );
        marsTerrain.GetComponent<Terrain>().terrainData.terrainLayers = new TerrainLayer[] { terrainLayer };
    }

    // Modified CreateCloudLayer method
    void CreateCloudLayer()
    {
        if (dustTexture == null)
        {
            Debug.LogError("Dust texture is null!");
            return;
        }

        Terrain terrain = marsTerrain.GetComponent<Terrain>();
        Vector3 terrainSize = terrain.terrainData.size;

        // 1. Create the cloud object & parent it to the simulation root
        cloudInstance = Instantiate(dustCloudPrefab);
        cloudInstance.transform.SetParent(simulationRoot, false);

        // 2. Position it locally above the terrain center
        //    Because marsTerrain is at local (-terrainLength/2, 0, -terrainWidth/2),
        //    the center is roughly (terrainLength/2, 0, terrainWidth/2) in that local space.
        cloudInstance.transform.localPosition = new Vector3(
            0, // offset from the parent's origin
            cloudHeight,           // height above terrain
            0
        );

        // 3. Scale the cloud
        cloudInstance.transform.localScale = new Vector3(
            TerrainLength / 10f,
            1f,
            TerrainWidth / 10f
        );

        // 4. Assign material & set up scrolling
        Renderer renderer = cloudInstance.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("No Renderer component found on cloud prefab!");
            return;
        }

        Material cloudMat = new Material(Shader.Find("Unlit/Transparent"));
        cloudMat.mainTexture = dustTexture;
        dustTexture.wrapMode = TextureWrapMode.Repeat;
        renderer.material = cloudMat;

        CloudScroller scroller = cloudInstance.AddComponent<CloudScroller>();
        scroller.scrollSpeed = cloudScrollSpeed;
        scroller.materialInstance = cloudMat;

        dustIsLoaded = true;
    }

    // Updated CloudScroller class
    public class CloudScroller : MonoBehaviour
    {
        public float scrollSpeed = 0.0001f;
        [HideInInspector] public Material materialInstance;
        private Vector2 offset = Vector2.zero;

        void Update()
        {
            if (materialInstance != null)
            {
                offset.x += Time.deltaTime * scrollSpeed;
                offset.y += Time.deltaTime * scrollSpeed * 0.5f;
                materialInstance.mainTextureOffset = offset;
            }
        }
    }
}