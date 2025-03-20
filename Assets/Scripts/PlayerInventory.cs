using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventory : MonoBehaviour
{
    public int NumberOfMinerals {get; private set;}
    public ArrayList Minerals = new ArrayList();

    public UnityEvent<PlayerInventory> OnMineralCollected;

    public void MineralCollected(object mineral) {
        Minerals.Add(mineral);
        NumberOfMinerals++;
        OnMineralCollected.Invoke(this);
    }
}