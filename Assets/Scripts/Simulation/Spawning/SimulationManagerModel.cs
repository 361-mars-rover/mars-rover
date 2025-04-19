using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class SimulationManagerModel : MonoBehaviour
{
    public List<Vector2Int> TileIndices = new List<Vector2Int>();
    public List<GameObject> sims = new List<GameObject>();
    public int curIdx = 0;
    public List<string> roverIds = new List<string>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
}