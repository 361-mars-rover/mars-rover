using System;
using UnityEngine;

public class Avatar
{
    public string ID;
    public string Name;
    public string Description;
    public Vector2Int row_col;

    public Brain brain;

    public Avatar(){
        ID = Guid.NewGuid().ToString();
    }
}
