using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimulationManager : MonoBehaviour
{
    public GameObject SimulationPrefab;

    public Vector2Int[] TileIndices;
    private int prevIdx = 0;
    private int MAX_ROW = 128;
    private int MAX_COL = 256;
    GameObject[] sims = new GameObject[9];
    public int curIdx = 0;
    public string[] roverIds = new string[9];

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
            sim.GetComponent<SimulationStart>().SetRowCol(row,col);
            // sim.SetActive(true);
            sims[i] = sim;
            roverIds[i] = "Rover" + i + "-" + Guid.NewGuid().ToString();
        }
        // Start at the first sim
        sims[0].SetActive(true);
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
            SwitchSimulation(simIdx);
        }

    }

    private void SwitchSimulation(int simIdx){
        // Don't switch if index hasnt changed
        if (simIdx == prevIdx){
            return;
        }
        sims[prevIdx].SetActive(false);
        sims[simIdx].SetActive(true);
        prevIdx = simIdx;
    }

    private bool IsValidRowCol(int row, int col){
        return (row >= 0 && row < MAX_ROW) && (col >= 0 && col <= MAX_COL);
    }
}
