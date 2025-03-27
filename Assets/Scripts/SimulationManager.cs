using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimulationManager : MonoBehaviour
{
    public GameObject SimulationPrefab;
    private float TerrainWidth = 1563.675f;
    int prevIdx = -1;
    GameObject[] sims = new GameObject[5];
    public int[,] RowColPairs = new int[2,2];
    Camera cur;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Instantiate simulations
        GameObject simulation1 = Instantiate(SimulationPrefab, new Vector3(0,0,0), Quaternion.identity);
        simulation1.GetComponent<StartupSpawner>().SetRowCol(1,1);
        simulation1.SetActive(true);
        GameObject simulation2 = Instantiate(SimulationPrefab, new Vector3(TerrainWidth,0,0), Quaternion.identity);
        simulation2.GetComponent<StartupSpawner>().SetRowCol(11,11);
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
        // int curIdx = System.Convert.ToInt32(camIdx);
        int keyPress = -1;
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha0 + i)))
            {
                Debug.Log($"Pressed {i}");
                keyPress = i;
            }
        }
        if (keyPress > 0 && keyPress <= sims.Length)
		{
            Debug.Log($"You pressed {keyPress}!");
            GameObject curSim = sims[keyPress - 1];
            Transform child = curSim.transform.Find("CarCamera");
            if (child != null)
            {
                Debug.Log("CarCamera child found!");
                Camera myCarCamera = child.GetComponent<Camera>();
                if (myCarCamera != null){
                    Debug.Log("CarCamera child componenent found!");
                }
                myCarCamera.gameObject.SetActive(true);
                Debug.Log($"CarCamera for sim {keyPress} set as active!");
                cur.gameObject.SetActive(false);
                cur = myCarCamera;
                cur.gameObject.SetActive(true);

                if (prevIdx > -1){
                    GameObject prevSim = sims[prevIdx];
                    CarControl prevCarControl = prevSim.GetComponentInChildren<CarControl>();
                    prevCarControl.allowInputs = false;  // Disable input
                    // prevSim.transform.Find("EventSystem").GetComponent<EventSystem>().enabled = false;
                    Debug.Log("Disabled previous inputs system");
                }

                // enable input
                // EventSystem eventSystem = curSim.transform.Find("EventSystem").GetComponent<EventSystem>();
                // eventSystem.enabled = true;   // Enable input
                CarControl curCarControl = curSim.GetComponentInChildren<CarControl>();
                curCarControl.allowInputs = true;  // Disable input
                Debug.Log("Enabled new event system");

                prevIdx = keyPress - 1;

                // camIdx = !camIdx;
                // do something with myCarCamera
            }
            else
            {
                Debug.LogError($"CarCamera for sim {keyPress} not found!");
            }
		}
    }
}
