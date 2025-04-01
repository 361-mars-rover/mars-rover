using System;
using UnityEngine;

public class Avatar
{
    public string ID;
    public string Name;
    public string Description;

    public Avatar(){
        ID = Guid.NewGuid().ToString();
    }
}
