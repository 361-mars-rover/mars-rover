using UnityEngine;
using TMPro;

public class InventoryView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mineralText;

    public void UpdateMineralCount(int count)
    {
        if (mineralText != null)
        {
            Debug.Log("Mineral text found");

            mineralText.text = $"Minerals collected: {count}";
        }
    }
}
