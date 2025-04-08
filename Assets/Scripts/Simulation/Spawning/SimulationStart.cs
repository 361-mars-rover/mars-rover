using System.Collections;
using UnityEngine;
using Loaders;

public class SimulationStart : MonoBehaviour
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

    // This is our local "root" for the entire simulation
    public FirebaseManager firebaseManager; // Reference to the Firebase manager

    private TerrainLoader tl;
    private CloudLoader cl;

    private Loader compositeLoader;

    private InvisibleWallLoader iwl;
    public void SetRowCol(int row, int col){
        spawnTileRow = row;
        spawnTileCol = col;
    }

    private void setupLoader()
    {
        Transform simulationRoot = this.transform;
        tl = TerrainLoader.Factory.Create(spawnTileRow, spawnTileCol, simulationRoot, marsTerrain);
        cl = CloudLoader.Factory.Create(spawnTileRow, spawnTileCol, dustCloudPrefab, marsTerrain, simulationRoot);
        iwl = InvisibleWallLoader.Factory.Create(invisibleWall, invisibleWall2, invisibleWall3, invisibleWall4);
        compositeLoader = CompositeLoader.Factory.Create(null, tl, cl, iwl);
    }

    void Start()
    {
        setupLoader();
        compositeLoader.Load();
        StartCoroutine(SpawnCarDelay(this.transform));
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
            cl.DustColouring = cl.DustTexture.GetPixel((int)localPos.x, (int)localPos.y);
        }
    }

    private IEnumerator SpawnCarDelay(Transform simulationRoot)
    {
        // Wait until terrain & dust are fully loaded
        while (!tl.IsLoaded || !cl.IsLoaded)
        {
            Debug.Log($"TL is loaded: ${tl.IsLoaded}");
            Debug.Log($"CL is loaded: ${cl.IsLoaded}");
            yield return new WaitForSeconds(0.1f);
        }
        Vector3 carSpawnPosition = new Vector3(0, 0, 0);
        Debug.Log("Terrain and dust are now loaded");
        float height = marsTerrain.GetComponent<Terrain>().SampleHeight(carSpawnPosition);

        // 4. Parent the car to the simulation root, then set local position
        car.transform.SetParent(simulationRoot, false);
        carSpawnPosition.y = height + 3f;
        car.transform.localPosition = carSpawnPosition;

        car.SetActive(true);
    }
}