using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    
    // Loads initial chunks based on car position
    void Start()
    {
        TerrainWidth = GetTileSpan();
        TerrainLength = TerrainWidth;

        prevChunkPosition = GetClosestChunkCenter(car.transform.position);
        UpdateChunks();

        StartCoroutine(UpdateChunksLoop());
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
        // TODO: Make this cleaner...
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

    Dictionary<string, List<Vector3>> GetChunksToAddAndDelete(Vector3[] chunksToLoad)
    {
        Dictionary<string, List<Vector3>> chunksToAddAndDelete = new Dictionary<string, List<Vector3>>();
        
        HashSet<Vector3> currentChunkPositionsSet = new HashSet<Vector3>(loadedChunks.Keys); // Positions of all currently loaded chunks
        HashSet<Vector3> chunksToLoadSet = new HashSet<Vector3>(chunksToLoad); // Positions of all the chunks that must be loaded given car position

        chunksToAddAndDelete["add"] = chunksToLoadSet.Except(currentChunkPositionsSet).ToList(); // Only the positions of new chunks
        chunksToAddAndDelete["delete"] = currentChunkPositionsSet.Except(chunksToLoadSet).ToList(); // Only positions of chunks to be deleted

        return chunksToAddAndDelete;
    }

    void AddAndDeleteChunks(Vector3[] chunksToLoad)
    {
        Dictionary<string, List<Vector3>> chunksToAddAndDelete = GetChunksToAddAndDelete(chunksToLoad);

        foreach (Vector3 newChunkPos in chunksToAddAndDelete["add"]) // Load in all the new chunks
        { 
            var (row, col) = GetRowColFromPosition(newChunkPos);
            if (row < 0 || col < 0) continue;
            GameObject chunk = Instantiate(terrainPrefab, newChunkPos, Quaternion.identity);
            chunk.GetComponent<TerrainChunk>().Inititialize(row, col, TerrainLength, TerrainWidth);
            loadedChunks[newChunkPos] = chunk;
        }


        foreach (Vector3 deletePos in chunksToAddAndDelete["delete"]) // Delete all the old chunks
        {
            Destroy(loadedChunks[deletePos]);
            loadedChunks.Remove(deletePos);
        }
    }

    void UpdateNeighbours(Vector3[] chunksToLoad)
    {
        // TODO: Explain this...
        Terrain center = loadedChunks.TryGetValue(chunksToLoad[8], out GameObject cChunk) ? cChunk.GetComponent<Terrain>() : null;
        Terrain left = loadedChunks.TryGetValue(chunksToLoad[3], out GameObject lChunk) ? lChunk.GetComponent<Terrain>() : null;
        Terrain up = loadedChunks.TryGetValue(chunksToLoad[0], out GameObject uChunk) ? uChunk.GetComponent<Terrain>() : null;
        Terrain down = loadedChunks.TryGetValue(chunksToLoad[1], out GameObject dChunk) ? dChunk.GetComponent<Terrain>() : null;
        Terrain right = loadedChunks.TryGetValue(chunksToLoad[2], out GameObject rChunk) ? rChunk.GetComponent<Terrain>() : null;

        Debug.Log($"Center: {center}");
        Debug.Log($"left: {left}");
        Debug.Log($"up: {up}");
        Debug.Log($"down: {down}");
        Debug.Log($"right: {right}");

        center.SetNeighbors(left, up, right, down);
    }

    void UpdateChunks()
    {
        // Debug.Log($"Current chunk center: {GetRowColFromPosition(currentChunkPosition)}");

        Vector3[] chunksToLoad = GetChunksToLoad(car.transform.position);

        AddAndDeleteChunks(chunksToLoad);

        UpdateNeighbours(chunksToLoad);
    }

    // Infinitely loops to check car position and load new chunks when needed
    IEnumerator UpdateChunksLoop()
    {
        while(true)
        {
            Vector3 currentChunkPosition = GetClosestChunkCenter(car.transform.position);

            // Only do updates if we need new chunks
            if (currentChunkPosition != prevChunkPosition)
            {
                UpdateChunks();
            }

            prevChunkPosition = currentChunkPosition;

            yield return new WaitForSeconds(checkInterval);
        }        
    }
}
