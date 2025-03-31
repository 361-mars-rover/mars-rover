using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public GameObject mineralPanelPrefab; // UI prefab for displaying minerals
    public Transform panelContainer; // Parent panel to hold mineral entries
    public GameObject title; // Title or header
    public Button nextButton; // Button to go to next page
    public Button prevButton; // Button to go to previous page

    private List<(string id, string x, string z)> mineralIds = new List<(string, string, string)>(); // Store mineral info
    private int currentPage = 0;
    private int mineralsPerPage = 5;

    private DatabaseReference materialsRef; // Firebase reference for minerals
    private string currentCarId = ""; // Store the current car ID to detect changes

    void Start()
    {
        mineralPanelPrefab.SetActive(false);
        panelContainer.gameObject.SetActive(false);
        title.SetActive(false);
        nextButton.gameObject.SetActive(false);
        prevButton.gameObject.SetActive(false);

        // Initialize Firebase reference only when car ID is available
        UpdateCarReference();

        // Add listeners for pagination buttons
        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PreviousPage);
    }

    void Update()
    {
        // Check if car ID has changed and update reference
        if (CarControl.id != currentCarId)
        {
            UpdateCarReference();
        }
    }

    private void UpdateCarReference()
    {
        if (!string.IsNullOrEmpty(CarControl.id))
        {
            currentCarId = CarControl.id;
            materialsRef = FirebaseManager.dbReference.Child("materials").Child(currentCarId);

            Debug.Log($"Updated Firebase reference to car ID: {currentCarId}");

            // If inventory is open, refresh minerals
            if (panelContainer.gameObject.activeSelf)
            {
                FetchMinerals();
            }
        }
        else
        {
            Debug.LogWarning("Car ID is not set yet.");
        }
    }

    // Fetch minerals from Firebase and listen for changes
    public void FetchMinerals()
    {
        if (string.IsNullOrEmpty(currentCarId))
        {
            Debug.LogError("Cannot fetch minerals: Car ID is not set.");
            return;
        }

        Debug.Log("Opening inventory...");
        panelContainer.gameObject.SetActive(true);
        title.SetActive(true);

        // Remove previous listener if any
        if (materialsRef != null)
        {
            materialsRef.ValueChanged -= OnMineralsChanged;
        }

        // Listen for changes in real-time
        Debug.Log("Fetching minerals from database...");
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

        Debug.Log("Database updated. Refreshing minerals...");
        mineralIds.Clear();  // Clear current data to refresh UI
        ClearPanel(); // Remove old entries

        DataSnapshot snapshot = args.Snapshot;
        if (snapshot.Exists)
        {
            foreach (var mineralEntry in snapshot.Children)
            {
                string mineralId = mineralEntry.Child("id").Value.ToString();
                string positionX = mineralEntry.Child("position").Child("x").Value.ToString();
                string positionZ = mineralEntry.Child("position").Child("z").Value.ToString();
                mineralIds.Add((mineralId, positionX, positionZ));
            }

            Debug.Log("Minerals updated successfully!");
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
        int endIndex = Mathf.Min(startIndex + mineralsPerPage, mineralIds.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            CreateMineralEntry(mineralIds[i].id, mineralIds[i].x, mineralIds[i].z);
        }

        // Enable/disable pagination buttons based on current page
        prevButton.gameObject.SetActive(currentPage > 0);
        nextButton.gameObject.SetActive(endIndex < mineralIds.Count);
    }

    // Go to the next page of minerals
    public void NextPage()
    {
        if ((currentPage + 1) * mineralsPerPage < mineralIds.Count)
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
