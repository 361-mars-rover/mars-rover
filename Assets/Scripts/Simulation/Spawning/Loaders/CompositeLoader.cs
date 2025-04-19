using System.Collections.Generic;
using Loaders;
using UnityEngine;

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