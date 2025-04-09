using System.Collections.Generic;
using UnityEngine;

class Inventory{
    private List<(string id, string x, string z)> mineralIds = new List<(string, string, string)>(); // Store mineral info
    
    public void Clear(){
        mineralIds.Clear();
    }
    public void AddMineral(string mineralId, string positionX, string positionZ){
        mineralIds.Add((mineralId, positionX, positionZ));
    }

    public (string, string, string) GetMineral(int index){
        return mineralIds[index];
    }

    public int GetMineralCount(){
        return mineralIds.Count;
    }
}