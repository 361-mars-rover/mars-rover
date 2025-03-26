// using System.Collections;
// using UnityEngine;
// using UnityEngine.Networking;

// public class StartupScript : MonoBehaviour
// {
//     public GameObject car;

//     public GameObject camera;

//     // Terrain generation variables (no prefab needed anymore)
//     public GameObject marsTerrain;
//     // // private Terrain terrain;
//     // private TerrainCollider terrainCollider;
//     // private TerrainData terrainData;

//     private const float SCALE_DENOMINATOR = 2.1814659085787088E+06f;
//     private const float TILE_WIDTH = 256f;
//     private float WMS_PIXEL_SIZE = 0.28e-3f;
//     private float TerrainWidth;
//     private float TerrainLength;

//     private int spawnTileRow = 10;
//     private int spawnTileCol = 10;

//     // Terrain settings
//     private float scaleFactor = 1f;
//     private float heightScale = 0.0025f;

//     public int blurIterations = 2;
//     private const float MIN_ELEVATION = -8000f;  // Lowest point on Mars
//     private const float MAX_ELEVATION = 21000f;  // Olympus Mons peak

//     // Tile settings
//     private int tileMatrixSet = 7;
//     public int tileRow;
//     public int tileCol;
//     public bool terrainIsLoaded = false;
//     private bool dustIsLoaded = false;

//     private float ELEVATION_RANGE;

//     public GameObject dustCloudPrefab; // Assign a plane/quad prefab in Inspector
//     private float cloudHeight = 250f; // Height above terrain
//     private float cloudScrollSpeed = 0.005f;

//     private Texture2D dustTexture;
//     private GameObject cloudInstance;

//     public Texture2D mineralTexture;
//     public ObjectSpawner mineralSpawner;

//     private string heightbaseURL = "https://trek.nasa.gov/tiles/Mars/EQ/Mars_MOLA_blend200ppx_HRSC_Shade_clon0dd_200mpp_lzw/1.0.0/default/default028mm";
//     private string colorbaseURL = "https://trek.nasa.gov/tiles/Mars/EQ/Mars_Viking_MDIM21_ClrMosaic_global_232m/1.0.0/default/default028mm";
//     private Color dust_coloring;

//     void Start()
//     {
//         ELEVATION_RANGE = (MAX_ELEVATION - MIN_ELEVATION) * heightScale;
//         car.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
//         TerrainWidth = GetTileSpan() / 100;
//         TerrainLength = TerrainWidth;

//         Debug.Log("Chunk script started");
//         Debug.Log("Getting initial tiles");

//         Inititialize(spawnTileRow, spawnTileCol, TerrainLength, TerrainWidth);
//         // marsTerrain.transform.position = transform.parent.position;
//         // float parentX = transform.parent.position.x;
//         // float parentZ = transform.parent.position.z;
//         Vector3 chunkCenter = new Vector3(0, 100000f, 0);
//         Debug.Log($"Spawning car at position: {chunkCenter.x}, {chunkCenter.z}");
//         // Vector3 chunkCenter = new Vector3(100f , 200000f, 1000f);

//         StartCoroutine(DownloadDustTexture(spawnTileRow, spawnTileCol));
//         // StartCoroutine(sampleAfterDustLoad(chunkCenter));
//         // StartCoroutine(sampleAfterTerrainLoad(chunkCenter));
//         StartCoroutine(SpawnCarDelay(chunkCenter));
//     }

//     // private IEnumerator sampleAfterDustLoad(Vector3 chunkCenter){
//     //     while (!dustIsLoaded)
//     //     {
//     //         yield return new WaitForSeconds(0.1f);
//     //     }
//     //     Debug.Log("Dust is loaded");
//     //     float height = marsTerrain.GetComponent<Terrain>().SampleHeight(chunkCenter);
//     //     Debug.Log($"Sampled height after dust loaded: {height}");
//     // }

//     // private IEnumerator sampleAfterTerrainLoad(Vector3 chunkCenter){
//     //     while (!terrainIsLoaded)
//     //     {
//     //         yield return new WaitForSeconds(0.1f);
//     //     }
//     //     Debug.Log("Terrain is loaded");
//     //     Debug.Log($"Terrain loaded: {terrainIsLoaded}");
//     //     float height = marsTerrain.GetComponent<Terrain>().SampleHeight(chunkCenter);
//     //     Debug.Log($"Sampled height after terrain loaded: {height}");
//     // }

//     private IEnumerator SpawnCarDelay(Vector3 chunkCenter)
//     {
//         while (!terrainIsLoaded || !dustIsLoaded)
//         {
//             yield return new WaitForSeconds(0.2f);
//         }
//         Debug.Log("Terrain and dust are now loaded");
//         Debug.Log($"Terrain loaded: {terrainIsLoaded}, Dust loaded: {dustIsLoaded}");
//         float height = marsTerrain.GetComponent<Terrain>().SampleHeight(chunkCenter);
//         car.transform.SetParent(marsTerrain.transform.parent);
//         Vector3 carSpawnPosition = new Vector3(chunkCenter.x, height + 2f, chunkCenter.z);
//         Debug.Log($"Spawning car at {carSpawnPosition}");
//         car.transform.position = carSpawnPosition;
//         car.SetActive(true);

//         // RaycastHit hit;
//         // Ray ray = new Ray(chunkCenter, Vector3.down);
//         // if (Physics.Raycast(ray, out hit))
//         // {
//         //     // Debug.Log("Printing hit");
//         //     // Debug.Log(hit.point);
//         //     // car.transform.position = hit.point + Vector3.up * 10f;
//         //     // car.transform.position = new Vector3(-263.65f, 1971.145f, 69.5703f);
//         // }
//         // else
//         // {
//         //     Debug.Log("No hit");
//         // }
//     }

//     IEnumerator DownloadDustTexture(int row, int col)
//     {
//         string dustURL = $"https://trek.nasa.gov/tiles/Mars/EQ/TES_Dust/1.0.0/default/default028mm/{0}/{0}/{0}.png";
//         UnityWebRequest dustRequest = UnityWebRequestTexture.GetTexture(dustURL);
//         yield return dustRequest.SendWebRequest();

//         if (dustRequest.result == UnityWebRequest.Result.Success)
//         {
//             dustTexture = DownloadHandlerTexture.GetContent(dustRequest);
//             CreateCloudLayer();
//         }
//         else
//         {
//             Debug.LogError("Failed to download dust texture: " + dustRequest.error);
//         }
//     }

//     void Update()
//     {
//         Vector3 pos = car.transform.position;
//         if (dust_coloring == null){
//             dust_coloring = dustTexture.GetPixel((int)pos.x, (int)pos.y);
//         }
//         //Debug.Log(dust_coloring);
//     }

//     float GetPixelSpan()
//     {
//         return SCALE_DENOMINATOR * WMS_PIXEL_SIZE;
//     }

//     float GetTileSpan()
//     {
//         return TILE_WIDTH * GetPixelSpan() * scaleFactor;
//     }

//     Vector3 GetChunkCenterFromRowCol(int row, int col)
//     {
//         float x = TerrainWidth * row + TerrainWidth / 2;
//         float z = TerrainWidth * col + TerrainWidth / 2;
//         return new Vector3(x, 0, z);
//     }

//     public void Inititialize(int row, int col, float terrainLength, float terrainWidth)
//     {
//         TerrainData terrainData = new TerrainData
//         {
//             heightmapResolution = 257,
//             size = new Vector3(terrainLength, ELEVATION_RANGE, terrainLength)
//         };

//         marsTerrain.GetComponent<Terrain>().terrainData = terrainData;
//         marsTerrain.GetComponent<TerrainCollider>().terrainData = terrainData;

//         // Create the terrain GameObject from the terrainData.
//         // GameObject terrainGO = Terrain.CreateTerrainGameObject(terrainData);
//         // terrainGO.transform.position = new Vector3(-terrainLength / 2, 0, -terrainLength / 2);
//         Debug.Log("Spawning new terrain");
//         Debug.Log($"Parent positon is ${transform.position}");
//         Debug.Log($"Transform positon is ${transform.position}");

//         Debug.Log($"Mars terrain parent was: {marsTerrain.transform.parent}");

//         // marsTerrain.transform.SetParent(transform.parent);

//         // Debug.Log($"Mars terrain parent is now: {marsTerrain.transform.parent}");


//         // marsTerrain.transform.position = transform.parent.position + new Vector3(-terrainLength / 2, 0, -terrainLength / 2);
//         marsTerrain.transform.position = new Vector3(-terrainLength / 2, 0, -terrainLength / 2);
//         Debug.Log($"Mars terrain position: {marsTerrain.transform.position}");
//         // terrain = terrainGO.GetComponent<Terrain>();
//         // terrainCollider = terrainGO.GetComponent<TerrainCollider>();

//         tileCol = col;
//         tileRow = row;

//         StartCoroutine(DownloadHeightmapAndColor(row, col));
//     }

//     string GetDownloadURL(string baseURL, int row, int col)
//     {
//         return $"{baseURL}/{tileMatrixSet}/{row}/{col}.jpg";
//     }

//     IEnumerator DownloadHeightmapAndColor(int row, int col)
//     {        
//         Debug.Log($"Row col: {row}, {col}");
//         string heightURL = GetDownloadURL(heightbaseURL, row, col);
//         Debug.Log(heightURL);
//         string colorURL = GetDownloadURL(colorbaseURL, row, col);
//         Debug.Log(colorURL);

//         UnityWebRequest heightRequest = UnityWebRequestTexture.GetTexture(heightURL);
//         UnityWebRequest colorRequest = UnityWebRequestTexture.GetTexture(colorURL);
//         yield return heightRequest.SendWebRequest();
//         yield return colorRequest.SendWebRequest();

//         if (heightRequest.result == UnityWebRequest.Result.Success)
//         {
//             Texture2D heightTexture = DownloadHandlerTexture.GetContent(heightRequest);
//             Texture2D colorTexture = DownloadHandlerTexture.GetContent(colorRequest);
//             ApplyHeightmap(heightTexture);
//             ApplyColorMap(colorTexture);
//         }
//         else
//         {
//             Debug.LogError("Failed to download heightmap: " + heightRequest.error);
//         }
//         terrainIsLoaded = true;
//     }

//     void ApplyHeightmap(Texture2D texture)
//     {
//         int resolution = marsTerrain.GetComponent<Terrain>().terrainData.heightmapResolution;
//         float[,] heights = new float[resolution, resolution];
//         Color[] pixels = texture.GetPixels();

//         for (int y = 0; y < resolution; y++)
//         {
//             int texY = (int)((y / (float)resolution) * texture.height);
//             for (int x = 0; x < resolution; x++)
//             {
//                 int texX = (int)((x / (float)resolution) * texture.width);
//                 Color pixel = pixels[texY * texture.width + texX];
//                 float heightValue = pixel.r * ELEVATION_RANGE + MIN_ELEVATION;
//                 heights[y, x] = Mathf.Clamp01((heightValue - MIN_ELEVATION) / ELEVATION_RANGE);
//             }
//         }

//         heights = SmoothHeights(heights, resolution, blurIterations);

//         marsTerrain.GetComponent<Terrain>().terrainData.SetHeights(0, 0, heights);
//         marsTerrain.GetComponent<TerrainCollider>().terrainData.SetHeights(0, 0, heights);
//     }

//     float[,] SmoothHeights(float[,] heights, int resolution, int iterations)
//     {
//         for (int i = 0; i < iterations; i++)
//         {
//             for (int y = 1; y < resolution - 1; y++)
//             {
//                 for (int x = 1; x < resolution - 1; x++)
//                 {
//                     float avg = (
//                         heights[y, x] +
//                         heights[y, x + 1] +
//                         heights[y, x - 1] +
//                         heights[y + 1, x] +
//                         heights[y - 1, x]
//                     ) / 5f;
//                     heights[y, x] = avg;
//                 }
//             }
//         }
//         return heights;
//     }

//     void ApplyColorMap(Texture2D texture)
//     {
//         if (texture == null)
//         {
//             Debug.LogError("Mars texture not set!");
//             return;
//         }

//         TerrainLayer terrainLayer = new TerrainLayer();
//         terrainLayer.diffuseTexture = texture;
//         terrainLayer.tileSize = new Vector2(marsTerrain.GetComponent<Terrain>().terrainData.size.x, marsTerrain.GetComponent<Terrain>().terrainData.size.z);
//         marsTerrain.GetComponent<Terrain>().terrainData.terrainLayers = new TerrainLayer[] { terrainLayer };
//     }

//     // Modified CreateCloudLayer method
//     void CreateCloudLayer()
//     {
//         if (dustTexture == null)
//         {
//             Debug.LogError("Dust texture is null!");
//             return;
//         }

//         Terrain terrain = marsTerrain.GetComponent<Terrain>();
//         Vector3 terrainPos = terrain.transform.position;
//         Vector3 terrainSize = terrain.terrainData.size;

//         // Create the cloud object
//         cloudInstance = Instantiate(dustCloudPrefab);
//         cloudInstance.transform.SetParent(marsTerrain.transform.parent);
        
//         // Position and scale it
//         cloudInstance.transform.position = new Vector3(
//         terrainPos.x + terrainSize.x/2,  // Center X
//         cloudHeight,                     // Height above terrain
//         terrainPos.z + terrainSize.z/2   // Center Z
//         );

//         cloudInstance.transform.localScale = new Vector3(
//             TerrainLength/10f, 
//             1f, 
//             TerrainWidth/10f
//         );

//         // Get or create the renderer
//         Renderer renderer = cloudInstance.GetComponent<Renderer>();
//         if (renderer == null)
//         {
//             Debug.LogError("No Renderer component found on cloud prefab!");
//             return;
//         }

//         // Create a new material using the Unlit/Transparent shader
//         Material cloudMat = new Material(Shader.Find("Unlit/Transparent"));
//         cloudMat.mainTexture = dustTexture;
        
//         // Set texture wrap mode
//         dustTexture.wrapMode = TextureWrapMode.Repeat;
        
//         // Apply to renderer
//         renderer.material = cloudMat;

//         // Add scrolling component
//         CloudScroller scroller = cloudInstance.AddComponent<CloudScroller>();
//         scroller.scrollSpeed = cloudScrollSpeed;
//         scroller.materialInstance = cloudMat;
//         dustIsLoaded = true;
//     }


//     // Updated CloudScroller class
//     public class CloudScroller : MonoBehaviour
//     {
//         public float scrollSpeed = 0.0001f;
//         [HideInInspector] public Material materialInstance;
//         private Vector2 offset = Vector2.zero;

//         void Update()
//         {
//             if (materialInstance != null)
//             {
//                 offset.x += Time.deltaTime * scrollSpeed;
//                 offset.y += Time.deltaTime * scrollSpeed * 0.5f;
//                 materialInstance.mainTextureOffset = offset;
//             }
//         }
//     }
// }

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class StartupScript : MonoBehaviour
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
    public int tileRow;
    public int tileCol;
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

    void Start()
    {
        // 1. Store a reference to the SimulationPrefab’s transform
        simulationRoot = this.transform;

        ELEVATION_RANGE = (MAX_ELEVATION - MIN_ELEVATION) * heightScale;
        car.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

        // 2. Compute how large each tile should be in meters (or Unity units).
        TerrainWidth  = GetTileSpan() / 100;
        TerrainLength = TerrainWidth;

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
            yield return new WaitForSeconds(0.2f);
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

        tileCol = col;
        tileRow = row;

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