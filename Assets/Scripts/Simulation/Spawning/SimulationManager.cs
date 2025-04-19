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

    private int MAX_ROW = 128;
    private int MAX_COL = 256;
    // List<GameObject> sims = new List<GameObject>();
    public int curIdx = 0;
    // public List<string> model.roverIds = new List<string>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public SimulationManagerModel model;
    void Awake()
    {
        model = FindFirstObjectByType<SimulationManagerModel>();
    }
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
            SimulationStart startupSpawner = sim.GetComponent<SimulationStart>();
            startupSpawner.SetRowCol(row,col);

                        // Get the "car" child object
            Transform carTransform = sim.transform.Find("car");

            // Now get the CarControl component from the car object
            CarControl carControl = carTransform.GetComponent<CarControl>();
            carControl.PrepareAIControllers(startupSpawner);
            carControl.SetAI(a.brain.aIMode);
            Debug.Log($"Car control: {carControl}");
            CarColorUtils.SetCarColor(carTransform, a.rover.color);
            // sim.SetActive(true);
            model.sims.Add(sim);
            model.roverIds.Add("Rover" + i + "-" + Guid.NewGuid().ToString());
        }

        // SetActivity(simIdx: 1, active: false);
        // sims[0].SetActive(true);
        // SetActivity(simIdx: 0, active: true);
        model.sims[0].SetActive(true);
    }

    
    void Update()
    {
        int simIdx;
        if (Input.GetKeyDown(KeyCode.K))
        {
            simIdx = Math.Max(curIdx-1, 0);
            SwitchSimulation(simIdx);
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            simIdx = Math.Min(curIdx+1, AvatarTableManager.avatars.Count - 1);
            Debug.Log($"sim index set to {simIdx}");
            SwitchSimulation(simIdx);
        }

    }

    private void SwitchSimulation(int simIdx){
        // Don't switch if index hasnt changed
        if (simIdx == curIdx){
            return;
        }
        model.sims[curIdx].SetActive(false);
        model.sims[simIdx].SetActive(true);
        curIdx = simIdx;
    }

    private bool IsValidRowCol(int row, int col){
        return (row >= 0 && row < MAX_ROW) && (col >= 0 && col <= MAX_COL);
    }
}
