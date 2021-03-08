using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuzzyTiles
{
    //float threatLevel, team1Threat, team2Threat, friendly1Level, friendly2Level;
    Vector3 location;
    //public float closenessToTarget;

    DANGER danger = (DANGER)10;
    DANGER team1Danger = (DANGER)10;
    DANGER team2Danger = (DANGER)10;
    DANGER friendly1 = (DANGER)10;
    DANGER friendly2 = (DANGER)10;

    CLOSENESS closeness = (CLOSENESS)6;


    public FuzzyTiles(DANGER threat, Vector3 pos)
    {
        danger = threat;
        location = pos;
        // add no. enemies and distance later
    }


    public FuzzyTiles(Vector3 pos)
    {
        location = pos;
        // add no. enemies and distance later
    }


    public void setValues(DANGER threat)
    {
        danger = threat;
    }


    public void setValues(DANGER team1, DANGER team2, DANGER friend1, DANGER friend2)
    {
        team1Danger = team1;
        team2Danger = team2;
        friendly1 = friend1;
        friendly2 = friend2;

        team1Danger = (team1Danger - friendly1) + team1Danger;
        team2Danger = (team2Danger - friendly2) + team2Danger;
    }


    public DANGER getThreatLevel()
    {
        return danger;
    }


    public DANGER getTeamThreatLevel(int team)
    {
        if (team == 0)
        {
            return team1Danger;
        }
        else
        {
            return team2Danger;
        }
    }


    public DANGER getFriendLevel(int team)
    {
        if (team == 0)
        {
            return friendly1;
        }
        else
        {
            return friendly2;
        }
    }


    public Vector3 getLocation()
    {
        return location;
    }

    public void setCloseness(CLOSENESS close)
    {
        closeness = close;
    }

    public CLOSENESS getCloseness()
    {
        return closeness;
    }
}
