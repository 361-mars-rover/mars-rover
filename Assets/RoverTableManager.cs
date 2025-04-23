using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class RoverTableManager : TableManager<Rover>
{
    private List<Rover> rovers = new List<Rover>();

    protected override void Start()
    {
        AddTestRovers();
        base.Start();
    }

    void AddTestRovers()
    {
        Color [] colors = {Color.red, Color.blue, Color.yellow, Color.black, Color.gray, Color.cyan, Color.magenta, Color.white};
        string [] colorNames = {"red", "blue", "yellow", "black", "gray", "cyan", "magenta", "white"};
        string [] roverNames = {"Ralph", "Brenda", "Yan", "Borris", "Gertrude", "Craig", "Margaret", "Whitney"};
        for (int i = 0; i < colors.Length; i++)
        {
            Color color = colors[i];
            rovers.Add(new Rover { ID = i, name = roverNames[i], description = $"A {colorNames[i]} rover.", color=color});
        }
    }

    protected override List<Rover> GetDataList() => rovers;

    protected override string[] GetDisplayTexts(Rover rover)
    {
        return new string[] { rover.ID.ToString(), rover.name, rover.description };
    }

    protected override void OnToggleSelected(Toggle toggle)
    {
        mainMenu.setSelectedRover(toggle);
    }
    protected override void AttachDataToToggle(GameObject toggleGO, Rover data)
    {
        var toggleData = toggleGO.AddComponent<RoverToggleData>();
        toggleData.data = data;
    }

    protected override void SortDataList()
    {
        rovers.Sort((a, b) => a.ID.CompareTo(b.ID));
    }

    public List<Rover> GetRovers() => rovers;

    public void RemoveSelectedRover()
    {
        if (selectedToggle == null) return;

        var data = selectedToggle.GetComponent<ToggleData<Rover>>()?.data;
        if (data != null)
        {
            rovers.Remove(data);
            Debug.Log($"Consumed Rover: {data.name}");
        }

        listToggles.Remove(selectedToggle);
        Destroy(selectedToggle.gameObject);
        selectedToggle = null;

        mainMenu.setSelectedRover(null);
        mainMenu.UpdateButtons();
    }

    public void AddRover(Rover rover)
    {
        if (rover == null) return;

        rovers.Add(rover);

        GameObject newToggle = Instantiate(togglePrefab, transform);
        TextMeshProUGUI[] texts = newToggle.GetComponentsInChildren<TextMeshProUGUI>();
        string[] displayTexts = GetDisplayTexts(rover);
        for (int i = 0; i < texts.Length && i < displayTexts.Length; i++)
        {
            texts[i].text = displayTexts[i];
        }

        AttachDataToToggle(newToggle, rover);

        Toggle toggle = newToggle.GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(delegate {
            ToggleChanged(toggle);
        });

        listToggles.Add(toggle);
    }
}