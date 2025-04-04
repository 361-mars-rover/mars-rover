abstract class SpawnerMonoBehaviour : ParameterizedMonoBehaviour{
    protected bool isLoaded = false;
    public bool getIsLoaded(){
        return isLoaded;
    }

    public abstract void Spawn();
}