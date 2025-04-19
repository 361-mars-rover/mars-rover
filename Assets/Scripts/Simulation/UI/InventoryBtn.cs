using UnityEngine;
using UnityEngine.UI;

// toggles inventory open/closed via the Presenter
public class InventoryButton : MonoBehaviour
{
    public InventoryPresenter inventoryPresenter;
    private bool isOpen = false;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            isOpen = !isOpen;
            inventoryPresenter.ToggleInventory();
        });
    }
}
