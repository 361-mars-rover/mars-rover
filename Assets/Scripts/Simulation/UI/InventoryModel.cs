using System;
using System.Collections.Generic;
using UnityEngine;


/*
Authors: Jikael and Chloe
This is the model part of the model-view-presenter design pattern. In this design pattern, the model 
is responsible for holding the data. In our case, the data is a list of minerals collected by the rover.
This data is manipulated by the presenter class. It stores the number of minerals collected by a rover.
This class has no "knowledge" of the View class.
*/

public class InventoryModel : MonoBehaviour
{
    public List<(string id, float x, float z)> mineralData = new();

    public void CollectMineral(GameObject mineral)
    {
        string id = mineral.name;
        float x = mineral.transform.position.x;
        float z = mineral.transform.position.z;

        mineralData.Add((id, x, z));
    }

    public void Clear()
    {
        mineralData.Clear();
    }
}
