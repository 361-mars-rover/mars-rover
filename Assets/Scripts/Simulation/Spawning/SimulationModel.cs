using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class SimulationManagerModel : MonoBehaviour
{
    public Vector2Int[] TileIndices;

    private int MAX_ROW = 128;
    private int MAX_COL = 256;
    public List<GameObject> sims = new List<GameObject>();
    public int curIdx = 0;
    public List<string> roverIds = new List<string>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
}