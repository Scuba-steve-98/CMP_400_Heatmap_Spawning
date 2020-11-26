using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles
{
    float threatLevel;
    Vector3 location;

    
    public Tiles(float threat, Vector3 pos)
    {
        threatLevel = threat;
        location = pos;
    }


    public void init(float threat, Vector3 pos)
    {
        threatLevel = threat;
        location = pos;
    }


    public float getThreatLevel()
    {
        return threatLevel;
    }


    public Vector3 getLocation()
    {
        return location;
    }
}
