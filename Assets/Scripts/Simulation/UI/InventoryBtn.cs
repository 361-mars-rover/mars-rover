using UnityEngine;
using UnityEngine.UI;
// Chloe Gavrilovic 260955835

// inventory button functionality in UI that toggles the inventory panel and fetches minerals when opened
public class InventoryButton : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public bool isOpen;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => {
        if (isOpen)
        {
            isOpen = false;
            inventoryManager.CloseInventory();
        }
        else
        {
            isOpen = true;
            inventoryManager.FetchMinerals();
        }});
        
    }
}
