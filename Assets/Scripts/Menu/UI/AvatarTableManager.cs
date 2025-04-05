using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class AvatarTableManager : TableManager<Avatar>
{
    public static List<Avatar> avatars = new List<Avatar>();

    protected override void Start()
    {
        //AddFakeData();
        base.Start();
    }

    void AddFakeData()
    {
        for (int i = 0; i < 3; i++)
        {
            avatars.Add(new Avatar
            {
                description = $"Avatar {i}",
                SpawnRowCol = new Vector2Int(i, i),
                rover = new Rover { name = $"Rover {i}" },
                brain = new Brain { name = $"Brain {i}" }
            });
        }
    }

    protected override List<Avatar> GetDataList() => avatars;

    protected override string[] GetDisplayTexts(Avatar avatar)
    {
        return new string[] { avatar.ID.ToString(), avatar.rover.name, avatar.brain.name };
    }

    protected override void OnToggleSelected(Toggle toggle)
    {
        mainMenu.setSelectedAvatar(toggle);
    }

    protected override void AttachDataToToggle(GameObject toggleGO, Avatar data)
    {
        var toggleData = toggleGO.AddComponent<AvatarToggleData>();
        toggleData.data = data;
    }
    protected override void SortDataList()
    {
        avatars.Sort((a, b) => a.ID.CompareTo(b.ID));
    }

    public List<Avatar> getAvatars() => avatars;

    void SetNewestAvatarName(string name){
        throw new NotImplementedException();
    }

    void SetNewestAvatarRowCol(int row, int col){
        throw new NotImplementedException();
    }

    void SetNewestAvatarBrain(string brain){
        throw new NotImplementedException();
    }

    public void RefreshTable()
    {
        Avatar latest = avatars[^1]; // Get the last avatar (C# 8 syntax)

        GameObject newToggle = Instantiate(togglePrefab, transform);
        TextMeshProUGUI[] texts = newToggle.GetComponentsInChildren<TextMeshProUGUI>();

        string[] displayTexts = GetDisplayTexts(latest);
        for (int i = 0; i < texts.Length && i < displayTexts.Length; i++)
        {
            texts[i].text = displayTexts[i];
        }

        AttachDataToToggle(newToggle, latest);

        Toggle toggle = newToggle.GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(delegate {
            ToggleChanged(toggle);
        });

        listToggles.Add(toggle);
    }

    public void RemoveSelectedAvatar()
    {
        if (selectedToggle == null) return;

        var dataComponent = selectedToggle.GetComponent<ToggleData<Avatar>>();
        if (dataComponent != null)
        {
            Avatar avatar = dataComponent.data;
            avatars.Remove(avatar);
            Debug.Log($"Removed avatar: ID {avatar.ID}");

            // Pass brain/rover back to MainMenu
            mainMenu.ReAddBrainAndRover(avatar.brain, avatar.rover);
        }

        listToggles.Remove(selectedToggle);
        Destroy(selectedToggle.gameObject);
        selectedToggle = null;

        mainMenu.setSelectedAvatar(null);
        mainMenu.UpdateButtons();
    }
}