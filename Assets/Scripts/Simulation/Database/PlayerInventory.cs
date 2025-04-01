using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class PlayerInventory : MonoBehaviour
{
    public List<GameObject> Minerals = new List<GameObject>(); // ✅ Ensure list is initialized
    public int NumberOfMinerals = 0;
    public UnityEvent<PlayerInventory> OnMineralCollected = new UnityEvent<PlayerInventory>(); // ✅ Ensure event is initialized
    public SimulationManager simulationManager;
    public void MineralCollected(GameObject mineral)
    {
        simulationManager = FindObjectOfType<SimulationManager>();
        Debug.Log("Mineral collected for: " + simulationManager.roverIds[simulationManager.curIdx]);
        Minerals.Add(mineral);
        NumberOfMinerals++;

        Debug.Log($"✅ Total Minerals Collected: {NumberOfMinerals}");

        if (OnMineralCollected != null)
        {
            OnMineralCollected.Invoke(this);
        }
        else
        {
            Debug.LogError("❌ OnMineralCollected event is null!");
        }

        if (FirebaseManager.dbReference != null)
        {
            FirebaseManager.StoreMaterialData(mineral, simulationManager.roverIds[simulationManager.curIdx]);
        }
        else
        {
            Debug.LogError("❌ FirebaseManager.Instance is null!");
        }
    }
}


