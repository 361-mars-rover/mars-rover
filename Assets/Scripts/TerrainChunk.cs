using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TerrainChunk : MonoBehaviour {

    //terrain type input
    public Terrain terrain;

    // terrainCollider
    public TerrainCollider terrainCollider;

    public TerrainData terrainData;

    private string baseURL = "https://trek.nasa.gov/tiles/Mars/EQ/Mars_MOLA_blend200ppx_HRSC_Shade_clon0dd_200mpp_lzw/1.0.0/default/default028mm";

    private float WMS_PIXEL_SIZE = 0.28e-3f;
    
    [Header("Terrain Settings")]

    // temp scales to get map to look good, to be changed
    public float heightScale = 1f;
    public int blurIterations = 2;
    // Real size of tiles
    private const float TILE_SIZE_KM = 100;
    private const float MIN_ELEVATION = -8000f;  // Lowest point on Mars
    private const float MAX_ELEVATION = 21000f;  // Olympus Mons peak
    private const float ELEVATION_RANGE = MAX_ELEVATION - MIN_ELEVATION;

    private const float SCALE_DENOMINATOR = 1.0907329542893544E+06f;

    private const float TILE_WIDTH = 256f;

    [Header("Tile Settings")]

    public int tileMatrixSet;
    public int tileRow;
    public int tileCol;

    // Initializes size of terrain data
    void Awake()
    {
        terrainData = new TerrainData();
        terrainData.heightmapResolution = 513;
    }
    // Generates the terrain from NASA data
    // This is called in ChunkHandler, which specifies the row and col
    public void Inititialize(int row, int col)
    {
        if (terrain == null) {
            Debug.LogError("Terrain reference not set!");
            return;
        }

        terrainData.size = new Vector3(GetTileSpan(), ELEVATION_RANGE, GetTileSpan());
        StartCoroutine(DownloadHeightmap(row,col));
    }

    // Gets pixel span (span = height or width) in meters based on WMS docs: https://www.ogc.org/publications/standard/wmts/
    float GetPixelSpan(){return  SCALE_DENOMINATOR * WMS_PIXEL_SIZE;}

    // Gets tile span in meters ()
    float GetTileSpan()
    {return TILE_WIDTH * GetPixelSpan();}
    // Fills URL for API request
    string GetDownloadURL(int row, int col)
    {
        return $"{baseURL}/{tileMatrixSet}/{row}/{col}.jpg";
    }


    // Downloads heightmap
    IEnumerator DownloadHeightmap(int row, int col) 
    {        
        string url = GetDownloadURL(row, col);

        UnityWebRequest dataRequest = UnityWebRequestTexture.GetTexture(url);
        yield return dataRequest.SendWebRequest();

        //applies API texture to terrain
        if (dataRequest.result == UnityWebRequest.Result.Success) {
            Texture2D texture = DownloadHandlerTexture.GetContent(dataRequest);
            ApplyHeightmap(texture);
        } else {
            Debug.LogError("Failed to download heightmap: " + dataRequest.error);
        }

        if (row == 0 && col == 0)
        {
            Debug.Log("Logging heihgt");
            Debug.Log(terrain.SampleHeight(new Vector3(39091.87f, 0f, 39091.87f)));
        }
    }

    // Sets heights based on texture data
    void ApplyHeightmap(Texture2D texture) {
        // This block of code gets our terrain inputs current data
        int resolution = terrainData.heightmapResolution;
        float[,] heights = new float[resolution, resolution];
        //each pixel color represents the height
        Color[] pixels = texture.GetPixels();

        // loop through the resolution grid pixel by pixel
        for (int y = 0; y < resolution; y++) {
            int texY = (int)((y / (float)resolution) * texture.height);
            for (int x = 0; x < resolution; x++) {
                int texX = (int)((x / (float)resolution) * texture.width);

                Color pixel = pixels[texY * texture.width + texX];

                float heightValue = pixel.r * ELEVATION_RANGE + MIN_ELEVATION;

                // Normalize to Unity terrain height range
                heights[y, x] = Mathf.Clamp01((heightValue - MIN_ELEVATION) / ELEVATION_RANGE * heightScale);
            }
        }

        heights = SmoothHeights(heights, resolution, blurIterations);

        terrainData.SetHeights(0, 0, heights);

        terrain.terrainData = terrainData;
        terrainCollider.terrainData = terrainData;
    }

    // Smooths heights to avoid sharp edges
    float[,] SmoothHeights(float[,] heights, int resolution, int iterations) {
        for (int i = 0; i < iterations; i++) {
            for (int y = 1; y < resolution - 1; y++) {
                for (int x = 1; x < resolution - 1; x++) {
                    // Simple box blur
                    float avg = (
                        heights[y, x] +
                        heights[y, x + 1] +
                        heights[y, x - 1] +
                        heights[y + 1, x] +
                        heights[y - 1, x]
                    ) / 5f;
                    heights[y, x] = avg;
                }
            }
        }
        return heights;
    }
}