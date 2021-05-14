using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class HaloCESpawnSelector : MonoBehaviour
{
    List<int> blocked;

    CESpawns[] CESpawns;

    [SerializeField, Range(0, 1)]
    int team = 0;

    // Start is called before the first frame update
    void Start()
    {        
        CESpawns = FindObjectsOfType<CESpawns>();
        blocked = new List<int>();
    }

    public Vector3 findTDMSpawn(int team)
    {
        // sorts the array based on the block chance for the players team
        Array.Sort<CESpawns>(CESpawns, delegate (CESpawns x, CESpawns y) { return x.getTeamDistances(team).CompareTo(y.getTeamDistances(team)); });
        blocked.Clear();
        int j;

        // loops through every spawn point
        for (int i = 0; i < CESpawns.Length; i++)
        {
            // to ensure that when checking the next member it doesn't try access a member outside the bounds of the array
            if (i < CESpawns.Length - 1)
            {
                // checks if the first member is better than the second and randomly selects the spawn based on how many times it has looped through
                if (CESpawns[i].sortArray(Math.Abs((team - 1))) < CESpawns[i + 1].sortArray(Math.Abs((team - 1))))
                {
                    // blocks the member if enemy player is too close or if teammate is too close
                    if ((CESpawns[i].sortArray(Math.Abs((team - 1))) >= 1 || CESpawns[i].getTeamDistances(team) < 3) || (CESpawns[i].sortArray(Math.Abs((team - 1))) >= 1 && CESpawns[i].getTeamDistances(team) < 3))
                    {
                        blocked.Add(i);
                    }
                    else
                    {
                        // continually loop through until it finds an unblocked spawn point
                        do
                        {
                            j = UnityEngine.Random.Range(0, i);
                        } while (blocked.Contains(j));
                        return CESpawns[j].gameObject.transform.position;
                    }
                }
            }
            else
            {
                // blocks bad spawns 
                if (CESpawns[i].sortArray(Math.Abs((team - 1))) >= 1)
                {
                    blocked.Add(i);
                }
                if (CESpawns[i].getTeamDistances(team) < 3)
                {
                    blocked.Add(i);
                }
                else
                {
                    // finds a random unblocked spawn
                    do
                    {
                        j = UnityEngine.Random.Range(0, i);
                    } while (blocked.Contains(j));
                    return CESpawns[j].gameObject.transform.position;
                }
            }
        }
        // finds a random unblocked spawn because all paths need to return a value
        do
        {
            j = UnityEngine.Random.Range(0, CESpawns.Length);
        } while (blocked.Contains(j));
        return CESpawns[j].gameObject.transform.position;
    }

    public Vector3 findFFASpawn()
    {
        // sorts the array based on the block chance for the players team
        Array.Sort<CESpawns>(CESpawns, delegate (CESpawns x, CESpawns y) { return x.getFFAPercentage().CompareTo(y.getFFAPercentage()); });
        blocked.Clear();
        int j;

        // loops through every spawn point
        for (int i = 0; i < CESpawns.Length; i++)
        {
            // to ensure that when checking the next member it doesn't try access a member outside the bounds of the array
            if (i < CESpawns.Length - 1)
            {
                // checks if the block chance is better than the next members
                if (CESpawns[i].getFFAPercentage() < CESpawns[i + 1].getFFAPercentage())
                {
                    // if enemy is too close block it
                    if (CESpawns[i].getFFAPercentage() >= 1)
                    {
                        blocked.Add(i);
                    }
                    // continually loop through until it finds an unblocked spawn point
                    do
                    {
                        j = UnityEngine.Random.Range(0, i);
                    } while (blocked.Contains(j));
                    return CESpawns[UnityEngine.Random.Range(0, i)].gameObject.transform.position;
                }
            }
            else
            {
                // finds a random unblocked spawn because all paths need to return a value
                if (CESpawns[i].getFFAPercentage() >= 1)
                {
                    blocked.Add(i);
                }
                do
                {
                    j = UnityEngine.Random.Range(0, i);
                } while (blocked.Contains(j));
                return CESpawns[UnityEngine.Random.Range(0, i)].gameObject.transform.position;
            }
        }
        // finds a random unblocked spawn because all paths need to return a value
        do
        {
            j = UnityEngine.Random.Range(0, CESpawns.Length);
        } while (blocked.Contains(j));
        return CESpawns[UnityEngine.Random.Range(0, CESpawns.Length)].gameObject.transform.position;
    }
}
