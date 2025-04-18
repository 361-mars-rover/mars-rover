using UnityEngine;

public class InventoryPresenter : MonoBehaviour
{
    [SerializeField] private SimulationManager simulationManager;
    [SerializeField] private FirebaseManager firebaseManager;

    private InventoryModel model;
    private InventoryView view;
    private void Awake()
    {
        simulationManager = FindFirstObjectByType<SimulationManager>();
        firebaseManager = FindFirstObjectByType<FirebaseManager>();
        model = new InventoryModel();
        view = FindFirstObjectByType<InventoryView>();
        model.OnMineralCollected += HandleMineralCollected;
    }

    private void OnDestroy()
    {
        model.OnMineralCollected -= HandleMineralCollected;
    }

    public void CollectMineral(GameObject mineral)
    {
        model.CollectMineral(mineral);
    }

    private void HandleMineralCollected(InventoryModel updatedModel)
    {
        Debug.Log($"Mineral collected for rover: {simulationManager.roverIds[simulationManager.curIdx]}");
        Debug.Log($"Total Minerals Collected: {updatedModel.NumberOfMinerals}");
        Debug.Log($"View is null: {view == null}");
        Debug.Log($"Model is null: {model == null}");
        Debug.Log($"Model num minerals is null: {model.NumberOfMinerals == null}");
        view.UpdateMineralCount(model.NumberOfMinerals);
        if (firebaseManager != null)
        {
            firebaseManager.StoreMaterialData(
                updatedModel.Minerals[^1], // last collected mineral
                simulationManager.roverIds[simulationManager.curIdx],
                firebaseManager.simulationId
            );
        }
    }

    public int GetMineralCount() => model.NumberOfMinerals;
}
