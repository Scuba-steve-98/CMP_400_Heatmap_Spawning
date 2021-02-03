using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossibleSpawns
{
    Vector3 location;
    float closenessToTarget;
    float closestEnemy;
    int noOfEnemiesSeen;
    float[] distanceToEnemies;

    // Start is called before the first frame update
    void Start()
    {
        closenessToTarget = 150;
        closestEnemy = 1000;
        noOfEnemiesSeen = 0;
    }

    // Update is called once per frame
    void Update()
    {

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
}
