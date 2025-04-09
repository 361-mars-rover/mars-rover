using UnityEngine;

namespace AI
{
    // Non-generic base class
    public abstract class AIControllerBase : MonoBehaviour
    {
        // Common method that all AI controllers will have
        public abstract void UpdateRover();
    }

    // Generic derived class
    public abstract class AIController<T> : AIControllerBase
    {
        // The specific typed method to get update results
        public abstract T UpdateAI();
        
        // Implementation of base class method using the typed result
        public override void UpdateRover()
        {
            T result = UpdateAI();
            ProcessResult(result);
        }
        
        // Each derived class needs to implement this to process its specific result type
        protected abstract void ProcessResult(T result);
    }
}