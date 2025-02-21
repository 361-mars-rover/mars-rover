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
    
    public float terrainWidth = 78183.74f;
    public float terrainHeight = 78183.74f;
    
    // Loads initial chunks based on car position
    void Start()
    {

        Debug.Log("Chunk script started");
        prevChunkPosition = GetClosestChunkCenter(car.transform.position);
        Vector3[] chunksToLoad = GetChunksToLoad(car.transform.position);
        

        // currentChunks[0] = Instantiate(terrainPrefab, prevChunkPosition, Quaternion.identity);
        foreach(Vector3 chunkPos in chunksToLoad)
        {
            var (row, col) = GetRowColFromPosition(chunkPos);
            if (row < 0 || col < 0) continue;
            Debug.Log($"Position: {chunkPos}");
            Debug.Log($"row: {row} col: {col}");

            GameObject chunk = Instantiate(terrainPrefab, chunkPos, Quaternion.identity);

            chunk.GetComponent<TerrainChunk>().Inititialize(row, col);

            loadedChunks[chunkPos] = chunk;
        }

        StartCoroutine(CheckChunkDistance());
    }
    
    // Find the position of the nearest chunk center relative to the car's position
    Vector3 GetClosestChunkCenter(Vector3 position)
    {
        // This gets bottom left corner of the tile
        float x = Mathf.Floor(position.x / terrainWidth) * terrainWidth;
        float z = Mathf.Floor(position.z / terrainHeight) * terrainHeight;

        return new Vector3(x, 0, z);
    }
    // Gets the surrounding 8 chunk positions
    Vector3[] GetChunksToLoad(Vector3 position)
    {
        Vector3 nearestChunkPositon = GetClosestChunkCenter(position);
        Vector3[] chunksToLoad = new Vector3[9];
        chunksToLoad[0] = nearestChunkPositon + Vector3.forward * terrainHeight; // Above
        chunksToLoad[1] = nearestChunkPositon + Vector3.back * terrainHeight; // Below
        chunksToLoad[2] = nearestChunkPositon + Vector3.right * terrainWidth; // Right
        chunksToLoad[3] = nearestChunkPositon + Vector3.left * terrainWidth; // Left
        chunksToLoad[4] = nearestChunkPositon + Vector3.forward * terrainHeight + Vector3.right * terrainWidth;  //  Top right
        chunksToLoad[5] = nearestChunkPositon + Vector3.forward * terrainHeight + Vector3.left * terrainWidth; // Top left
        chunksToLoad[6] = nearestChunkPositon + Vector3.back * terrainHeight + Vector3.right * terrainWidth; // Back right
        chunksToLoad[7] = nearestChunkPositon + Vector3.back * terrainHeight + Vector3.left * terrainWidth; // Back left
        chunksToLoad[8] = nearestChunkPositon; // Current chunk
        return chunksToLoad;
    }

    // Takes a chunk center position and returns the corresponding row and column of the tile
    (int, int) GetRowColFromPosition(Vector3 position)
    {
        int row = (int) Math.Round((position.z - (terrainHeight / 2)) / terrainHeight);
        int col = (int) Math.Round((position.x - (terrainHeight / 2)) / terrainHeight);
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
                    chunk.GetComponent<TerrainChunk>().Inititialize(row, col);
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
