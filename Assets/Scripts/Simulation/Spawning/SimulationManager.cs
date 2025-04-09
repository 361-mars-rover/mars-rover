using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class SimulationManager : MonoBehaviour
{
    public GameObject SimulationPrefab;

    public Vector2Int[] TileIndices;
    int prevIdx = 0;

    private int MAX_ROW = 128;
    private int MAX_COL = 256;
    GameObject[] sims = new GameObject[9];
    public int curIdx = 0;
    public string[] roverIds = new string[9];
    Camera cur;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Printing existing avatars:...");

        for (int i = 0; i < AvatarTableManager.avatars.Count; i++){
            Avatar a = AvatarTableManager.avatars[i];
            Debug.Log($"Setting up simulation for rover {a.description}");
            int row = a.SpawnRowCol.y;
            int col = a.SpawnRowCol.x;
            if (!IsValidRowCol(row, col)){
                Debug.LogError($"Row col pair {row},{col} is invalid");
            }
            GameObject sim = Instantiate(SimulationPrefab, new Vector3(0,0,0), Quaternion.identity);
            StartupSpawner startupSpawner = sim.GetComponent<StartupSpawner>();
            startupSpawner.SetRowCol(row,col);

                        // Get the "car" child object
            Transform carTransform = sim.transform.Find("car");

            // Now get the CarControl component from the car object
            CarControl carControl = carTransform.GetComponent<CarControl>();
            carControl.PrepareAIControllers(startupSpawner);
            carControl.SetAI(AIMode.SunlightAI);
            Debug.Log($"Car control: {carControl}");
            // sim.SetActive(true);
            sims[i] = sim;
            roverIds[i] = "Rover" + i + "-" + Guid.NewGuid().ToString();
        }

        // SetActivity(simIdx: 1, active: false);
        // sims[0].SetActive(true);
        SetActivity(simIdx: 0, active: true);
    }
    void Update()
    {
        int simIdx;
        if (Input.GetKeyDown(KeyCode.K))
        {
            simIdx = Math.Max(prevIdx-1, 0);
            SwitchSimulation(simIdx);
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            simIdx = Math.Min(prevIdx+1, AvatarTableManager.avatars.Count - 1);
            Debug.Log($"sim index set to {simIdx}");
            // Debug.Log($"Sim length is ${sims.Length}");
            SwitchSimulation(simIdx);
        }

    }

    private Camera SetActivity(int simIdx, bool active){
        Debug.Log($"Setting simulation: ${simIdx} to have activity: ${active}");
        GameObject sim = sims[simIdx];
        sim.SetActive(active);
        Transform child = sim.transform.Find("CarCamera");   
        if (child != null)
        {
            Camera simCamera = child.GetComponent<Camera>();
            if (simCamera == null){
                Debug.Log("CarCamera is null");
            }
            // Set whether camera should be active or inactive
            simCamera.gameObject.SetActive(active);
            // Debug.Log($"CarCamera for sim {simIdx + 1} set as active!");
            cur = simCamera;
            cur.gameObject.SetActive(active);

            // enable/disable input
            // EventSystem eventSystem = sim.transform.Find("EventSystem").GetComponent<EventSystem>();
            // eventSystem.enabled = active;
            
            EventSystem[] eventSystems = sim.GetComponentsInChildren<EventSystem>(true);

            Debug.Log($"Event systems length: {eventSystems.Length}");

            foreach (EventSystem es in eventSystems)
            {
                es.enabled = active;
            }
            // EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();

            // for (int i = 1; i < eventSystems.Length; i++)
            // {
            //     Destroy(eventSystems[i].gameObject);  // Destroy all but the first one
            // }
            // if (eventSystem == null){
            //     Debug.Log("event system is null");
            // }
            Canvas canvas = sim.transform.Find("Canvas").GetComponent<Canvas>();
            if (canvas == null){
                Debug.Log("canvas is null");
            }
            canvas.enabled = active;
            CarControl carControl = sim.transform.Find("car").GetComponent<CarControl>();
            // if (carControl == null){
            //     Debug.Log("car control is null");
            // }

            // carControl.allowInputs = active;  // Disable input
            prevIdx =  simIdx;
            return cur;
        }
        else
        {
            Debug.LogError($"CarCamera for sim {simIdx + 1} not found!");
            return null;
        }
    }

    private void SwitchSimulation(int simIdx){
        curIdx = simIdx;
        // Don't switch if index hasnt changed
        if (simIdx == prevIdx){
            return;
        }
        // Get the simulation for the selected index
        Camera oldCamera = SetActivity(prevIdx, active: false);
        if (oldCamera == null){
            Debug.LogError($"Failed to disactive {prevIdx + 1}");
        }
        Camera newCamera = SetActivity(simIdx, active: true);
        if (newCamera == null){
            Debug.LogError($"Failed to disactive {simIdx + 1}");
        }
        prevIdx = simIdx;
    }

    private bool IsValidRowCol(int row, int col){
        return (row >= 0 && row < MAX_ROW) && (col >= 0 && col <= MAX_COL);
    }
}
