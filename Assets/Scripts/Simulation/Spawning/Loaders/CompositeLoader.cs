using System.Collections.Generic;
using UnityEngine;

/*
JIKAEL
A class that composes various loaders into a single loader to allow for multiple objects to 
be loaded in one call.
*/
class CompositeLoader : Loader
{
    private List<Loader> loaders = new List<Loader>();
    public class Factory : MonoBehaviourFactory{
        public static CompositeLoader Create(GameObject gameObject = null, params Loader[] loaders){
            CompositeLoader cl = Create<CompositeLoader>(gameObject);
            foreach(Loader loader in loaders){
                cl.loaders.Add(loader);
            }
            return cl;
        }
    }
    public override void Load()
    {
        foreach(Loader loader in loaders){
            loader.Load();
        }
    }
}