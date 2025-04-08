using UnityEngine;

class CloudLoaderFactory : MonoBehaviourFactory{
    public static CloudLoader Create(int row, int col, GameObject DustCloudPrefab, GameObject MarsTerrain, Transform SimulationRoot, GameObject gameObject = null){
        CloudLoader cl = Create<CloudLoader>(gameObject);
        cl.row = row;
        cl.col = col;
        cl.DustCloudPrefab = DustCloudPrefab;
        cl.MarsTerrain = MarsTerrain;
        cl.SimulationRoot = SimulationRoot;
        return cl;
    }
}