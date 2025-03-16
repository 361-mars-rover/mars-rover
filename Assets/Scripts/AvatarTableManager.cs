using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AvatarTableManager : MonoBehaviour
{
    public GameObject togglePrefab;
    public List<Avatar> avatars;

    void Start()
    {
        PopulateTable();
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
}
