using UnityEngine;
using System.Collections.Generic;

public class ExplorationTracker : MonoBehaviour
{
    public float baseRadius = 100f; // Exploration range for each base
    public float circleStep = 10f; // Step for each concentric circle
    public float mineralSearchRadius = 50f; // If minerals found, search this extra radius

    private HashSet<Vector2Int> exploredCircles = new HashSet<Vector2Int>();
    private List<Vector3> mineralLocations = new List<Vector3>();
    private HashSet<Vector2Int> exploredBases = new HashSet<Vector2Int>();

    public bool IsCircleExplored(Vector3 center, float radius)
    {
        Vector2Int gridPos = WorldToGrid(center, radius);
        return exploredCircles.Contains(gridPos);
    }

    public void MarkCircleAsExplored(Vector3 center, float radius)
    {
        Vector2Int gridPos = WorldToGrid(center, radius);
        exploredCircles.Add(gridPos);
    }

    public void MarkBaseAsExplored(Vector3 baseCenter)
    {
        Vector2Int gridPos = WorldToGrid(baseCenter, baseRadius);
        exploredBases.Add(gridPos);
    }

    public bool IsBaseExplored(Vector3 baseCenter)
    {
        Vector2Int gridPos = WorldToGrid(baseCenter, baseRadius);
        return exploredBases.Contains(gridPos);
    }

    public void SaveMineral(Vector3 position)
    {
        mineralLocations.Add(position);
    }

    public int GetMineralCount()
    {
        return mineralLocations.Count;
    }

    private Vector2Int WorldToGrid(Vector3 worldPos, float radius)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPos.x / radius), Mathf.RoundToInt(worldPos.z / radius));
    }
}
