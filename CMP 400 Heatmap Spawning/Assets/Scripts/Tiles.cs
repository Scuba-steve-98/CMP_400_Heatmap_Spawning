using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles
{
    int noEnemiesSeen;
    float closestEnemy;
    float threatLevel;
    Vector3 location;
    bool isActive;

    
    public Tiles(float threat, Vector3 pos)
    {
        threatLevel = threat;
        location = pos;
        // add no. enemies and distance later
    }


    public Tiles(Vector3 pos)
    {
        //threatLevel = threat;
        location = pos;
        // add no. enemies and distance later
    }


    public Tiles(Vector3 pos, bool active)
    {
        //threatLevel = threat;
        location = pos;
        isActive = active;
        // add no. enemies and distance later
    }


    public void init(float threat, Vector3 pos)
    {
        //threatLevel = threat;
        location = pos;
    }


    public void setValues(float threat)
    {
        threatLevel = threat;
    }


    public float getThreatLevel()
    {
        return threatLevel;
    }


    public Vector3 getLocation()
    {
        return location;
    }


    public bool getActive()
    {
        return isActive;
    }
}
