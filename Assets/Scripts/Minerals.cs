using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minerals : MonoBehaviour
{
    public void OnTriggerEnter(Collider other) {
        PlayerInventory playerInventory = other.GetComponent<PlayerInventory>();

        if (playerInventory != null) {
            playerInventory.MineralCollected();
            gameObject.SetActive(false);
        }
    }
}