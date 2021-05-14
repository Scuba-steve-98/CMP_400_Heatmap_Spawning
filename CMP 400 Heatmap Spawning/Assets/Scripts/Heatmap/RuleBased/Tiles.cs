using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles
{
    float threatLevel, team1Threat, team2Threat, friendly1Level, friendly2Level;
    Vector3 location;
    public float closenessToTarget;


    public Tiles(Vector3 pos)
    {
        location = pos;
    }
    public void setValues(float threat)
    {
        threatLevel = threat;
    }


    public void setValues(float team1, float team2, float friend1, float friend2)
    {
        team1Threat = team1;
        team2Threat = team2;
        friendly1Level = friend1;
        friendly2Level = friend2;

        team1Threat -= friendly1Level;
        team2Threat -= friendly2Level;
    }


    public Vector3 getLocation()
    {
        return location;
    }
}
