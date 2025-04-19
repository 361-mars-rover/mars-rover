namespace Loaders{
    public abstract class LoaderMonoBehaviour : MonoBehaviourFactory{
        protected bool isLoaded = false;
        public bool getIsLoaded(){
            return isLoaded;
        }

        public abstract void Load();
    }
}
