using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossibleFuzzySpawns
{
    CLOSENESS closeness = (CLOSENESS)16;
    CLOSENESS friendlyCloseness = (CLOSENESS)6;
    ENEMIES_SEEN enemiesSeen = (ENEMIES_SEEN)3;
    ENEMIES_SEEN friendliesSeen = (ENEMIES_SEEN)3;

    Vector3 location;
    bool isSpawn = false;
    int closenessToTarget;

    public PossibleFuzzySpawns()
    {

    }

    public PossibleFuzzySpawns(Vector3 loc)
    {
        location = loc;
    }

    public void setSpawn()
    {
        isSpawn = true;
    }

    public void setSpawn(bool spawn)
    {
        isSpawn = spawn;
    }

    public bool getSpawn()
    {
        return isSpawn;
    }

    public void setCloseness(int close)
    {
        closenessToTarget = close;
    }

    public int getCloseness()
    {
        return closenessToTarget;
    }

    public void setLocation(Vector3 loc)
    {
        location = loc;
    }

    public Vector3 getLocation()
    {
        return location;
    }

    public void setEnemiesSeen(ENEMIES_SEEN no)
    {
        enemiesSeen = no;
    }

    public ENEMIES_SEEN getEnemiesSeen()
    {
        return enemiesSeen;
    }

    public void setClosest(CLOSENESS close)
    {
        closeness = close;
    }

    public CLOSENESS getClosest()
    {
        return closeness;
    }

    public void setFriendliesSeen(ENEMIES_SEEN no)
    {
        friendliesSeen = no;
    }

    public ENEMIES_SEEN getFriendliesSeen()
    {
        return friendliesSeen;
    }

    public void setClosestFriendly(CLOSENESS close)
    {
        friendlyCloseness = close;
    }

    public CLOSENESS getClosestFriendly()
    {
        return friendlyCloseness;
    }
}
