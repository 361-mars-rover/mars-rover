using UnityEngine;

public class Minerals : MonoBehaviour
{
    // Handles collisions with Minerals
    public void OnTriggerEnter(Collider other)
    {
        InventoryPresenter presenter = other.GetComponent<InventoryPresenter>();
        // InventoryPresenter presenter = other.GetComponent<InventoryPresenter>();


        if (presenter != null)
        {
            // Call presenter to notify that mineral was collected
            presenter.CollectMineral(gameObject);
            // FindObjectOfType<InventoryPresenter>().CollectMineral(gameObject);
            gameObject.SetActive(false);
        }
    }
}
