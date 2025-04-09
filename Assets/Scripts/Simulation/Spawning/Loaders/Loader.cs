using UnityEngine;

namespace Loaders{
    public abstract class Loader : MonoBehaviour{
        protected bool isLoaded = false;

        public bool IsLoaded{
            get {return isLoaded;}
        }
        public abstract void Load();
    }
}
