// using System.Collections;
// using UnityEngine;
// using UnityEngine.Networking;

// class CloudSpawner : MonoBehaviour{

//     private Texture2D dustTexture;
//     private GameObject cloudInstance;

//     void Spawn(){
//                 if (dustTexture == null)
//         {
//             Debug.LogError("Dust texture is null!");
//             return;
//         }

//         Terrain terrain = marsTerrain.GetComponent<Terrain>();
//         Vector3 terrainSize = terrain.terrainData.size;

//         // 1. Create the cloud object & parent it to the simulation root
//         cloudInstance = Instantiate(dustCloudPrefab);
//         cloudInstance.transform.SetParent(simulationRoot, false);

//         // 2. Position it locally above the terrain center
//         //    Because marsTerrain is at local (-terrainLength/2, 0, -terrainWidth/2),
//         //    the center is roughly (terrainLength/2, 0, terrainWidth/2) in that local space.
//         cloudInstance.transform.localPosition = new Vector3(
//             0, // offset from the parent's origin
//             cloudHeight,           // height above terrain
//             0
//         );

//         // 3. Scale the cloud
//         cloudInstance.transform.localScale = new Vector3(
//             TerrainLength / 10f,
//             1f,
//             TerrainWidth / 10f
//         );

//         // 4. Assign material & set up scrolling
//         Renderer renderer = cloudInstance.GetComponent<Renderer>();
//         if (renderer == null)
//         {
//             Debug.LogError("No Renderer component found on cloud prefab!");
//             return;
//         }

//         Material cloudMat = new Material(Shader.Find("Unlit/Transparent"));
//         cloudMat.mainTexture = dustTexture;
//         dustTexture.wrapMode = TextureWrapMode.Repeat;
//         renderer.material = cloudMat;

//         CloudScroller scroller = cloudInstance.AddComponent<CloudScroller>();
//         scroller.scrollSpeed = cloudScrollSpeed;
//         scroller.materialInstance = cloudMat;

//         dustIsLoaded = true;
//     }
//     IEnumerator DownloadDustTexture(int row, int col)
//     {
//         // TODO: Stop hardcoding this???
//         string dustURL = $"https://trek.nasa.gov/tiles/Mars/EQ/TES_Dust/1.0.0/default/default028mm/{0}/{0}/{0}.png";
//         UnityWebRequest dustRequest = UnityWebRequestTexture.GetTexture(dustURL);
//         yield return dustRequest.SendWebRequest();

//         if (dustRequest.result == UnityWebRequest.Result.Success)
//         {
//             dustTexture = DownloadHandlerTexture.GetContent(dustRequest);
//             CreateCloudLayer();
//         }
//         else
//         {
//             Debug.LogError("Failed to download dust texture: " + dustRequest.error);
//         }
//     }
// }