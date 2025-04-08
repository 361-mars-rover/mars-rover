using UnityEngine;
using Loaders;
class InvisibleWallLoader : Loader
{
    public BoxCollider wall1;
    public BoxCollider wall2;
    public BoxCollider wall3;
    public BoxCollider wall4;
    private float wallHeight = 120f;

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