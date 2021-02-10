using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossibleSpawns
{
    Vector3 location;
    float closenessToTarget;
    float closestEnemy, closestFriendly;
    int noOfEnemiesSeen, noOfFriendliesSeen;
    bool isSpawn = false;

    //// Start is called before the first frame update
    //void Start()
    //{
    //    closenessToTarget = 150;
    //    closestEnemy = 1000;
    //    noOfEnemiesSeen = 0;
    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

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

    public void setCloseness(float close)
    {
        closenessToTarget = close;
    }

    public float getCloseness()
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

    public void setEnemiesSeen(int no)
    {
        noOfEnemiesSeen = no;
    }

    public int getEnemiesSeen()
    {
        return noOfEnemiesSeen;
    }

    public void setClosest(float close)
    {
        closestEnemy = close;
    }

    public float getClosest()
    {
        return closestEnemy;
    }

    public void setFriendliesSeen(int no)
    {
        noOfFriendliesSeen = no;
    }

    public int getFriendliesSeen()
    {
        return noOfFriendliesSeen;
    }

    public void setClosestFriendly(float close)
    {
        closestFriendly = close;
    }

    public float getClosestFriendly()
    {
        return closestFriendly;
    }
}
