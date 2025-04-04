using UnityEngine;

// https://dev.to/iamscottcab/instantiating-monobehaviours-in-unity-5c2g
public class ParameterizedMonoBehaviour : MonoBehaviour
{
    protected static T Create<T>(GameObject gameObject = null) where T : MonoBehaviour{
        GameObject obj = gameObject;
        if (obj == null)
            obj = new GameObject(typeof(T).ToString());
        
        return obj.AddComponent<T>();
    }
}