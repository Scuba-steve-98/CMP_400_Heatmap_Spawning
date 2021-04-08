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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            findTDMSpawn(team);
        }
    }

    public Vector3 findTDMSpawn(int team)
    {
        Array.Sort<CESpawns>(CESpawns, delegate (CESpawns x, CESpawns y) { return x.getTeamDistances(team).CompareTo(y.getTeamDistances(team)); });
        blocked.Clear();
        int j;

        for (int i = 0; i < CESpawns.Length; i++)
        {
            if (i < CESpawns.Length - 1)
            {
                if (CESpawns[i].sortArray(Math.Abs((team - 1))) < CESpawns[i + 1].sortArray(Math.Abs((team - 1))))
                {
                    if ((CESpawns[i].sortArray(Math.Abs((team - 1))) >= 1 || CESpawns[i].getTeamDistances(team) < 3) && (CESpawns[i].sortArray(Math.Abs((team - 1))) >= 1 || CESpawns[i].getTeamDistances(team) < 3))
                    {
                        blocked.Add(i);
                    }
                    else
                    {
                        do
                        {
                            j = UnityEngine.Random.Range(0, i);
                        } while (blocked.Contains(j));
                        Debug.Log(CESpawns[j].name + "  y");
                        return CESpawns[j].gameObject.transform.position;
                    }
                }
            }
            else
            {
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
                    do
                    {
                        j = UnityEngine.Random.Range(0, i);
                    } while (blocked.Contains(j));
                    Debug.Log(CESpawns[j].name + "  y");
                    return CESpawns[j].gameObject.transform.position;
                }
            }

            Debug.Log(CESpawns[i].getTeamDistances(team) + CESpawns[i].getName());
        }
        do
        {
            j = UnityEngine.Random.Range(0, CESpawns.Length);
        } while (blocked.Contains(j));
        Debug.Log(CESpawns[j].name + "  f");
        return CESpawns[j].gameObject.transform.position;
    }

    public Vector3 findFFASpawn()
    {
        Array.Sort<CESpawns>(CESpawns, delegate (CESpawns x, CESpawns y) { return x.getFFAPercentage().CompareTo(y.getFFAPercentage()); });
        blocked.Clear();
        int j;

        for (int i = 0; i < CESpawns.Length; i++)
        {
            if (i < CESpawns.Length - 1)
            {
                if (CESpawns[i].getFFAPercentage() < CESpawns[i + 1].getFFAPercentage())
                {
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
            else
            {
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
        do
        {
            j = UnityEngine.Random.Range(0, CESpawns.Length);
        } while (blocked.Contains(j));
        return CESpawns[UnityEngine.Random.Range(0, CESpawns.Length)].gameObject.transform.position;
    }
}
