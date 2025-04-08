using UnityEngine;

class TerrainLoaderFactory : MonoBehaviourFactory{
    public static TerrainLoader Create(int row, int col, Transform simulationRoot, GameObject marsTerrain, GameObject gameObject = null){
        Debug.Log("Creating a spawner");
        TerrainLoader tl = Create<TerrainLoader>(gameObject);
        tl.row = row;
        tl.col = col;
        tl.simulationRoot = simulationRoot;
        tl.marsTerrain = marsTerrain;
        return tl;
    }
}