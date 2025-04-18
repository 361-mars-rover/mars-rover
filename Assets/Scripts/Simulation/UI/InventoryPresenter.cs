// using System.Collections.Generic;
// using Firebase.Database;
// using UnityEngine;

// // public class InventoryPresenter : MonoBehaviour
// // {
// //     [SerializeField] private SimulationManager simulationManager;
// //     [SerializeField] private FirebaseManager firebaseManager;

// //     private InventoryModel model;
// //     private InventoryView view;
// //     private void Awake()
// //     {
// //         simulationManager = FindFirstObjectByType<SimulationManager>();
// //         firebaseManager = FindFirstObjectByType<FirebaseManager>();
// //         model = new InventoryModel();
// //         view = FindFirstObjectByType<InventoryView>();
// //         model.OnMineralCollected += HandleMineralCollected;
// //     }

// //     private void OnDestroy()
// //     {
// //         model.OnMineralCollected -= HandleMineralCollected;
// //     }

// //     public void CollectMineral(GameObject mineral)
// //     {
// //         model.CollectMineral(mineral);
// //     }

// //     private void HandleMineralCollected(InventoryModel updatedModel)
// //     {
// //         Debug.Log($"Mineral collected for rover: {simulationManager.roverIds[simulationManager.curIdx]}");
// //         Debug.Log($"Total Minerals Collected: {updatedModel.NumberOfMinerals}");
// //         Debug.Log($"View is null: {view == null}");
// //         Debug.Log($"Model is null: {model == null}");
// //         Debug.Log($"Model num minerals is null: {model.NumberOfMinerals == null}");
// //         view.UpdateMineralCount(model.NumberOfMinerals);
// //         if (firebaseManager != null)
// //         {
// //             firebaseManager.StoreMaterialData(
// //                 updatedModel.Minerals[^1], // last collected mineral
// //                 simulationManager.roverIds[simulationManager.curIdx],
// //                 firebaseManager.simulationId
// //             );
// //         }
// //     }

// //     public int GetMineralCount() => model.NumberOfMinerals;
// // }

// public class InventoryPresenter : MonoBehaviour
// {
//     [SerializeField] private InventoryView view;
//     [SerializeField] private FirebaseManager firebaseManager;
//     [SerializeField] private SimulationManager simulationManager;

//     private InventoryModel model;
//     private string currentCarId;
//     private DatabaseReference materialsRef;
//     private List<(string id, string x, string z)> mineralEntries = new();
//     private int currentPage = 0;
//     private int mineralsPerPage = 5;

//     private void Awake()
//     {
//         model = new InventoryModel();
//         model.OnMineralCollected += HandleMineralCollected;
//         view = FindFirstObjectByType<InventoryView>();
//     }

//     private void Start()
//     {
//         simulationManager = FindObjectOfType<SimulationManager>();
//         firebaseManager = FindObjectOfType<FirebaseManager>();
//         currentCarId = simulationManager.roverIds[simulationManager.curIdx];
//         materialsRef = FirebaseManager.dbReference.Child("Simulations")
//             .Child(firebaseManager.simulationId)
//             .Child("Avatars")
//             .Child(currentCarId);

//         view.nextButton.onClick.AddListener(NextPage);
//         view.prevButton.onClick.AddListener(PreviousPage);
//     }

//     public void CollectMineral(GameObject mineral)
//     {
//         model.CollectMineral(mineral);
//         firebaseManager.StoreMaterialData(mineral, currentCarId, firebaseManager.simulationId);
//     }

//     private void HandleMineralCollected(InventoryModel model)
//     {
//         view.UpdateMineralCount(model.NumberOfMinerals);
//     }

//     public void OpenInventory()
//     {
//         view.ToggleInventoryPanel(true);
//         FetchMineralsFromDatabase();
//     }

//     public void CloseInventory()
//     {
//         view.ToggleInventoryPanel(false);
//         view.ClearPanel();
//         materialsRef.ValueChanged -= OnMineralsChanged;
//     }

//     private void FetchMineralsFromDatabase()
//     {
//         materialsRef.ValueChanged += OnMineralsChanged;
//     }

//     private void OnMineralsChanged(object sender, ValueChangedEventArgs args)
//     {
//         if (args.DatabaseError != null)
//         {
//             Debug.LogError("DB Error: " + args.DatabaseError.Message);
//             return;
//         }

//         mineralEntries.Clear();

//         foreach (var mineralEntry in args.Snapshot.Children)
//         {
//             string id = mineralEntry.Child("id").Value.ToString();
//             string x = mineralEntry.Child("position").Child("latitude").Value.ToString();
//             string z = mineralEntry.Child("position").Child("longitude").Value.ToString();
//             mineralEntries.Add((id, x, z));
//         }

//         UpdatePagination();
//     }

//     private void UpdatePagination()
//     {
//         view.ShowPanels(mineralEntries, currentPage, mineralsPerPage);
//     }

//     private void NextPage()
//     {
//         if ((currentPage + 1) * mineralsPerPage < mineralEntries.Count)
//         {
//             currentPage++;
//             UpdatePagination();
//         }
//     }

//     private void PreviousPage()
//     {
//         if (currentPage > 0)
//         {
//             currentPage--;
//             UpdatePagination();
//         }
//     }
// }

using System.Collections.Generic;
using Firebase.Extensions;
using UnityEngine;

public class InventoryPresenter : MonoBehaviour
{
    [SerializeField] private SimulationManager simulationManager;
    [SerializeField] private FirebaseManager firebaseManager;
    [SerializeField] private InventoryView view;

    private List<(string id, string x, string z)> mineralData = new();
    private int currentPage = 0;
    private const int mineralsPerPage = 5;

    private void Awake()
    {
        simulationManager = FindAnyObjectByType<SimulationManager>();
        firebaseManager = FindAnyObjectByType<FirebaseManager>();
        view = FindFirstObjectByType<InventoryView>();
        view.Init(this);
    }

    public void ToggleInventory()
    {
        if (view.IsOpen())
        {
            view.Close();
        }
        else
        {
            FetchMineralsFromDatabase();
            view.Open();
        }
    }

    private void FetchMineralsFromDatabase()
    {
        string currentCarId = simulationManager.roverIds[simulationManager.curIdx];
        var materialsRef = FirebaseManager.dbReference
            .Child("Simulations")
            .Child(firebaseManager.simulationId)
            .Child("Avatars")
            .Child(currentCarId);

        materialsRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || !task.Result.Exists)
            {
                Debug.LogWarning("Failed to fetch minerals.");
                return;
            }

            mineralData.Clear();
            foreach (var mineralEntry in task.Result.Children)
            {
                string mineralId = mineralEntry.Child("id").Value.ToString();
                string positionX = mineralEntry.Child("position").Child("latitude").Value.ToString();
                string positionZ = mineralEntry.Child("position").Child("longitude").Value.ToString();
                mineralData.Add((mineralId, positionX, positionZ));
            }
            currentPage = 0;
            UpdatePagination();
        });
    }

    private void UpdatePagination()
    {
        view.ClearMineralEntries();
        int start = currentPage * mineralsPerPage;
        int end = Mathf.Min(start + mineralsPerPage, mineralData.Count);

        for (int i = start; i < end; i++)
        {
            var (id, x, z) = mineralData[i];
            view.CreateMineralEntry(id, x, z);
        }

        view.SetPaginationControls(
            hasPrev: currentPage > 0,
            hasNext: end < mineralData.Count
        );
    }

    public void OnNextPage()
    {
        if ((currentPage + 1) * mineralsPerPage < mineralData.Count)
        {
            currentPage++;
            UpdatePagination();
        }
    }

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
        string currentCarId = simulationManager.roverIds[simulationManager.curIdx];
        firebaseManager.StoreMaterialData(mineral, currentCarId, firebaseManager.simulationId);

        // Optional: if you want the collected mineral to show up immediately
        // Extract mineral info (assuming Mineral script has a public ID + position)
        string id = mineral.name;
        string x = mineral.transform.position.x.ToString("F2");
        string z = mineral.transform.position.z.ToString("F2");
        mineralData.Add((id, x, z));

        // Refresh UI if inventory is currently open
        if (view.IsOpen())
        {
            UpdatePagination();
        }
    }
}
