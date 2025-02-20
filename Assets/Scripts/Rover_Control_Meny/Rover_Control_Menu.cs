using UnityEngine;
using UnityEngine.UI;

public class Rover_Control_Menu : MonoBehaviour
{
  [SerializeField] private GameObject menuPanel; // TODO Assign menu panel in the Inspector
  private Button toggleButton;

  void Start()
  {
    // Get the Button component
    toggleButton = GetComponent<Button>();
    
    // Add click listener
    toggleButton.onClick.AddListener(ToggleMenu);
    
    // Make sure menu is initially hidden
    if (menuPanel != null)
      menuPanel.SetActive(false);
  }

  void ToggleMenu()
  {
    // Toggle menu visibility
    if (menuPanel != null)
      menuPanel.SetActive(!menuPanel.activeSelf);
  }
}
