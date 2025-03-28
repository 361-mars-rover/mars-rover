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
    void Start() {
        // Get the root reference location of the database.
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("firebase" + dbReference);
    }

    void Awake()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public static void StoreMaterialData(GameObject mineral)
    {
        DatabaseReference newMineralRef = dbReference.Child("materials").Child(CarControl.id).Push(); 
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
public class Rover
{
    public string avatar;
    public string brain;
    
    public Rover(string avatar, string brain)
    {
        this.avatar = avatar;
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
