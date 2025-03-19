using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int NumberOfMinerals {get; private set;}

    public void MineralCollected() {
        NumberOfMinerals++;
    }
}