using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public GameObject SimulationPrefab;
    private float TerrainWidth = 1563.675f;
    bool camIdx = true;
    GameObject[] sims = new GameObject[5];
    Camera cur;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Instantiate simulations
        GameObject simulation1 = Instantiate(SimulationPrefab, new Vector3(0,0,0), Quaternion.identity);
        simulation1.SetActive(true);
        GameObject simulation2 = Instantiate(SimulationPrefab, new Vector3(TerrainWidth,0,0), Quaternion.identity);
        simulation2.SetActive(true);
        sims[0] = simulation1;
        sims[1] = simulation2;
        // cur = simulation1.GetComponent<Camera>();
        // Debug.Log("Trying to get camera for simulation1");
        // if (cur == null){
        //     Debug.Log("cur is null");
        // }
        Transform child = simulation1.transform.Find("CarCamera");
        if (child != null)
        {
            Debug.Log("CarCamera child found!");
            Camera myCarCamera = child.GetComponent<Camera>();
            if (myCarCamera != null){
                Debug.Log("CarCamera child componenent found!");
            }
            cur = myCarCamera;
            Debug.Log($"Setting SIM 1 camera as active");
            cur.gameObject.SetActive(true);
            // do something with myCarCamera
        }
        else
        {
            Debug.LogError("CarCamera child not found!");
        }
    }
    void Update()
    {
        int curIdx = System.Convert.ToInt32(camIdx);
        if (Input.GetKeyDown(KeyCode.J))
		{
            Debug.Log("You pressed J!");
            GameObject curSim = sims[curIdx];
            Transform child = curSim.transform.Find("CarCamera");
            if (child != null)
            {
                Debug.Log("CarCamera child found!");
                Camera myCarCamera = child.GetComponent<Camera>();
                if (myCarCamera != null){
                    Debug.Log("CarCamera child componenent found!");
                }
                myCarCamera.gameObject.SetActive(true);
                Debug.Log($"CarCamera for sim {curIdx} set as active!");
                cur.gameObject.SetActive(false);
                cur = myCarCamera;
                camIdx = !camIdx;
                // do something with myCarCamera
            }
            else
            {
                Debug.LogError($"CarCamera for sim {curIdx} not found!");
            }
		}
    }
}
