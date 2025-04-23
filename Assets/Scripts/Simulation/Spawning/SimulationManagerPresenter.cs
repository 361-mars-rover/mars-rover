using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

/*
JIKAEL
Creates simulations from the list of avatars and spawnpoints and sets them as the model data. Also
handles switching between simulations and making UI updates.
*/
public class SimulationManagerPresenter : MonoBehaviour
{
    public GameObject SimulationPrefab;

    public Vector2Int[] TileIndices;

    private int MAX_ROW = 128;
    private int MAX_COL = 256;
    // List<GameObject> sims = new List<GameObject>();
    public int curIdx = 0;
    // public List<string> model.roverIds = new List<string>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private SimulationManagerModel model;
    private SimulationManagerView view;

    public float keyCooldown = 0.1f; // time in seconds between allowed presses
    private float nextAllowedPressTime = 0f;
    public float loadCooldown = 3f;

    private bool[] hasBeenLoaded = new bool[AvatarTableManager.avatars.Count];

    void Awake()
    {
        model = FindFirstObjectByType<SimulationManagerModel>();
        view = FindFirstObjectByType<SimulationManagerView>();

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
            model.TileIndices.Add(new Vector2Int(row, col));
            GameObject sim = Instantiate(SimulationPrefab, new Vector3(0,0,0), Quaternion.identity);
            SimulationStart startupSpawner = sim.GetComponent<SimulationStart>();
            startupSpawner.SetRowCol(row,col);
            var mineralSpawner = sim.GetComponentInChildren<MineralSpawner>();
            if (mineralSpawner == null) {
                Debug.LogError("[SimulationManagerPresenter] MineralSpawner component not found on prefab!");
            } else {
                mineralSpawner.Init(row, col);
            }
            hasBeenLoaded[i] = false;

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
        model.sims[0].SetActive(true);
        UpdateUI(0);
    }

    
    void Update()
    {
        int simIdx;

        // Make sure car is on the ground before being allowed to switch
        if (!model.sims[curIdx].GetComponent<SimulationStart>().carIsGrounded){
            return;
        }
        else{
            hasBeenLoaded[curIdx] = true;
        }

        if (Time.time >= nextAllowedPressTime)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                simIdx = Math.Max(curIdx-1, 0);
                SwitchSimulation(simIdx);
                if (!hasBeenLoaded[simIdx]){
                    nextAllowedPressTime = Time.time + loadCooldown;
                }
                else{
                    nextAllowedPressTime = Time.time + keyCooldown;
                }
                
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                simIdx = Math.Min(curIdx+1, AvatarTableManager.avatars.Count - 1);
                Debug.Log($"sim index set to {simIdx}");
                SwitchSimulation(simIdx);
                if (!hasBeenLoaded[simIdx]){
                    nextAllowedPressTime = Time.time + loadCooldown;
                }
                else{
                    nextAllowedPressTime = Time.time + keyCooldown;
                }
            }
        }
        

    }

    void UpdateUI(int idx){
        view.SetRoverID(model.roverIds[idx]);
        view.SetTileIndices(model.TileIndices[idx]);
        view.SetAIMode(AvatarTableManager.avatars[idx].brain.aIMode);
    }

    private void SwitchSimulation(int simIdx){
        // Don't switch if index hasnt changed
        if (simIdx == curIdx){
            return;
        }
        model.sims[curIdx].SetActive(false);
        model.sims[simIdx].SetActive(true);
        curIdx = simIdx;
        UpdateUI(simIdx);
    }

    private bool IsValidRowCol(int row, int col){
        return (row >= 0 && row < MAX_ROW) && (col >= 0 && col <= MAX_COL);
    }
}
