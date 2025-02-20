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
        // this means the texture is represented in game by 513x513 points
        // this is not the same as the number of pixels in the image, which is 256x256
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
        tileCol = col;
        tileRow = row;
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
    }

    // Sets heights based on texture data
    void ApplyHeightmap(Texture2D texture) {
        /*
        To understand this function it's first important to distinguish between the resolution of the heightmap and
        the texture. The heightmap is the actual terrain used in game, which is a set of points (x,y,z) representing
        the points of the terrain. The texture is the image we're using to generate the heightmaps. Notably, the heightmap
        and image DO NOT HAVE THE SAME RESOLUTION ((513x513) and (256x256) respectively). 
        
        This function takes points in the heightmap, maps them to points on the texture, and then computes the height.
        This function is a surjection, since many points on the heightmap map to a single point on the texture.
        */
        int resolution = terrainData.heightmapResolution; // number of pixels in the *height map* is resolution x resolution
        float[,] heights = new float[resolution, resolution];

        // Each pixel’s color represents elevation data, typically encoded in the red channel.
        // This is a flattened matrix (ie. [[r1],[r2]] -> [r1,r2])
        Color[] pixels = texture.GetPixels();

        // loop through the resolution grid pixel by pixel
        for (int row = 0; row < resolution; row++) {
            int texRow = (int)((row / (float)resolution) * texture.height); // get corresponding row in texture
            for (int col = 0; col < resolution; col++) {
                int texCol = (int)((col / (float)resolution) * texture.width); // get corresponding col in texture
                Color pixel = pixels[texRow * texture.width + texCol]; // get pixel data from the image

                // compute height based on the red channel
                // since the image is grayscale, this is simply in the range [0,1], where lighter point are higher
                float heightValue = pixel.r * ELEVATION_RANGE + MIN_ELEVATION; 

                // Normalize to Unity terrain height range
                heights[row, col] = Mathf.Clamp01((heightValue - MIN_ELEVATION) / ELEVATION_RANGE * heightScale);
            }
        }

        heights = SmoothHeights(heights, resolution, blurIterations);

        terrainData.SetHeights(0, 0, heights);

        terrain.terrainData = terrainData;
        terrainCollider.terrainData = terrainData;
    }

    // Smooths heights to avoid sharp edges
    float[,] SmoothHeights(float[,] heights, int resolution, int iterations) {
        // Moves a cross along the terrain and sets the height of each pixel to the average of its neighbours
        // to generate smoother terrain
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