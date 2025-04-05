using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class TableManager<T> : MonoBehaviour
{
    public GameObject togglePrefab;
    protected List<Toggle> listToggles = new List<Toggle>();
    protected Toggle selectedToggle = null;
    protected MainMenu mainMenu;

    protected abstract List<T> GetDataList();
    protected abstract string[] GetDisplayTexts(T item); // e.g. ID, Name, Description
    protected abstract void OnToggleSelected(Toggle toggle);

    protected virtual void Start()
    {
        mainMenu = GetComponentInParent<MainMenu>();
        PopulateTable();
    }

    protected void PopulateTable()
    {
        SortDataList();
        foreach (var item in GetDataList())
        {
            GameObject newToggle = Instantiate(togglePrefab, transform);
            TextMeshProUGUI[] texts = newToggle.GetComponentsInChildren<TextMeshProUGUI>();

            string[] displayTexts = GetDisplayTexts(item);
            for (int i = 0; i < texts.Length && i < displayTexts.Length; i++)
            {
                texts[i].text = displayTexts[i];
            }

            AttachDataToToggle(newToggle, item);

            Toggle toggle = newToggle.GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(delegate {
                ToggleChanged(toggle);
            });

            listToggles.Add(toggle);
        }
    }

    protected void ToggleChanged(Toggle changedToggle)
    {
        if (changedToggle.isOn)
        {
            foreach (var toggle in listToggles)
            {
                if (toggle != changedToggle)
                    toggle.isOn = false;
            }

            selectedToggle = changedToggle;
            OnToggleSelected(changedToggle);
        }
        else if (selectedToggle == changedToggle)
        {
            selectedToggle = null;
            OnToggleSelected(null); 
        }
        mainMenu.UpdateButtons();
    }

    protected virtual void AttachDataToToggle(GameObject toggleGO, T data)
    {
        if (toggleGO == null)
        {
            Debug.Log("Null toggle passed to AttachDataToToggle!");
            return;
        }

        if (data == null)
        {
            Debug.Log("Null data passed to AttachDataToToggle");
            return;
        }

        var toggleData = toggleGO.AddComponent<ToggleData<T>>();
        toggleData.data = data;
    }

    public Toggle GetSelectedToggle() => selectedToggle;
    protected virtual void SortDataList() { }

    public void ReorderTable()
    {
        // Destroy all toggle GameObjects
        foreach (var toggle in listToggles)
        {
            Destroy(toggle.gameObject);
        }

        listToggles.Clear();
        selectedToggle = null;

        PopulateTable(); // This will sort and repopulate in correct order
    }
}

public class RoverToggleData : ToggleData<Rover> { }
public class BrainToggleData : ToggleData<Brain> { }
public class AvatarToggleData : ToggleData<Avatar> { }