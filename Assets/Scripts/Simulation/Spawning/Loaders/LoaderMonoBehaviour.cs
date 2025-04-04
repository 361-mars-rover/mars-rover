namespace Loaders{
    public abstract class LoaderMonoBehaviour : ParameterizedMonoBehaviour{
        protected bool isLoaded = false;
        public bool getIsLoaded(){
            return isLoaded;
        }

        public abstract void Load();
    }
}
