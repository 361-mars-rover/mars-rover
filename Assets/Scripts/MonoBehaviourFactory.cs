using UnityEngine;

// https://dev.to/iamscottcab/instantiating-monobehaviours-in-unity-5c2g
/*
JIKAEL
From a blog post about how to instantiate mono behaviour children w/ params
*/
public class MonoBehaviourFactory : MonoBehaviour
{
    // Generic method that creates an object of the given type.
    // This is overriden in various classes to also take parameters
    protected static T Create<T>(GameObject gameObject = null) where T : MonoBehaviour{
        GameObject obj = gameObject;
        if (obj == null)
            obj = new GameObject(typeof(T).ToString());
        
        return obj.AddComponent<T>();
    }
}