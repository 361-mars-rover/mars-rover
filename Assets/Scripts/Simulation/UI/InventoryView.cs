// // using UnityEngine;
// // using TMPro;

// // public class InventoryView : MonoBehaviour
// // {
// //     [SerializeField] private TextMeshProUGUI mineralText;

// //     public void UpdateMineralCount(int count)
// //     {
// //         if (mineralText != null)
// //         {
// //             Debug.Log("Mineral text found");

// //             mineralText.text = $"Minerals collected: {count}";
// //         }
// //     }
// // }

// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public class InventoryView : MonoBehaviour
// {
//     public TextMeshProUGUI mineralText;
//     public GameObject mineralPanelPrefab;
//     public Transform panelContainer;
//     public GameObject title;
//     public Button nextButton;
//     public Button prevButton;

//     public void UpdateMineralCount(int count)
//     {
//         if (mineralText != null)
//             mineralText.text = $"Minerals collected: {count}";
//     }

//     public void ShowPanels(List<(string id, string x, string z)> minerals, int page, int perPage)
//     {
//         ClearPanel();

//         int start = page * perPage;
//         int end = Mathf.Min(start + perPage, minerals.Count);
//         for (int i = start; i < end; i++)
//         {
//             var (id, x, z) = minerals[i];
//             GameObject panel = Instantiate(mineralPanelPrefab, panelContainer);
//             panel.SetActive(true);
//             var text = panel.GetComponentInChildren<TextMeshProUGUI>();
//             if (text != null)
//                 text.text = $"Mineral at ({x}, {z})";
//         }

//         prevButton.gameObject.SetActive(page > 0);
//         nextButton.gameObject.SetActive(end < minerals.Count);
//     }

//     public void ClearPanel()
//     {
//         foreach (Transform child in panelContainer)
//         {
//             if (child.gameObject != mineralPanelPrefab && child.gameObject != title)
//                 Destroy(child.gameObject);
//         }
//     }

//     public void ToggleInventoryPanel(bool visible)
//     {
//         panelContainer.gameObject.SetActive(visible);
//         title.SetActive(visible);
//         nextButton.gameObject.SetActive(visible);
//         prevButton.gameObject.SetActive(visible);
//     }
// }
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour
{
    [SerializeField] private GameObject mineralPanelPrefab;
    [SerializeField] private Transform panelContainer;
    [SerializeField] private GameObject title;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    private InventoryPresenter presenter;

    public void Init(InventoryPresenter presenter)
    {
        this.presenter = presenter;
        nextButton.onClick.AddListener(presenter.OnNextPage);
        prevButton.onClick.AddListener(presenter.OnPrevPage);
        mineralPanelPrefab.SetActive(false);
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
}

