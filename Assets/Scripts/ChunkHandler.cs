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
            
            // Debug.Log($"Position: {chunkPos}");
            GetRowColFromPosition(chunkPos);
        }

        StartCoroutine(CheckChunkDistance());
    }
    
    // Find the position of the nearest chunk center relative to the car's position
    Vector3 GetClosestChunkCenter(Vector3 position)
    {
        // This gets bottom left corner
        float x = Mathf.Floor(position.x / terrainWidth) * terrainWidth;
        float z = Mathf.Floor(position.z / terrainHeight) * terrainHeight;
        // Now get center
        x += terrainWidth / 2;
        z += terrainHeight / 2;
        return new Vector3(x, 0, z);
    }
    // Gets the surrounding 8 chunk positions
    Vector3[] GetChunksToLoad(Vector3 position)
    {
        Vector3 nearestChunkPositon = GetClosestChunkCenter(position);
        Vector3[] chunksToLoad = new Vector3[9];
        chunksToLoad[0] = nearestChunkPositon + Vector3.forward * terrainHeight;
        chunksToLoad[1] = nearestChunkPositon + Vector3.back * terrainHeight;
        chunksToLoad[2] = nearestChunkPositon + Vector3.right * terrainWidth;
        chunksToLoad[3] = nearestChunkPositon + Vector3.left * terrainWidth;
        chunksToLoad[4] = nearestChunkPositon + Vector3.forward * terrainHeight + Vector3.right * terrainWidth; 
        chunksToLoad[5] = nearestChunkPositon + Vector3.forward * terrainHeight + Vector3.left * terrainWidth;
        chunksToLoad[6] = nearestChunkPositon + Vector3.back * terrainHeight + Vector3.right * terrainWidth;
        chunksToLoad[7] = nearestChunkPositon + Vector3.back * terrainHeight + Vector3.left * terrainWidth;
        chunksToLoad[8] = nearestChunkPositon;
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

            // Check to see if new chunks need to be loaded
            if (currentChunkPosition != prevChunkPosition)
            {
                // Debug.Log("Position updated!");
                prevChunkPosition = currentChunkPosition;
                // Vector3[] nearestNeighbours = getChunksToLoad(car.transform.position);
            }

            HashSet<Vector3> currentChunkPositions = new HashSet<Vector3>(loadedChunks.Keys);
            HashSet<Vector3> chunksToLoad = new HashSet<Vector3>(GetChunksToLoad(car.transform.position));

            HashSet<Vector3> newChunkPositions = new HashSet<Vector3>(chunksToLoad.Except(currentChunkPositions));
            HashSet<Vector3> chunksToDelete = new HashSet<Vector3>(currentChunkPositions.Except(chunksToLoad));

            foreach (Vector3 newChunkPos in newChunkPositions)
            { 
                var (row, col) = GetRowColFromPosition(newChunkPos);
                if (row < 0 || col < 0) continue;
                GameObject chunk = Instantiate(terrainPrefab, newChunkPos, Quaternion.identity);
                chunk.GetComponent<TerrainChunk>().Inititialize(row, col);
                loadedChunks[newChunkPos] = chunk;
            }

            foreach (Vector3 deletePos in chunksToDelete)
            {
                Destroy(loadedChunks[deletePos]);
                loadedChunks.Remove(deletePos);
            }

            // float h = loadedChunks[Vector3.zero].GetComponent<Terrain>().SampleHeight(new Vector3(39091.87f, 0, 39091.87f));

            // Debug.Log($"height at zero: {h}");
            yield return new WaitForSeconds(checkInterval);
        }        
    }
}
