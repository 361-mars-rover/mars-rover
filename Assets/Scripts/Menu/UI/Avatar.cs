using System;
using UnityEngine;

public class Avatar
{
    private static int nextID = 0;
    public Rover rover;
    public Brain brain;
    public int ID { get; private set; }
    public string name;
    public string description;

    public Vector2Int SpawnRowCol;

    public Avatar(Rover pRover, Brain pBrain) {
        rover = pRover;
        brain = pBrain;

        ID = nextID++;
    }
    public Avatar() {
        ID = nextID ++;
    }

}
