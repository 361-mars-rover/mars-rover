using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventory : MonoBehaviour
{
    public List<GameObject> Minerals = new List<GameObject>(); // ✅ Ensure list is initialized
    public int NumberOfMinerals = 0;
    public UnityEvent<PlayerInventory> OnMineralCollected = new UnityEvent<PlayerInventory>(); // ✅ Ensure event is initialized

    public void MineralCollected(GameObject mineral)
    {
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

        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.StoreMaterialData(mineral.name);
        }
        else
        {
            Debug.LogError("❌ FirebaseManager.Instance is null!");
        }
    }
}


