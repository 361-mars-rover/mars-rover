using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
// Chloe Gavrilovic 260955835

public class InventoryManager : MonoBehaviour
{
    public GameObject mineralPanelPrefab; 
    public Transform panelContainer; 
    public GameObject title; 
    public Button nextButton; 
    public Button prevButton; 
    private List<(string id, string x, string z)> mineralIds = new List<(string, string, string)>(); 
    private int currentPage = 0;
    private int mineralsPerPage = 5;
    private DatabaseReference materialsRef; 
    public SimulationManager simulationManager; 
    public FirebaseManager firebaseManager; 
    private string currentCarId; 

    void Start()
    {
        // init inventory UI
        mineralPanelPrefab.SetActive(false);
        panelContainer.gameObject.SetActive(false);
        title.SetActive(false);
        nextButton.gameObject.SetActive(false);
        prevButton.gameObject.SetActive(false);
        simulationManager = FindObjectOfType<SimulationManager>();
        firebaseManager = FindObjectOfType<FirebaseManager>();
        Debug.Log($"firebase manager is null: {firebaseManager == null}");
        currentCarId = simulationManager.roverIds[simulationManager.curIdx];
        materialsRef = FirebaseManager.dbReference.Child("Simulations").Child(firebaseManager.simulationId).Child("Avatars").Child(currentCarId);
        Debug.Log($"firebase materials ref is null: {materialsRef == null}"); 
        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PreviousPage);
    }

    // fetch minerals from firebase and listen for changes
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

        if (materialsRef != null)
        {
            materialsRef.ValueChanged -= OnMineralsChanged;
        }
        materialsRef.ValueChanged += OnMineralsChanged;
    }

    // track changes in the minerals database
    private void OnMineralsChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Database error: " + args.DatabaseError.Message);
            return;
        }
        mineralIds.Clear(); 
        ClearPanel();

        DataSnapshot snapshot = args.Snapshot;
        if (snapshot.Exists)
        {
            foreach (var mineralEntry in snapshot.Children)
            {
                string mineralId = mineralEntry.Child("id").Value.ToString();
                string positionX = mineralEntry.Child("position").Child("latitude").Value.ToString();
                string positionZ = mineralEntry.Child("position").Child("longitude").Value.ToString();
                mineralIds.Add((mineralId, positionX, positionZ));
            }
            UpdatePagination(); 
        }
        else
        {
            Debug.Log("No minerals found!");
        }
    }

    // update the UI with minerals for the current page
    private void UpdatePagination()
    {
        ClearPanel(); 
        int startIndex = currentPage * mineralsPerPage;
        int endIndex = Mathf.Min(startIndex + mineralsPerPage, mineralIds.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            CreateMineralEntry(mineralIds[i].id, mineralIds[i].x, mineralIds[i].z);
        }
        prevButton.gameObject.SetActive(currentPage > 0);
        nextButton.gameObject.SetActive(endIndex < mineralIds.Count);
    }

    // go to next page of minerals
    public void NextPage()
    {
        if ((currentPage + 1) * mineralsPerPage < mineralIds.Count)
        {
            currentPage++;
            UpdatePagination();
        }
    }

    // go to previous page of minerals
    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePagination();
        }
    }

    // create entry for a mineral
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

    // close the inventory and remove listeners
    public void CloseInventory()
    {
        ClearPanel();
        panelContainer.gameObject.SetActive(false);
        title.SetActive(false);
        nextButton.gameObject.SetActive(false);
        prevButton.gameObject.SetActive(false);

        if (materialsRef != null)
        {
            materialsRef.ValueChanged -= OnMineralsChanged; 
        }
    }

    // remove all UI elements from the panel
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
