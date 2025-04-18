using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryModel
{
    public event Action<InventoryModel> OnMineralCollected;

    private List<GameObject> minerals = new List<GameObject>();
    private int numberOfMinerals = 0;

    public IReadOnlyList<GameObject> Minerals => minerals;
    public int NumberOfMinerals => numberOfMinerals;

    public void CollectMineral(GameObject mineral)
    {
        minerals.Add(mineral);
        numberOfMinerals++;
        // Notifies listeners for the OnMineralCollected event, if it's defined
        OnMineralCollected?.Invoke(this);
    }

    public void Reset()
    {
        minerals.Clear();
        numberOfMinerals = 0;
        // Notifies listeners for the OnMineralCollected event, if it's defined
        OnMineralCollected?.Invoke(this);
    }
}
