using UnityEngine;

namespace Loaders{
    public abstract class Loader : MonoBehaviour{
        protected bool isLoaded = false;
        public bool getIsLoaded(){
            return isLoaded;
        }
        public abstract void Load();
    }
}
