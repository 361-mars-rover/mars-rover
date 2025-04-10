using UnityEngine;
using TMPro;

public class MineralsCollectedCountUI : MonoBehaviour
{
    private TextMeshProUGUI mineralText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mineralText = GetComponent<TextMeshProUGUI>();
    }

    public void UpdateMineralText(PlayerInventory playerInventory){
        mineralText.text = $"Minerals collected: {playerInventory.NumberOfMinerals}";
    }
}
