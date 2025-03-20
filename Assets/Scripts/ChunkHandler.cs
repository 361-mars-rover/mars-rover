using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkHandler : MonoBehaviour
{
    public GameObject car;
    public GameObject[] chunks;
    public Vector3[] chunkPositions;

    public Dictionary<Vector3, GameObject> loadedChunks = new Dictionary<Vector3, GameObject>();

    public Vector3 prevChunkPosition;
    public float activationDistance = 1500.0f;
    public float checkInterval = 0.1f;
    public GameObject terrainPrefab;

    // Set this to be 
    // private float TerrainWidth = 156367.5f;
    // private float TerrainLength = 156367.5f;

    private const float SCALE_DENOMINATOR = 2.1814659085787088E+06f;

    private const float TILE_WIDTH = 256f;

    private float WMS_PIXEL_SIZE = 0.28e-3f;

    private float TerrainWidth;
    private float TerrainLength;

    private int spawnTileRow = 10;
    private int spawnTileCol = 10;
    
    // Loads initial chunks based on car position
    void Start()
    {
        // car.SetActive(false);
        // Vector3 chunkCenter = GetChunkCenterFromRowCol(spawnTileRow, spawnTileCol);
        // Debug.Log($"Chunk center for ({spawnTileRow}, {spawnTileRow}) is {chunkCenter}");
        TerrainWidth = GetTileSpan();
        TerrainLength = TerrainWidth;

        Debug.Log("Chunk script started");
        prevChunkPosition = GetClosestChunkCenter(car.transform.position);
        Vector3[] chunksToLoad = GetChunksToLoad(car.transform.position);
        

        // currentChunks[0] = Instantiate(terrainPrefab, prevChunkPosition, Quaternion.identity);
        Debug.Log("Getting initial tiles");
        Vector3 chunkPos = Vector3.zero;
        // Create a tile at 0,0,0
        GameObject chunk = Instantiate(terrainPrefab, chunkPos, Quaternion.identity);
        TerrainChunk t = chunk.GetComponent<TerrainChunk>();
        t.Inititialize(spawnTileRow, spawnTileCol, TerrainLength, TerrainWidth);
        Vector3 chunkCenter = new Vector3(TerrainWidth / 2, 20000f, TerrainWidth / 2);
        // Wait for chunk to load

        StartCoroutine(SpawnCarDelay(chunkCenter, t));
        StartCoroutine(CheckChunkDistance());
    }

    // Spawns car at the center of the selected chunk
    private IEnumerator SpawnCarDelay(Vector3 chunkCenter, TerrainChunk t){
        while (!t.isLoaded){
            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log("terrain is now loaded");
        RaycastHit hit;
        Ray ray = new Ray(chunkCenter, Vector3.down);
        if (Physics.Raycast(ray, out hit)){
            Debug.Log("Printing hit");
            Debug.Log(hit.point);
            car.transform.position = hit.point + Vector3.up * 10f;
            // car.SetActive(true);
        }
        else {
            Debug.Log("no hit");
        }
    }

    // Gets pixel span (span = height or width) in meters based on WMS docs: https://www.ogc.org/publications/standard/wmts/
    float GetPixelSpan(){return  SCALE_DENOMINATOR * WMS_PIXEL_SIZE;}

    // Gets tile span in meters ()
    float GetTileSpan()
    {return TILE_WIDTH * GetPixelSpan();}
    
    // Find the position of the nearest chunk center relative to the car's position
    Vector3 GetClosestChunkCenter(Vector3 position)
    {
        // This gets bottom left corner of the tile
        float x = Mathf.Floor(position.x / TerrainWidth) * TerrainWidth;
        float z = Mathf.Floor(position.z / TerrainLength) * TerrainLength;

        return new Vector3(x, 0, z);
    }
    // Gets the surrounding 8 chunk positions
    Vector3[] GetChunksToLoad(Vector3 position)
    {
        Vector3 nearestChunkPositon = GetClosestChunkCenter(position);
        Vector3[] chunksToLoad = new Vector3[9];
        chunksToLoad[0] = nearestChunkPositon + Vector3.forward * TerrainLength; // Above
        chunksToLoad[1] = nearestChunkPositon + Vector3.back * TerrainLength; // Below
        chunksToLoad[2] = nearestChunkPositon + Vector3.right * TerrainWidth; // Right
        chunksToLoad[3] = nearestChunkPositon + Vector3.left * TerrainWidth; // Left
        chunksToLoad[4] = nearestChunkPositon + Vector3.forward * TerrainLength + Vector3.right * TerrainWidth;  //  Top right
        chunksToLoad[5] = nearestChunkPositon + Vector3.forward * TerrainLength + Vector3.left * TerrainWidth; // Top left
        chunksToLoad[6] = nearestChunkPositon + Vector3.back * TerrainLength + Vector3.right * TerrainWidth; // Back right
        chunksToLoad[7] = nearestChunkPositon + Vector3.back * TerrainLength + Vector3.left * TerrainWidth; // Back left
        chunksToLoad[8] = nearestChunkPositon; // Current chunk
        return chunksToLoad;
    }

    Vector3 GetChunkCenterFromRowCol(int row, int col){
        float x = TerrainWidth * row + TerrainWidth / 2;
        float z = TerrainWidth * col + TerrainWidth / 2;
        return new Vector3(x, 0, z);
    }

    // Takes a chunk corner position and returns the corresponding row and column of the tile
    (int, int) GetRowColFromPosition(Vector3 position)
    {
        // int row = (int) Math.Round((position.z - (TerrainLength / 2)) / TerrainLength);
        // int col = (int) Math.Round((position.x - (TerrainLength / 2)) / TerrainLength);
        int row = (int) Math.Round(position.z / TerrainLength);
        int col = (int) Math.Round(position.x  / TerrainWidth);
        // Debug.Log($"row: {row} col: {col}");

        return (row,col);
    }

    // Infinitely loops to check car position and load new chunks when needed
    IEnumerator CheckChunkDistance()
    {
        while(true)
        {
            Vector3 currentChunkPosition = GetClosestChunkCenter(car.transform.position);

            // Only do updates if we need new chunks
            if (currentChunkPosition != prevChunkPosition)
            {
                Debug.Log($"Current chunk center: {GetRowColFromPosition(currentChunkPosition)}");

                HashSet<Vector3> currentChunkPositions = new HashSet<Vector3>(loadedChunks.Keys); // Positions of all currently loaded chunks
                HashSet<Vector3> chunksToLoad = new HashSet<Vector3>(GetChunksToLoad(car.transform.position)); // Positions of all the chunks that must be loaded given car position

                HashSet<Vector3> newChunkPositions = new HashSet<Vector3>(chunksToLoad.Except(currentChunkPositions)); // Only the positions of new chunks
                HashSet<Vector3> chunksToDelete = new HashSet<Vector3>(currentChunkPositions.Except(chunksToLoad)); // Only positions of chunks to be deleted

                foreach (Vector3 newChunkPos in newChunkPositions) // Load in all the new chunks
                { 
                    var (row, col) = GetRowColFromPosition(newChunkPos);
                    if (row < 0 || col < 0) continue;
                    GameObject chunk = Instantiate(terrainPrefab, newChunkPos, Quaternion.identity);
                    chunk.GetComponent<TerrainChunk>().Inititialize(row, col, TerrainLength, TerrainWidth);
                    loadedChunks[newChunkPos] = chunk;
                }

                foreach (Vector3 deletePos in chunksToDelete) // Delete all the old chunks
                {
                    Destroy(loadedChunks[deletePos]);
                    loadedChunks.Remove(deletePos);
                }
            }

            prevChunkPosition = currentChunkPosition;


            // float h = loadedChunks[Vector3.zero].GetComponent<Terrain>().SampleHeight(new Vector3(39091.87f, 0, 39091.87f));

            // Debug.Log($"height at zero: {h}");
            yield return new WaitForSeconds(checkInterval);
        }        
    }
}
