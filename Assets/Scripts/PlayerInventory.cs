using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int NumberOfMinerals {get; private set;}
    public ArrayList Minerals = new ArrayList();

    public void MineralCollected(object mineral) {
        Minerals.Add(mineral);
        NumberOfMinerals++;
    }
}