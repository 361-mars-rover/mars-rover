using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
public class BrainTableManager : TableManager<Brain>
{
    public List<Brain> brains = new List<Brain>();

    protected override void Start()
    {
        AddTestBrains();
        base.Start();
    }

    void AddTestBrains()
    {
        for (int i = 0; i < Enum.GetNames(typeof(AIMode)).Length; i++)
        {
            AIMode aiMode = (AIMode)i;
            brains.Add(new Brain { ID = i, name = $"{aiMode.ToString()}", description = aiMode.GetDescription(), aIMode=aiMode});
        }
    }

    protected override List<Brain> GetDataList() => brains;

    protected override string[] GetDisplayTexts(Brain brain)
    {
        return new string[] { brain.ID.ToString(), brain.name, brain.description };
    }

    protected override void OnToggleSelected(Toggle toggle)
    {
        mainMenu.setSelectedBrain(toggle);
    }
    protected override void AttachDataToToggle(GameObject toggleGO, Brain data)
    {
        var toggleData = toggleGO.AddComponent<BrainToggleData>();
        toggleData.data = data;
    }
    protected override void SortDataList()
    {
        brains.Sort((a, b) => a.ID.CompareTo(b.ID));
    }

    public List<Brain> GetBrains() => brains;

    public void RemoveSelectedBrain()
    {
        if (selectedToggle == null) return;

        var data = selectedToggle.GetComponent<ToggleData<Brain>>()?.data;
        if (data != null)
        {
            brains.Remove(data);
            Debug.Log($"Consumed Brain: {data.name}");
        }

        listToggles.Remove(selectedToggle);
        Destroy(selectedToggle.gameObject);
        selectedToggle = null;

        mainMenu.setSelectedBrain(null);
        mainMenu.UpdateButtons();
    }

    public void AddBrain(Brain brain)
    {
        if (brain == null) return;

        brains.Add(brain);

        GameObject newToggle = Instantiate(togglePrefab, transform);
        TextMeshProUGUI[] texts = newToggle.GetComponentsInChildren<TextMeshProUGUI>();
        string[] displayTexts = GetDisplayTexts(brain);
        for (int i = 0; i < texts.Length && i < displayTexts.Length; i++)
        {
            texts[i].text = displayTexts[i];
        }

        AttachDataToToggle(newToggle, brain);

        Toggle toggle = newToggle.GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(delegate {
            ToggleChanged(toggle);
        });

        listToggles.Add(toggle);
    }
}