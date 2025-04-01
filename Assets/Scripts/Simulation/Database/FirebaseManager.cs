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
    void Start() {
        // Get the root reference location of the database.
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("firebase" + dbReference);
    }

    void Awake()
    {
        // create sim id based on date
        simulationId = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public static void StoreMaterialData(GameObject mineral, string carId, string simId)
    {
        DatabaseReference newMineralRef = dbReference.Child("materials").Child(simId).Child(carId).Push(); 
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
