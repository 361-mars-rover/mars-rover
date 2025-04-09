using UnityEngine;

namespace AI{
     public abstract class AIControllerBase : MonoBehaviour
    {
    }
    public abstract class AIController<T> : AIControllerBase{
        public abstract T AIUpdate();
    }     
}