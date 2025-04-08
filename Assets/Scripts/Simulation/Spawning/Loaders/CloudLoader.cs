using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Loaders;
class CloudLoader : Loader
{
    private GameObject DustCloudPrefab;
    private GameObject MarsTerrain;
    private Transform SimulationRoot;

    private Texture2D dustTexture;
    public Texture2D DustTexture{
        get {return dustTexture;}
        set {dustTexture = value;}
    }

    private GameObject CloudInstance;
    private Color dustColouring;

    public Color DustColouring{
        get {return dustColouring;}
        set {dustColouring = value;}
    }

    private readonly float CloudHeight = 250f; // Height above terrain
    private readonly float cloudScrollSpeed = 0.005f;
    private int row;
    private int col;
    public class Factory : MonoBehaviourFactory{
        public static CloudLoader Create(int row, int col, GameObject DustCloudPrefab, GameObject MarsTerrain, Transform SimulationRoot, GameObject gameObject = null){
            CloudLoader cl = Create<CloudLoader>(gameObject);
            cl.row = row;
            cl.col = col;
            cl.DustCloudPrefab = DustCloudPrefab;
            cl.MarsTerrain = MarsTerrain;
            cl.SimulationRoot = SimulationRoot;
            return cl;
        }
    }
    public override void Load()
    {
        StartCoroutine(DownloadDustTexture(row, col));
    }

    private IEnumerator DownloadDustTexture(int row, int col)
    {
        string dustURL = $"https://trek.nasa.gov/tiles/Mars/EQ/TES_Dust/1.0.0/default/default028mm/{0}/{0}/{0}.png";
        UnityWebRequest dustRequest = UnityWebRequestTexture.GetTexture(dustURL);
        yield return dustRequest.SendWebRequest();

        if (dustRequest.result == UnityWebRequest.Result.Success)
        {
            dustTexture = DownloadHandlerTexture.GetContent(dustRequest);
            CreateCloudLayer();
        }
        else
        {
            Debug.LogError("Failed to download dust texture: " + dustRequest.error);
        }
    }

    private void CreateCloudLayer()
    {
        if (DustTexture == null)
        {
            Debug.LogError("Dust texture is null!");
            return;
        }

        Terrain terrain = MarsTerrain.GetComponent<Terrain>();
        Vector3 terrainSize = terrain.terrainData.size;

        // 1. Create the cloud object & parent it to the simulation root
        CloudInstance = Instantiate(DustCloudPrefab);
        CloudInstance.transform.SetParent(SimulationRoot, false);

        // 2. Position it locally above the terrain center
        //    Because MarsTerrain is at local (-terrainLength/2, 0, -terrainWidth/2),
        //    the center is roughly (terrainLength/2, 0, terrainWidth/2) in that local space.
        CloudInstance.transform.localPosition = new Vector3(
            0, // offset from the parent's origin
            CloudHeight,           // height above terrain
            0
        );

        // 3. Scale the cloud
        CloudInstance.transform.localScale = new Vector3(
            TerrainInfo.TERRAIN_LENGTH / 10f,
            1f,
            TerrainInfo.TERRAIN_WIDTH / 10f
        );

        // 4. Assign material & set up scrolling
        Renderer renderer = CloudInstance.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("No Renderer component found on cloud prefab!");
            return;
        }

        Material cloudMat = new Material(Shader.Find("Unlit/Transparent"));
        cloudMat.mainTexture = DustTexture;
        DustTexture.wrapMode = TextureWrapMode.Repeat;
        renderer.material = cloudMat;

        CloudScroller scroller = CloudInstance.AddComponent<CloudScroller>();
        scroller.scrollSpeed = cloudScrollSpeed;
        scroller.materialInstance = cloudMat;

        isLoaded = true;
    }

}