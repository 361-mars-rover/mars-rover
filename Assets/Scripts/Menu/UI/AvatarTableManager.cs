using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class AvatarTableManager : MonoBehaviour
{
    public GameObject togglePrefab;
    public static List<Avatar> avatars = new List<Avatar>();
    private Avatar currentAvatar;

    void Start()
    {
        // PopulateTable();
        Debug.Log("Adding a test avatars");
        AddFakeData();
    }

    // Written by Jikael... Just for testing that data can be sent to SimulationManager...
    void AddFakeData(){
        for (int i = 0; i < 3; i++){
            Avatar a = new Avatar();
            a.Description = $"Avatar {i}";
            a.SpawnRowCol = new Vector2Int(i,i);
            avatars.Add(a);
        }
    }

    void CreateAvatar(){
        Avatar a = new Avatar();
        currentAvatar = a;
    }

    void SetNewestAvatarName(string name){
        throw new NotImplementedException();
    }

    void SetNewestAvatarRowCol(int row, int col){
        throw new NotImplementedException();
    }

    void SetNewestAvatarBrain(string brain){
        throw new NotImplementedException();
    }

    void PopulateTable()
    {
        Debug.Log("Tables Populated");
        /*foreach (var avatar in avatars)
        {
            GameObject newToggle = Instantiate(togglePrefab, transform);
            Text[] texts = newToggle.GetComponentsInChildren<Text>();
            // Assuming the order of texts in your prefab is ID, Name, Description
            texts[0].text = avatar.ID.ToString();
            texts[1].text = avatar.Name;
            texts[2].text = avatar.Description;

            Toggle toggle = newToggle.GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(delegate {
                ToggleChanged(toggle);
            });
        }*/
    }

    void ToggleChanged(Toggle changedToggle)
    {
        if (changedToggle.isOn)
        {
            Debug.Log("Toggle On: " + changedToggle.GetComponentInChildren<Text>().text);
        }
    }

    public List<Avatar> getAvatars()
    {
        return avatars;
    }
}
