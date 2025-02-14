using System.Collections;
using UnityEngine;

public class ChunkHandler : MonoBehaviour
{
    public GameObject car;
    public GameObject[] chunks;
    public float activationDistance = 1500.0f;
    public float checkInterval = 0.1f;
    void Start()
    {
        StartCoroutine(CheckChunkDistance());
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
                Debug.Log($"Distance from chunk {distanceToCar}");
                Debug.Log($"This chunk is currently active: {chunk.activeSelf}");

                if (distanceToCar <= activationDistance)
                {
                    chunk.SetActive(true);
                    Debug.Log($"Chunk set active {chunk}");

                }
                else
                {
                    chunk.SetActive(false);
                    Debug.Log($"Chunk set non-active {chunk}");

                }
            }
            yield return new WaitForSeconds(checkInterval);
        }        
    }
}
