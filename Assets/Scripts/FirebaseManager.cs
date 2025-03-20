using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;
    private DatabaseReference dbReference;
    private bool isFirebaseInitialized = false;
    void Start() {
        // Get the root reference location of the database.
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    void Awake()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void StoreMaterialData(string materialID)
    {
        MaterialData material = new MaterialData();
        string json = JsonUtility.ToJson(material);
        dbReference.Child("materials").Child(materialID).SetRawJsonValueAsync(json);
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
