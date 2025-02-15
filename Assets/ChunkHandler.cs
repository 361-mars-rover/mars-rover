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

    void Start()
    {
        prevChunkPosition = getClosestChunkCenter(car.transform.position);
        Vector3[] chunksToLoad = getChunksToLoad(car.transform.position);
        

        // currentChunks[0] = Instantiate(terrainPrefab, prevChunkPosition, Quaternion.identity);
        foreach(Vector3 chunkPos in chunksToLoad)
        {
            var (row, col) = GetRowColFromPosition(chunkPos);
            if (row < 0 || col < 0) continue;
            Debug.Log($"Position: {chunkPos}");
            Debug.Log($"row: {row} col: {col}");

            GameObject chunk = Instantiate(terrainPrefab, chunkPos, Quaternion.identity);
            chunk.GetComponent<MarsGlobalTerrain>().Inititialize(row, col);
            loadedChunks[chunkPos] = chunk;
            
            // Debug.Log($"Position: {chunkPos}");
            GetRowColFromPosition(chunkPos);
        }

        StartCoroutine(CheckChunkDistance());
    }
    
    // Find the position of the nearest chunk
    Vector3 getClosestChunkCenter(Vector3 position)
    {
        // This gets bottom left corner
        float x = Mathf.Floor(position.x / terrainWidth) * terrainWidth;
        float z = Mathf.Floor(position.z / terrainHeight) * terrainHeight;
        // Now get center
        x += terrainWidth / 2;
        z += terrainHeight / 2;
        return new Vector3(x, 0, z);
    }

    Vector3[] getChunksToLoad(Vector3 position)
    {
        Vector3 nearestChunkPositon = getClosestChunkCenter(position);
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

    (int, int) GetRowColFromPosition(Vector3 position)
    {
        int row = (int) Math.Round((position.z - (terrainHeight / 2)) / terrainHeight);
        int col = (int) Math.Round((position.x - (terrainHeight / 2)) / terrainHeight);
        // Debug.Log($"row: {row} col: {col}");

        return (row,col);
    }

    IEnumerator CheckChunkDistance()
    {
        while(true)
        {
            // foreach (GameObject chunk in chunks)
            // {
            //     // Debug.Log($"Viewing chunk {chunk}");
            //     // Debug.Log($"Chunk position: {chunk.transform.position}");
            //     float distanceToCar = Vector3.Distance(car.transform.position, chunk.transform.position);
            //     // Debug.Log($"Distance from chunk {distanceToCar}");
            //     // Debug.Log($"This chunk is currently active: {chunk.activeSelf}");

            //     if (distanceToCar <= activationDistance)
            //     {
            //         chunk.SetActive(true);
            //         // Debug.Log($"Chunk set active {chunk}");

            //     }
            //     else
            //     {
            //         chunk.SetActive(false);
            //         // Debug.Log($"Chunk set non-active {chunk}");

            //     }
            // }
            // Debug.Log($"Closest chunk: {getClosestChunkCenter(car.transform.position)}");
            // Vector3[] nearestNeighbours = getChunksToLoad(car.transform.position);
            // // Debug.Log("Neighbouurs");
            // foreach(Vector3 v in nearestNeighbours){
            //     Debug.Log(v.ToString());
            // }

            Vector3 currentChunkPosition = getClosestChunkCenter(car.transform.position);
            // Debug.Log("Position data!");
            // Debug.Log(currentChunkPosition);
            // Debug.Log(prevChunkPosition);

            if (currentChunkPosition != prevChunkPosition)
            {
                // Debug.Log("Position updated!");
                prevChunkPosition = currentChunkPosition;
                // Vector3[] nearestNeighbours = getChunksToLoad(car.transform.position);
                
            }

            HashSet<Vector3> currentChunkPositions = new HashSet<Vector3>(loadedChunks.Keys);
            HashSet<Vector3> chunksToLoad = new HashSet<Vector3>(getChunksToLoad(car.transform.position));

            HashSet<Vector3> newChunkPositions = new HashSet<Vector3>(chunksToLoad.Except(currentChunkPositions));
            HashSet<Vector3> chunksToDelete = new HashSet<Vector3>(currentChunkPositions.Except(chunksToLoad));

            foreach (Vector3 newChunkPos in newChunkPositions)
            { 
                var (row, col) = GetRowColFromPosition(newChunkPos);
                if (row < 0 || col < 0) continue;
                GameObject chunk = Instantiate(terrainPrefab, newChunkPos, Quaternion.identity);
                chunk.GetComponent<MarsGlobalTerrain>().Inititialize(row, col);
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
