using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    private Toggle selectedRover = null;
    private Toggle selectedBrain = null;
    private Toggle selectedAvatar = null;

    public Button createAvatarButton;
    public Button deleteAvatarButton;
    public Button editAvatarButton;

    public Button setSpawnPointsButton;
    public TMPro.TextMeshProUGUI createAvatarButtonText;
    public TMPro.TextMeshProUGUI deleteAvatarButtonText;
    public TMPro.TextMeshProUGUI editAvatarButtonText;

    private AvatarTableManager avatarTableManager;
    private BrainTableManager brainTableManager;
    private RoverTableManager roverTableManager;


    private int MAX_ROVERS = 5;
    private string url = "https://console.firebase.google.com/u/0/project/mars-rover-b4a62/database/mars-rover-b4a62-default-rtdb/data";

    
    // Start is called before the first frame update
    void Start()
    {
        createAvatarButton = transform.Find("CreateAvatarButton")?.GetComponent<Button>();
        deleteAvatarButton = transform.Find("DeleteAvatarButton")?.GetComponent<Button>();
        editAvatarButton = transform.Find("EditAvatarButton")?.GetComponent<Button>();
        setSpawnPointsButton = transform.Find("SetSpawnPointsButton")?.GetComponent<Button>();
        
        createAvatarButtonText = createAvatarButton.GetComponentInChildren<TextMeshProUGUI>();
        deleteAvatarButtonText = deleteAvatarButton.GetComponentInChildren<TextMeshProUGUI>();  
        editAvatarButtonText = editAvatarButton.GetComponentInChildren<TextMeshProUGUI>();    

        avatarTableManager = GetComponentInChildren<AvatarTableManager>(true);
        brainTableManager = GetComponentInChildren<BrainTableManager>(true);
        roverTableManager = GetComponentInChildren<RoverTableManager>(true);
        // Disable all buttons and set their text color to grey
        InitializeButtonState(createAvatarButton);
        InitializeButtonState(deleteAvatarButton);
        InitializeButtonState(editAvatarButton);  
        InitializeButtonState(setSpawnPointsButton);

    }
    public void StartSimulation()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    void Update() {

    }
    public void UpdateButtons()
    {
        // --- Create Avatar Button ---
        bool canCreate = (selectedRover != null && selectedBrain != null && 
            AvatarTableManager.avatars.Count < MAX_ROVERS);
        createAvatarButton.interactable = canCreate;
        SetButtonTextColor(createAvatarButton, canCreate);

        // --- Delete Avatar Button ---
        bool canDelete = (selectedAvatar != null);
        deleteAvatarButton.interactable = canDelete;
        SetButtonTextColor(deleteAvatarButton, canDelete);

        // --- Edit Avatar Button ---
        bool canEdit = (selectedAvatar != null && selectedBrain != null);
        editAvatarButton.interactable = canEdit;
        SetButtonTextColor(editAvatarButton, canEdit);

        // --- Set Spawn Point Button ---
        bool canSpawn = AvatarTableManager.avatars.Count > 0;
        setSpawnPointsButton.interactable = canSpawn;
        SetButtonTextColor(setSpawnPointsButton, canSpawn);
    }

    private void InitializeButtonState(Button button)
    {
        if (button != null)
        {
            button.interactable = false;
            TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.color = Color.grey;
        }
    }

    private void SetButtonTextColor(Button button, bool isEnabled)
    {
        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.color = isEnabled ? Color.white : Color.grey;
        }
    }

    public void setSelectedRover(Toggle newSelectedRover) {
        selectedRover = newSelectedRover;
    }

    public void setSelectedBrain(Toggle newSelectedBrain) {
        selectedBrain = newSelectedBrain;
    }
    public void setSelectedAvatar(Toggle newSelectedAvatar) {
        
        selectedAvatar = newSelectedAvatar;
    }
    public void CreateAvatarFromSelection()
    {
        if (selectedRover == null || selectedBrain == null)
        {
            Debug.LogWarning("Cannot create Avatar - Rover or Brain not selected.");
            return;
        }

        // Extract data
        Rover rover = selectedRover.GetComponent<ToggleData<Rover>>()?.data;
        Brain brain = selectedBrain.GetComponent<ToggleData<Brain>>()?.data;

        if (rover == null || brain == null)
        {
            Debug.LogWarning("Missing Rover or Brain data.");
            return;
        }

        // Create Avatar
        Avatar newAvatar = new Avatar
        {
            rover = rover,
            brain = brain,
            description = $"Avatar {AvatarTableManager.avatars.Count}",
            SpawnRowCol = new Vector2Int(0, 0) // or assign later
        };

        AvatarTableManager.avatars.Add(newAvatar);
        avatarTableManager.RefreshTable();

        // Consume Rover & Brain
        // TODO: 
        // roverTableManager.RemoveSelectedRover();
        // brainTableManager.RemoveSelectedBrain();

        Debug.Log($"Created new Avatar: {newAvatar.description}");
        UpdateButtons();
    }

    public void EditSelectedAvatar()
    {
        if (selectedAvatar == null || selectedBrain == null)
        {
            Debug.LogWarning("EditAvatar called, but Avatar or Brain is not selected.");
            return;
        }

        // Get Avatar data
        AvatarToggleData avatarData = selectedAvatar.GetComponent<AvatarToggleData>();
        BrainToggleData newBrainData = selectedBrain.GetComponent<BrainToggleData>();

        if (avatarData == null || newBrainData == null)
        {
            Debug.LogError("ToggleData missing for selected Avatar or Brain.");
            return;
        }

        Avatar avatar = avatarData.data;
        Brain oldBrain = avatar.brain;
        Brain newBrain = newBrainData.data;

        if (oldBrain == newBrain)
        {
            Debug.Log("Selected brain is already assigned to avatar — nothing to change.");
            return;
        }

        // Swap brain
        avatar.brain = newBrain;

        // Update the toggle's displayed text
        TextMeshProUGUI[] texts = selectedAvatar.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length >= 3)
        {
            texts[2].text = newBrain.name; // Assuming ID, Rover Name, Brain Name
        }

        // Consume new brain and release old one
        brainTableManager.RemoveSelectedBrain();    // Removes new brain
        brainTableManager.AddBrain(oldBrain);       // Re-adds old brain

        // Clear selection and update buttons
        setSelectedBrain(null);
        UpdateButtons();
        ReorderTables();

        Debug.Log($"Avatar {avatar.ID} brain swapped: {oldBrain.name} → {newBrain.name}");
    }

    public void DeleteSelectedAvatar()
    {
        avatarTableManager.RemoveSelectedAvatar();
        ReorderTables();
    }

    public void ClearAvatarsAndRestoreComponents()
    {
        // List<Avatar> avatarsToRestore = new List<Avatar>(AvatarTableManager.avatars);

        // Clear avatar table
        AvatarTableManager.avatars.Clear();
        //avatarTableManager.RefreshTable();

        // Re-add rovers and brains
        // foreach (Avatar avatar in avatarsToRestore)
        // {
        //     roverTableManager.AddRover(avatar.rover);
        //     brainTableManager.AddBrain(avatar.brain);
        // }

        // Clear any selected state
        setSelectedAvatar(null);
        setSelectedRover(null);
        setSelectedBrain(null);

        UpdateButtons();
        ReorderTables();

        // Debug.Log($"Restarted simulation: {avatarsToRestore.Count} avatars cleared, rovers and brains restored.");
    }

    public void ReAddBrainAndRover(Brain brain, Rover rover)
    {
        brainTableManager.AddBrain(brain);
        roverTableManager.AddRover(rover);
    }

    public void ReorderTables() 
    {
        roverTableManager.ReorderTable();
        brainTableManager.ReorderTable();
        avatarTableManager.ReorderTable();
    }

    public void OpenURL()
    {
        Application.OpenURL(url);
    }

}
