using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryPresenter : MonoBehaviour
{
    public GameObject mineralPanelPrefab; // UI prefab for displaying minerals
    public Transform panelContainer; // Parent panel to hold mineral entries
    public GameObject title; // Title or header
    public Button nextButton; // Button to go to next page
    public Button prevButton; // Button to go to previous page

    private Inventory inventory;
    private int currentPage = 0;
    private int mineralsPerPage = 5;

    private DatabaseReference materialsRef; // Firebase reference for minerals
    public SimulationManager simulationManager; // Reference to the simulation manager
    public FirebaseManager firebaseManager; // Reference to the Firebase manager
    private string currentCarId; // Store the current car ID to detect changes

    void Start()
    {
        mineralPanelPrefab.SetActive(false);
        panelContainer.gameObject.SetActive(false);
        title.SetActive(false);
        nextButton.gameObject.SetActive(false);
        prevButton.gameObject.SetActive(false);
        simulationManager = FindObjectOfType<SimulationManager>();
        firebaseManager = FindObjectOfType<FirebaseManager>();
        currentCarId = simulationManager.roverIds[simulationManager.curIdx]; // Initialize with the current car ID
        materialsRef = FirebaseManager.dbReference.Child("Simulations").Child(firebaseManager.simulationId).Child("Avatars").Child(currentCarId);

        // Add listeners for pagination buttons
        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PreviousPage);
    }

    // Fetch minerals from Firebase and listen for changes
    public void FetchMinerals()
    {
        Debug.Log("Current rover:" + currentCarId);
        if (string.IsNullOrEmpty(currentCarId))
        {
            Debug.LogError("Cannot fetch minerals: Car ID is not set.");
            return;
        }

        panelContainer.gameObject.SetActive(true);
        title.SetActive(true);

        // Remove previous listener if any
        if (materialsRef != null)
        {
            materialsRef.ValueChanged -= OnMineralsChanged;
        }

        materialsRef.ValueChanged += OnMineralsChanged;
    }

    // Callback function for database changes (real-time)
    private void OnMineralsChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Database error: " + args.DatabaseError.Message);
            return;
        }


        inventory.Clear();  // Clear current data to refresh UI
        ClearPanel(); // Remove old entries

        DataSnapshot snapshot = args.Snapshot;
        if (snapshot.Exists)
        {
            foreach (var mineralEntry in snapshot.Children)
            {
                string mineralId = mineralEntry.Child("id").Value.ToString();
                string positionX = mineralEntry.Child("position").Child("x").Value.ToString();
                string positionZ = mineralEntry.Child("position").Child("z").Value.ToString();
                inventory.AddMineral(mineralId, positionX, positionZ);
            }

            UpdatePagination(); // Refresh the UI with the updated minerals without changing the current page
        }
        else
        {
            Debug.Log("No minerals found!");
        }
    }

    // Update the UI with the minerals for the current page
    private void UpdatePagination()
    {
        ClearPanel(); // Clear existing entries

        int startIndex = currentPage * mineralsPerPage;
        int endIndex = Mathf.Min(startIndex + mineralsPerPage, inventory.GetMineralCount());

        for (int i = startIndex; i < endIndex; i++)
        {
            (string id, string x, string z) = inventory.GetMineral(i);
            CreateMineralEntry(id, x, z);
        }

        // Enable/disable pagination buttons based on current page
        prevButton.gameObject.SetActive(currentPage > 0);
        nextButton.gameObject.SetActive(endIndex < inventory.GetMineralCount());
    }

    // Go to the next page of minerals
    public void NextPage()
    {
        if ((currentPage + 1) * mineralsPerPage < inventory.GetMineralCount())
        {
            currentPage++;
            UpdatePagination();
        }
    }

    // Go to the previous page of minerals
    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePagination();
        }
    }

    // Create a UI entry for a mineral
    private void CreateMineralEntry(string mineralId, string positionX, string positionZ)
    {
        if (mineralPanelPrefab == null)
        {
            Debug.LogError("Mineral panel prefab is missing!");
            return;
        }

        GameObject entry = Instantiate(mineralPanelPrefab, panelContainer);
        entry.SetActive(true);

        TextMeshProUGUI textComponent = entry.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.enabled = true;
            textComponent.gameObject.SetActive(true);
            float x = float.Parse(positionX);
            float z = float.Parse(positionZ);
            textComponent.text = $"Mineral at ({x:F2}, {z:F2})";
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found!");
        }
    }

    // Close the inventory and remove listeners
    public void CloseInventory()
    {
        ClearPanel();
        panelContainer.gameObject.SetActive(false);
        title.SetActive(false);
        nextButton.gameObject.SetActive(false);
        prevButton.gameObject.SetActive(false);

        if (materialsRef != null)
        {
            materialsRef.ValueChanged -= OnMineralsChanged; // Remove listener to avoid memory leaks
        }
    }

    // Remove all UI elements from the panel
    private void ClearPanel()
    {
        foreach (Transform child in panelContainer)
        {
            if (child.gameObject != title && child.gameObject != mineralPanelPrefab)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
