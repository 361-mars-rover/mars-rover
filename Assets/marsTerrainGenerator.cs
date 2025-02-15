using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Xml;
using System;

public class MarsGlobalTerrain : MonoBehaviour {

    //terrain type input
    public Terrain terrain;

    // terrainCollider
    public TerrainCollider terrainCollider;

    public TerrainData terrainData;

    // mars API url (in future make x and y variables to change depending on cars location)
    private string wmtsURL = "https://trek.nasa.gov/tiles/Mars/EQ/Mars_MOLA_blend200ppx_HRSC_Shade_clon0dd_200mpp_lzw/1.0.0/WMTSCapabilities.xml";
    private string baseURL = "https://trek.nasa.gov/tiles/Mars/EQ/Mars_MOLA_blend200ppx_HRSC_Shade_clon0dd_200mpp_lzw/1.0.0/default/default028mm";

    public bool isLoaded = false;

    private float WMS_PIXEL_SIZE = 0.28e-3f;

    Dictionary<string, string> tileData = new Dictionary<string, string>();

    
    [Header("Terrain Settings")]

    // temp scales to get map to look good, to be changed
    public float heightScale = 1f;
    public int blurIterations = 2;
    // Real size of tiles
    private const float TILE_SIZE_KM = 100;
    private const float MIN_ELEVATION = -8000f;  // Lowest point on Mars
    private const float MAX_ELEVATION = 21000f;  // Olympus Mons peak
    private const float ELEVATION_RANGE = 5000f;

    [Header("Tile Settings")]

    public int tileMatrixSet;
    public int tileRow;
    public int tileCol;

    // start function, checks terrain input and calls other functions
    void Awake()
    {
        // Debug.Log("Mars terrain being awoken");
        terrainData = new TerrainData();
        terrainData.heightmapResolution = 513;
    }

    void Start() {}

    public void Inititialize(int row, int col)
    {
        if (terrain == null) {
            Debug.LogError("Terrain reference not set!");
            return;
        }
        // Debug.Log("Terrain Data: " + terrain.terrainData);
        // Debug.Log("Terrain Collider Data: " + terrainCollider.terrainData);

        // ConfigureTerrainSize();
        StartCoroutine(DownloadHeightmap(row,col));
    }

    /// <summary>
    /// Gets XML data from wmtsURL for the tileMatrix selected in tileMatrixSet
    /// This data is added to tileData and is used afterwards to call the API
    /// The following is an example of tileMatrix data:
    /// 
    /// <example>
    /// Identifier: 4
    /// ScaleDenominator: 1.7451727268629670E+07
    /// TopLeftCorner: -180.0 90.0
    /// TileWidth: 256
    /// TileHeight: 256
    /// MatrixWidth: 32.0
    /// MatrixHeight: 16.0
    ///  </example>
    /// 
    /// This means that the data with Identifier 4 has 32 rows and 4 columns,
    /// where each tile has shape 256x256
    /// 
    /// </summary>
    IEnumerator GetTileData()
    {
        UnityWebRequest xmlRequest = UnityWebRequest.Get(wmtsURL);
        yield return xmlRequest.SendWebRequest();

        if (xmlRequest.result == UnityWebRequest.Result.Success) {
            // Debug.Log("WMTS Capabilities downloaded successfully!");
        } else {
            Debug.LogError("Failed to get WMTS: " + xmlRequest.error);
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlRequest.downloadHandler.text);
        XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
        nsManager.AddNamespace("wmts", "http://www.opengis.net/wmts/1.0");
        nsManager.AddNamespace("ows", "http://www.opengis.net/ows/1.1");

        XmlNode tileMatrix = xmlDoc.SelectSingleNode($"//wmts:TileMatrix[ows:Identifier='{tileMatrixSet}']", nsManager);

        if (tileMatrix != null)
        {
            foreach (XmlNode childNode in tileMatrix.ChildNodes){
                // Debug.Log(childNode?.Name);
                // Debug.Log(childNode?.InnerText);
                tileData[childNode.Name] = childNode.InnerText;
            }

        }
        else
        {
            // Debug.LogWarning($"TileMatrix with ID {tileMatrixSet} not found.");
        }

        // Debug.Log($"Pixel Span: {GetPixelSpan()}");
        // Debug.Log($"Tile Span: {GetTileSpan()}");
        // Debug.Log($"Grid Width: {GetGridWidth()}");


       terrainData.size = new Vector3(GetTileSpan(), ELEVATION_RANGE, GetTileSpan());
    //    terrainData.size = new Vector3(100000f, ELEVATION_RANGE, 100000f);


    }

    float GetPixelSpan()
    {
        float scaleDenominator = (float) Convert.ToDouble(tileData["ScaleDenominator"]);
        return  scaleDenominator * WMS_PIXEL_SIZE;
    }

    float GetTileSpan()
    {
        float tileWidth = (float) Convert.ToInt32(tileData["TileWidth"]);
        return tileWidth * GetPixelSpan();
    }

    float GetGridWidth()
    {
        float nRows = (float) Convert.ToDouble(tileData["MatrixHeight"]);
        return GetTileSpan() * nRows;
    }

    string GetDownloadURL(int tileRow, int tileCol)
    {
        return $"{baseURL}/{tileMatrixSet}/{tileRow}/{tileCol}.jpg";
    }

    // z, x, y currently usuless (for rendering later)
    IEnumerator DownloadHeightmap(int row, int col) 
    {        
        yield return GetTileData();
        // string url = string.Format(baseURL, z, x, y);
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

    
    void ApplyHeightmap(Texture2D texture) {
        // This block of code gets our terrain inputs current data
        // TerrainData terrainData = terrain.terrainData;
        int resolution = terrainData.heightmapResolution;
        float[,] heights = new float[resolution, resolution];
        //each pixel color represents the height
        Color[] pixels = texture.GetPixels();
        //comment

        // loop through the resolution grid pixel by pixel
        for (int y = 0; y < resolution; y++) {
            int texY = (int)((y / (float)resolution) * texture.height);
            for (int x = 0; x < resolution; x++) {
                int texX = (int)((x / (float)resolution) * texture.width);

                Color pixel = pixels[texY * texture.width + texX];

                float heightValue = pixel.r * ELEVATION_RANGE + MIN_ELEVATION;

                // Normalize to Unity terrain height range
                heights[y, x] = Mathf.Clamp01((heightValue - MIN_ELEVATION) / ELEVATION_RANGE * heightScale);

                if (x == 0 && y == 0) { // Only print for one pixel to avoid spam
                    // Debug.Log($"Pixel[{x}, {y}]: {pixel.r}, Computed Height: {heightValue}");
                    // Debug.Log($"Pixel Value (Red): {pixel.r}");
                }
            }
        }

        heights = SmoothHeights(heights, resolution, blurIterations);

        terrainData.SetHeights(0, 0, heights);

        terrain.terrainData = terrainData;
        terrainCollider.terrainData = terrainData;
        isLoaded = true;
    }

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