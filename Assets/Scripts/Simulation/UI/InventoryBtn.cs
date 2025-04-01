using UnityEngine;
using UnityEngine.UI;

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
