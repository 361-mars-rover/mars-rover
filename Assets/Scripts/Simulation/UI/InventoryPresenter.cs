using UnityEngine;
/*
Authors: Jikael and Chloe
This is the presenter part of the model-view-presenter design pattern. In this design pattern, the model and view are both passive
and the presenter modifies them both. This class also sends data to the database on mineral collection.
*/
public class InventoryPresenter : MonoBehaviour
{
    [SerializeField] private SimulationManager simulationManager;
    [SerializeField] private FirebaseManager firebaseManager;
    [SerializeField] private InventoryView view;

    [SerializeField] private SimulationManagerModel simulationManagerModel;

    private int currentPage = 0;
    private const int mineralsPerPage = 5;

    [SerializeField] private InventoryModel model;

    private void Awake()
    {
        simulationManager = FindAnyObjectByType<SimulationManager>();
        simulationManagerModel = FindFirstObjectByType<SimulationManagerModel>();
        firebaseManager = FindAnyObjectByType<FirebaseManager>();
        view = FindFirstObjectByType<InventoryView>();
        view.Init(this);
        // model = GetComponent<InventoryModel>();
        model = FindFirstObjectByType<InventoryModel>();

    }

    // Toggles inventory on button click
    public void ToggleInventory()
    {
        if (view.IsOpen())
        {
            view.Close();
        }
        else
        {
            view.Open();
            UpdatePagination();
        }
    }

    // Loads data for a page in the inventory table
    private void UpdatePagination()
    {
        view.ClearMineralEntries(); // Deletes all existing visible entries
        // Compute the start and end indices to load on this page
        int start = currentPage * mineralsPerPage;
        int end = Mathf.Min(start + mineralsPerPage, model.mineralData.Count);

        // Create an entry for each mineral that should be displayed on this page
        for (int i = start; i < end; i++)
        {
            var (id, x, z) = model.mineralData[i];
            Debug.Log("Creating a mineral entry");
            view.CreateMineralEntry(id, x.ToString(), z.ToString());
        }
        // Tells view whether this page should be allowed to go back/next
        view.SetPaginationControls(
            hasPrev: currentPage > 0,
            hasNext: end < model.mineralData.Count
        );
    }
    // Called when switching to next page. Updates mineral data
    public void OnNextPage()
    {
        if ((currentPage + 1) * mineralsPerPage < model.mineralData.Count)
        {
            currentPage++;
            UpdatePagination();
        }
    }
    // Called when switching to prev page. Updates mineral data
    public void OnPrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePagination();
        }
    }

    public void CollectMineral(GameObject mineral)
    {
        /*
        This is called by a Minerals.cs object when the rover collides with it.
        */
        string currentCarId = simulationManagerModel.roverIds[simulationManager.curIdx]; // UID for rover
        firebaseManager.StoreMaterialData(mineral, currentCarId, firebaseManager.simulationId); // Store data under rover's UID in firebase
        model.CollectMineral(mineral); // Updates the model to reflect that a new mineral has been collected
        view.UpdateMineralCount(model.mineralData.Count); // Updates the mineral count to reflect that a new mineral was collected
        if (view.IsOpen())
        {
            UpdatePagination(); // only update the UI if the inventory is visible
        }
    }
}
