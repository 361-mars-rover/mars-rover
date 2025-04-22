using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Tilemaps;

public class MineralSpawner : Spawner
{
    private int spawnTileRow;
    private int spawnTileCol;

    int maxTileRow = 128;
    int maxTileCol = 256;
    int maxMineralRow = 2;
    int maxMineralCol = 4;

    [Header("Spawning Settings")]
    public GameObject rockPrefab;
    private int numberOfRocks = 10000;
    public float spawnRadius = 500f;
    public float spawnDelay = 1f;

    [Header("Mineral Data")]
    public Texture2D mineralTexture;
    private Terrain targetTerrain;
    private Vector3 terrainPosition;
    private Vector3 terrainSize;

    [Header("Spawn Probability")]
    public AnimationCurve spawnProbabilityCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 0.1f),
        new Keyframe(1f, 1f)
    );

    private Vector2 uvTileCenter;
    private Rect uvTileRect;

    public void Init(int row, int col)
    {
        spawnTileRow = row;
        spawnTileCol = col;

        ComputeMineralUVSubrect(row, col, out uvTileCenter, out uvTileRect);
    }

    public override void Spawn()
    {
        if (mineralTexture == null)
        {
            Debug.LogError("No mineral texture loaded!");
            return;
        }

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = numberOfRocks * 5;

        while (spawned < numberOfRocks && attempts < maxAttempts)
        {
            Vector3 randomPosition = GetRandomTerrainPosition();

            if (randomPosition != Vector3.zero && CheckMineralValue(randomPosition))
            {
                CreateRock(randomPosition);
                spawned++;
            }
            attempts++;
        }

        Debug.Log($"Spawned {spawned} minerals (attempts: {attempts})");
    }

    IEnumerator Start()
    {
        // Wait for terrain to initialize
        while (Terrain.activeTerrain == null)
            yield return null;

        // Get terrain reference
        targetTerrain = Terrain.activeTerrain;
        terrainPosition = targetTerrain.transform.position;
        terrainSize = targetTerrain.terrainData.size;

        // Download the correct mineral tile texture based on mapped indices
        yield return StartCoroutine(DownloadMineralTexture(spawnTileRow, spawnTileCol));

        // Start spawning
        StartCoroutine(DelayedSpawn());
    }

    private IEnumerator DownloadMineralTexture(int tileRow, int tileCol)
    {
        // Map high-res tile to mineral grid indices
        float rowNorm = (float)tileRow / maxTileRow;
        float colNorm = (float)tileCol / maxTileCol;

        int mineralRow = Mathf.FloorToInt(rowNorm * maxMineralRow);
        int mineralCol = Mathf.FloorToInt(colNorm * maxMineralCol);

        string mineralURL =
            $"https://trek.nasa.gov/tiles/Mars/EQ/TES_Plagioclase/1.0.0/default/default028mm/1/{mineralRow}/{mineralCol}.png";

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(mineralURL))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                mineralTexture = DownloadHandlerTexture.GetContent(request);
                Debug.Log($"Loaded mineral texture: {mineralTexture.width}x{mineralTexture.height}");
            }
            else
            {
                Debug.LogError($"Failed to load mineral texture: {request.error}");
            }
        }
    }

    private IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(spawnDelay);
        Spawn();
    }

    private bool CheckMineralValue(Vector3 worldPosition)
    {
        // Compute normalized world UV within the tile
        Vector2 worldUV = new Vector2(
            (worldPosition.x - terrainPosition.x) / terrainSize.x,
            (worldPosition.z - terrainPosition.z) / terrainSize.z
        );

        // Map worldUV into the sub-rectangle of the mineral texture
        Vector2 sampleUV = new Vector2(
            uvTileRect.xMin + worldUV.x * uvTileRect.width,
            uvTileRect.yMin + worldUV.y * uvTileRect.height
        );

        Color pixel = mineralTexture.GetPixelBilinear(sampleUV.x, sampleUV.y);

        // Decode TES parameters (0-1 range)
        float plagioclaseConcentration = pixel.r * 0.2f;
        float albedoMask = pixel.g;

        bool isEquatorialLowAlbedo = albedoMask > 0.7f;
        bool aboveDetectionLimit = plagioclaseConcentration >= 0.05f;
        if (!isEquatorialLowAlbedo || !aboveDetectionLimit){
            // Debug.Log($"Low albedo: ${isEquatorialLowAlbedo}. Above detection limit: ${aboveDetectionLimit}");
            return false;
        }

        float spawnProbability = Mathf.InverseLerp(0.05f, 0.2f, plagioclaseConcentration);
        float boostedProbability = Mathf.Clamp01(spawnProbability * 100f);
        return Random.value < boostedProbability;
    }

    private void CreateRock(Vector3 position)
    {
        GameObject rock = Instantiate(rockPrefab, position, Quaternion.identity);
        rock.transform.SetParent(transform.parent);
        rock.transform.rotation = Random.rotation;
        rock.transform.localScale = Vector3.one * 0.2f;
    }

    private Vector3 GetRandomTerrainPosition()
    {
        float randomX = Random.Range(terrainPosition.x, terrainPosition.x + terrainSize.x);
        float randomZ = Random.Range(terrainPosition.z, terrainPosition.z + terrainSize.z);
        Vector3 pos = new Vector3(randomX, 0, randomZ);
        pos.y = targetTerrain.SampleHeight(pos);
        return pos;
    }

    private void ComputeMineralUVSubrect(
        int tileRow, int tileCol,
        out Vector2 uvCenter, out Rect uvRect
    )
    {

        float rowScaled = (tileRow / (float)maxTileRow) * maxMineralRow;
        float colScaled = (tileCol / (float)maxTileCol) * maxMineralCol;

        int mRow = Mathf.FloorToInt(rowScaled);
        int mCol = Mathf.FloorToInt(colScaled);
        float fracRow = rowScaled - mRow;
        float fracCol = colScaled - mCol;

        float cellU = 1f / (maxMineralCol + 1);
        float cellV = 1f / (maxMineralRow + 1);

        uvRect = new Rect(
            x: (mCol + fracCol) * cellU,
            y: (mRow + fracRow) * cellV,
            width: cellU,
            height: cellV
        );

        uvCenter = new Vector2(
            uvRect.xMin + uvRect.width * 0.5f,
            uvRect.yMin + uvRect.height * 0.5f
        );
    }
}