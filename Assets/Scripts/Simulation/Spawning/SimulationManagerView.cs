using TMPro;
using UnityEngine;
class SimulationManagerView : MonoBehaviour{
    [SerializeField] private TextMeshProUGUI roverIDTextMesh;
    [SerializeField] private TextMeshProUGUI indicesTextMesh;
    [SerializeField] private TextMeshProUGUI aiModeMesh;

    
    void Awake()
    {
        roverIDTextMesh.text = "TESTING TEXT!";
    }

    public void SetRoverID(string id){
        roverIDTextMesh.text = $"Rover ID: {id}";
    }

    public void SetTileIndices(Vector2Int tileIndices){
        indicesTextMesh.text = $"Tile: ({tileIndices.x},{tileIndices.y})";
    }

    public void SetAIMode(AIMode aIMode){
        aiModeMesh.text = $"AI mode: {aIMode.ToString()}";
    }
}