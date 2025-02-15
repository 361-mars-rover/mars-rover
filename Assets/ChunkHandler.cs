using System.Collections;
using UnityEngine;

public class ChunkHandler : MonoBehaviour
{
    public GameObject car;
    public GameObject[] chunks;
    public Vector3[] chunkPositions;

    public GameObject[] loadedChunks;

    public Vector3 prevChunkPosition;
    public float activationDistance = 1500.0f;
    public float checkInterval = 0.1f;
    public GameObject terrainPrefab;
    
    public float terrainWidth = 1000f;
    public float terrainHeight = 1000f;
    
    void Start()
    {
        prevChunkPosition = getClosestChunkPosition(car.transform.position);
        StartCoroutine(CheckChunkDistance());
    }
    
    // Find the position of the nearest chunk
    Vector3 getClosestChunkPosition(Vector3 position)
    {
        float x = Mathf.Floor(position.x / terrainWidth) * terrainWidth;
        float z = Mathf.Floor(position.z / terrainHeight) * terrainHeight;
        return new Vector3(x, 0, z);
    }

    Vector3[] getNeighbourChunkPositions(Vector3 position)
    {
        Vector3 nearestChunkPositon = getClosestChunkPosition(position);
        Vector3[] neighbours = new Vector3[8];
        neighbours[0] = nearestChunkPositon + Vector3.forward * terrainHeight;
        neighbours[1] = nearestChunkPositon + Vector3.back * terrainHeight;
        neighbours[2] = nearestChunkPositon + Vector3.right * terrainWidth;
        neighbours[3] = nearestChunkPositon + Vector3.left * terrainWidth;
        neighbours[4] = nearestChunkPositon + Vector3.forward * terrainHeight + Vector3.right * terrainWidth; 
        neighbours[5] = nearestChunkPositon + Vector3.forward * terrainHeight + Vector3.left * terrainWidth;
        neighbours[6] = nearestChunkPositon + Vector3.back * terrainHeight + Vector3.right * terrainWidth;
        neighbours[7] = nearestChunkPositon + Vector3.back * terrainHeight + Vector3.left * terrainWidth;
        return neighbours;
    }

    IEnumerator CheckChunkDistance()
    {
        while(true)
        {
            foreach (GameObject chunk in chunks)
            {
                // Debug.Log($"Viewing chunk {chunk}");
                // Debug.Log($"Chunk position: {chunk.transform.position}");
                float distanceToCar = Vector3.Distance(car.transform.position, chunk.transform.position);
                // Debug.Log($"Distance from chunk {distanceToCar}");
                // Debug.Log($"This chunk is currently active: {chunk.activeSelf}");

                if (distanceToCar <= activationDistance)
                {
                    chunk.SetActive(true);
                    // Debug.Log($"Chunk set active {chunk}");

                }
                else
                {
                    chunk.SetActive(false);
                    // Debug.Log($"Chunk set non-active {chunk}");

                }
            }
            // Debug.Log($"Closest chunk: {getClosestChunkPosition(car.transform.position)}");
            // Vector3[] nearestNeighbours = getNeighbourChunkPositions(car.transform.position);
            // // Debug.Log("Neighbouurs");
            // foreach(Vector3 v in nearestNeighbours){
            //     Debug.Log(v.ToString());
            // }

            Vector3 currentChunkPosition = getClosestChunkPosition(car.transform.position);
            Debug.Log("Position data!");
            Debug.Log(currentChunkPosition);
            Debug.Log(prevChunkPosition);

            if (currentChunkPosition != prevChunkPosition)
            {
                Debug.Log("Position updated!");
                prevChunkPosition = currentChunkPosition;
            }


            yield return new WaitForSeconds(checkInterval);
        }        
    }
}
