using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class StartupSpawner : MonoBehaviour
{
    public GameObject car;
    // public GameObject camera;
    public GameObject marsTerrain;
    public GameObject dustCloudPrefab;
    public Texture2D mineralTexture;

    // Terrain generation variables

    public BoxCollider invisibleWall;
    public BoxCollider invisibleWall2;
    public BoxCollider invisibleWall3;
    public BoxCollider invisibleWall4;

    private int spawnTileRow = 10;
    private int spawnTileCol = 10;

    private float heightScale = 0.0025f;
    public int blurIterations = 2;
    private const float MIN_ELEVATION = -8000f;
    private const float MAX_ELEVATION = 21000f;
    private float ELEVATION_RANGE;

    private int tileMatrixSet = 7;

    public bool terrainIsLoaded = false;
    private bool dustIsLoaded = false;

    private Texture2D dustTexture;
    private GameObject cloudInstance;
    private Color dust_coloring;

    // NASA WMS base URLs
    private string heightbaseURL = "https://trek.nasa.gov/tiles/Mars/EQ/Mars_MOLA_blend200ppx_HRSC_Shade_clon0dd_200mpp_lzw/1.0.0/default/default028mm";
    private string colorbaseURL =  "https://trek.nasa.gov/tiles/Mars/EQ/Mars_Viking_MDIM21_ClrMosaic_global_232m/1.0.0/default/default028mm";

    // This is our local "root" for the entire simulation
    private Transform simulationRoot;
    public FirebaseManager firebaseManager; // Reference to the Firebase manager

    private TerrainLoader tl;
    private CloudLoader cl;
    public void SetRowCol(int row, int col){
        spawnTileRow = row;
        spawnTileCol = col;
    }

    void Start()
    {
        // 1. Store a reference to the SimulationPrefab’s transform
        simulationRoot = this.transform;

        tl = TerrainLoader.Create(spawnTileRow, spawnTileCol, simulationRoot, marsTerrain);
        tl.Load();

        cl = CloudLoader.Create(spawnTileRow, spawnTileCol, dustCloudPrefab, marsTerrain, simulationRoot);
        cl.Load();

        // Example local chunk center for spawning the car
        Vector3 chunkCenter = new Vector3(0, 100000f, 0); // Local coords
        Debug.Log($"Spawning car at local position: {chunkCenter}");

        StartCoroutine(SpawnCarDelay(chunkCenter));
        InitializeInvisibleWalls(120f);
        firebaseManager.StoreMarsTerrainData(firebaseManager.simulationId, TerrainInfo.TERRAIN_WIDTH, TerrainInfo.TERRAIN_LENGTH, 
            spawnTileRow, spawnTileCol);

    }

        void Update()
    {
        // Example: sampling a pixel color from dustTexture based on the car’s local pos
        Vector3 localPos = car.transform.localPosition;
        // You’d need valid bounds checking here; this is just a placeholder example
        if (cl.DustTexture != null && localPos.x >= 0 && localPos.x < cl.DustTexture.width
                                 && localPos.y >= 0 && localPos.y < cl.DustTexture.height)
        {
            dust_coloring = cl.DustTexture.GetPixel((int)localPos.x, (int)localPos.y);
        }
    }

    private IEnumerator SpawnCarDelay(Vector3 chunkCenter)
    {
        // Wait until terrain & dust are fully loaded
        while (!tl.getIsLoaded() || !cl.getIsLoaded())
        {
            Debug.Log($"TL is loaded: ${tl.getIsLoaded()}");
            Debug.Log($"CL is loaded: ${cl.getIsLoaded()}");
            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log("Terrain and dust are now loaded");
        float height = marsTerrain.GetComponent<Terrain>().SampleHeight(chunkCenter);

        // 4. Parent the car to the simulation root, then set local position
        car.transform.SetParent(simulationRoot, false);

        // We assume chunkCenter is local. So if we want the car’s Y offset
        // to be height + 2 in local coords, do this:
        Vector3 carSpawnPosition = new Vector3(chunkCenter.x, height + 3f, chunkCenter.z);
        car.transform.localPosition = carSpawnPosition;

        car.SetActive(true);
    }

    void InitializeInvisibleWalls(float wallHeight){
        invisibleWall.center = new Vector3(0, wallHeight / 2, TerrainInfo.TERRAIN_LENGTH / 2);
        invisibleWall.size = new Vector3(TerrainInfo.TERRAIN_WIDTH, wallHeight, 1f);

        // South Wall
        invisibleWall2.center = new Vector3(0, wallHeight / 2, -TerrainInfo.TERRAIN_LENGTH / 2);
        invisibleWall2.size = new Vector3(TerrainInfo.TERRAIN_WIDTH, wallHeight, 1f);

        // East Wall
        invisibleWall3.center = new Vector3(TerrainInfo.TERRAIN_WIDTH / 2, wallHeight / 2, 0);
        invisibleWall3.size = new Vector3(1f, wallHeight, TerrainInfo.TERRAIN_LENGTH);

        // West Wall
        invisibleWall4.center = new Vector3(-TerrainInfo.TERRAIN_WIDTH / 2, wallHeight / 2, 0);
        invisibleWall4.size = new Vector3(1f, wallHeight, TerrainInfo.TERRAIN_LENGTH);
    }
}