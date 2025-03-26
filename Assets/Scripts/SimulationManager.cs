using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public GameObject SimulationPrefab;
    int camIdx = 0;
    GameObject[] sims = new GameObject[5];
    Camera cur;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Instantiate simulations
        GameObject simulation1 = Instantiate(SimulationPrefab, new Vector3(0,0,0), Quaternion.identity);
        // simulation1.SetActive(true);
        GameObject simulation2 = Instantiate(SimulationPrefab, new Vector3(10000,0,10000), Quaternion.identity);
        simulation2.SetActive(true);
        sims[0] = simulation1;
        sims[1] = simulation2;
        cur = simulation1.GetComponentInChildren<Camera>(true);
        if (cur == null){
            Debug.Log("cur is null");
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
		{
			camIdx++;
            Camera c = sims[camIdx % 5].GetComponentInChildren<Camera>(true);
            if (c == null){
                Debug.Log($"camera for scene {camIdx % 5} is null");
            }
            cur.gameObject.SetActive(false);
            c.gameObject.SetActive(true);
            cur = c;
		}
    }
}
