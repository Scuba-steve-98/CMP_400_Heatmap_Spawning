using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles
{
    //int noEnemiesSeen;
    //float closestEnemy;
    float threatLevel, team1Threat, team2Threat, friendly1Level, friendly2Level;
    Vector3 location;
    public float closenessToTarget;

    public Tiles(float threat, Vector3 pos)
    {
        threatLevel = threat;
        location = pos;
        // add no. enemies and distance later
    }


    public Tiles(Vector3 pos)
    {
        location = pos;
        // add no. enemies and distance later
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


    public float getThreatLevel()
    {
        return threatLevel;
    }


    public float getTeamThreatLevel(int team)
    {
        if (team == 0)
        {
            return team1Threat;
        }
        else
        {
            return team2Threat;
        }
    }


    public float getFriendLevel(int team)
    {
        if (team == 0)
        {
            return friendly1Level;
        }
        else
        {
            return friendly2Level;
        }
    }


    public Vector3 getLocation()
    {
        return location;
    }

    public void setCloseness(float close)
    {
        closenessToTarget = close;
    }

    public float getCloseness()
    {
        return closenessToTarget;
    }
}
