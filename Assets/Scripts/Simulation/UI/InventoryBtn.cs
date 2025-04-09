using UnityEngine;
using UnityEngine.UI;

public class InventoryButton : MonoBehaviour
{
    public InventoryPresenter inventoryPresenter;
    public bool isOpen;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => {
        if (isOpen)
        {
            isOpen = false;
            inventoryPresenter.CloseInventory();
        }
        else
        {
            isOpen = true;
            inventoryPresenter.FetchMinerals();
        }});
        
    }
}
