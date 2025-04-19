using TMPro;
using UnityEngine;
using UnityEngine.UI;
/*
Authors: Jikael and Chloe
This is the view part of the model-view-presenter design pattern. In this design pattern, the view is the actual UI. 
*/
public class InventoryView : MonoBehaviour
{
    [SerializeField] private GameObject mineralPanelPrefab;
    [SerializeField] private Transform panelContainer;
    [SerializeField] private GameObject title;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    [SerializeField] private TextMeshProUGUI mineralText;

    public void Init(InventoryPresenter presenter)
    {
        nextButton.onClick.AddListener(presenter.OnNextPage);
        prevButton.onClick.AddListener(presenter.OnPrevPage);
        mineralPanelPrefab.SetActive(false);
        mineralText.text = "Minerals collected: 0";
        Close();
    }

    public void CreateMineralEntry(string id, string x, string z)
    {
        GameObject entry = Instantiate(mineralPanelPrefab, panelContainer);
        entry.SetActive(true);

        var text = entry.GetComponentInChildren<TextMeshProUGUI>();
        text.text = $"Mineral at ({float.Parse(x):F2}, {float.Parse(z):F2})";
    }

    public void ClearMineralEntries()
    {
        foreach (Transform child in panelContainer)
        {
            if (child.gameObject != mineralPanelPrefab && child.gameObject != title)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void SetPaginationControls(bool hasPrev, bool hasNext)
    {
        prevButton.gameObject.SetActive(hasPrev);
        nextButton.gameObject.SetActive(hasNext);
    }

    public void Open()
    {
        panelContainer.gameObject.SetActive(true);
        title.SetActive(true);
    }

    public void Close()
    {
        ClearMineralEntries();
        panelContainer.gameObject.SetActive(false);
        title.SetActive(false);
        nextButton.gameObject.SetActive(false);
        prevButton.gameObject.SetActive(false);
    }

    public bool IsOpen()
    {
        return panelContainer.gameObject.activeSelf;
    }

    // Count of minerals in top left corner
    public void UpdateMineralCount(int count){
        mineralText.text = $"Minerals collected: {count}";
        Debug.Log($"Updating mineral text to: {mineralText.text}");
    }
}

