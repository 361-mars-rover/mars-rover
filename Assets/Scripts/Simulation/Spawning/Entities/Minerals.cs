using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Chloe Gavrilovic 260955835

// handle the collection of minerals 
public class Minerals : MonoBehaviour
{
    public void OnTriggerEnter(Collider other) {
        PlayerInventory playerInventory = other.GetComponent<PlayerInventory>();

        if (playerInventory != null) {
            playerInventory.MineralCollected(gameObject);
            gameObject.SetActive(false);
        }
    }
}