using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;
    public static DatabaseReference dbReference;
    private bool isFirebaseInitialized = false;
    public string simulationId;
    public bool isTerrainDataStored = false;
    void Start() {
        // Get the root reference location of the database.
        // dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        // Debug.Log("firebase" + dbReference);
        simulationId = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    void Awake()
    {
        // create sim id based on date
        
    }

    public void StoreMarsTerrainData(string simId, float terrainWidth, float terrainLength, float spawnTileRow, float spawnTileCol) 
    {
        if (isTerrainDataStored) return; 
        DatabaseReference terrainRef = dbReference.Child("Simulations").Child(simId).Child("MarsGeospatialData");
        float min_pos_x = spawnTileRow * terrainWidth;
        float min_pos_y = spawnTileCol * terrainLength;
        float max_pos_x = (spawnTileRow + 1) * terrainWidth;
        float max_pos_y = (spawnTileCol + 1) * terrainLength;
        Debug.Log($"Terrain position: {spawnTileCol}, {spawnTileRow}");

        var terrainData = new Dictionary<string, object>{
            {"North-west X-coord", min_pos_x},
            {"North-west Y-coord", max_pos_y},
            {"North-east X-coord", max_pos_x},
            {"North-east Y-coord", max_pos_y},
            {"South-west X-coord", min_pos_x},
            {"South-west Y-coord", min_pos_y},
            {"South-east X-coord", max_pos_x},
            {"South-east Y-coord", min_pos_y},
            {"Area length", terrainLength},
            {"Area Width", terrainWidth}
        };

        terrainRef.SetValueAsync(terrainData);
        isTerrainDataStored = true; 
    }

    public void StoreMaterialData(GameObject mineral, string carId, string simId)
    {
        DatabaseReference newMineralRef = dbReference.Child("Simulations").Child(simId).Child("Avatars").Child(carId).Push(); 
        var mineralData = new Dictionary<string, object>{
            {"id", mineral.name},
            {"position", new Dictionary<string, float>{
                {"x", mineral.transform.position.x},
                {"z", mineral.transform.position.z}
            }}
        };
        newMineralRef.SetValueAsync(mineralData)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"Mineral ID {mineral.name} saved with unique key: {newMineralRef.Key}");
                }
                else
                {
                    Debug.LogError("Error saving mineral ID: " + task.Exception);
                }
            });
    }
}


// rover data model
[System.Serializable]
public class AvatarData
{
    public string rover;
    public string brain;
    
    public AvatarData(string rover, string brain)
    {
        this.rover = rover;
        this.brain = brain;
    }
}

// material data model
[System.Serializable]
public class MaterialData
{
    // public string type;
    // public string collected_by;
    
    public MaterialData()
    // public MaterialData(string type, string collected_by)
    {
        // this.type = type;
        // this.collected_by = collected_by;
    }
}
