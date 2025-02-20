using UnityEngine;
using UnityEngine.UI;


public class MenuOption : MonoBehaviour
{

    [SerializeField] private GameObject menuPanel; // Assign your menu panel in the Inspector
    [SerializeField] private string optionType; // "Self-Control" or "AI_Simple"
    private Button optionButton;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the Button component
        optionButton = GetComponent<Button>();
        
        // Add click listener
        optionButton.onClick.AddListener(SelectOption);
    }

    void SelectOption()
    {
        // Handle the option selection
        Debug.Log(optionType + " option selected");
        
        // Your option-specific code here
        // ...

        // Close the menu panel
        if (menuPanel != null)
            menuPanel.SetActive(false);
    }
}
