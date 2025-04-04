using UnityEngine;
using Loaders;
class InvisibleWallLoader : LoaderMonoBehaviour
{
    private BoxCollider wall1;
    private BoxCollider wall2;
    private BoxCollider wall3;
    private BoxCollider wall4;
    private float wallHeight = 120f;
    public static InvisibleWallLoader Create(BoxCollider wall1, BoxCollider wall2, BoxCollider wall3, BoxCollider wall4, GameObject gameObject = null){
        InvisibleWallLoader iwl = Create<InvisibleWallLoader>(gameObject);
        iwl.wall1 = wall1;
        iwl.wall2 = wall2;
        iwl.wall3 = wall3;
        iwl.wall4 = wall4;
        return iwl;
    }
    public override void Load()
    {
        wall1.center = new Vector3(0, wallHeight / 2, TerrainInfo.TERRAIN_LENGTH / 2);
        wall1.size = new Vector3(TerrainInfo.TERRAIN_WIDTH, wallHeight, 1f);

        // South Wall
        wall2.center = new Vector3(0, wallHeight / 2, -TerrainInfo.TERRAIN_LENGTH / 2);
        wall2.size = new Vector3(TerrainInfo.TERRAIN_WIDTH, wallHeight, 1f);

        // East Wall
        wall3.center = new Vector3(TerrainInfo.TERRAIN_WIDTH / 2, wallHeight / 2, 0);
        wall3.size = new Vector3(1f, wallHeight, TerrainInfo.TERRAIN_LENGTH);

        // West Wall
        wall4.center = new Vector3(-TerrainInfo.TERRAIN_WIDTH / 2, wallHeight / 2, 0);
        wall4.size = new Vector3(1f, wallHeight, TerrainInfo.TERRAIN_LENGTH);
    }
}