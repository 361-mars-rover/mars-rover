using UnityEngine;

/*
JIKAEL
An abstract class that defines the behaviour of loaders. The role of a loader
is to load some objects into the simulation dynamically.
*/
public abstract class Loader : MonoBehaviour{
    protected bool isLoaded = false;

    public bool IsLoaded{
        get {return isLoaded;}
    }
    public abstract void Load();
}
