using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CESpawns : MonoBehaviour
{
    float FFABlockDistance, FFABlockPercentage;

    float[] team1Data, team2Data;

    List<Player> players;

    GameManager gameManager_;

    [SerializeField]
    AnimationCurve block;

    // Start is called before the first frame update
    void Start()
    {
        gameManager_ = FindObjectOfType<GameManager>();
        players = new List<Player>();
        team1Data = new float[2];
        team2Data = new float[2];
    }

    private void FixedUpdate()
    {
        FFABlockDistance = 100;
        team1Data[0] = 100;
        team2Data[0] = 100;

        for (int i = 0; i < players.Count; i++)
        {
            if (gameManager_.isTDM())
            {
                if (players[i].getTeam() == 0)
                {
                    float temp = Vector3.Distance(players[i].transform.position, transform.position);
                    if (temp < team1Data[0])
                    {
                        team1Data[0] = temp;

                        if (team1Data[0] > 23)
                        {
                            team1Data[1] = 0;
                        }
                        else if (team1Data[0] > 15)
                        {
                            team1Data[1] = block.Evaluate((team1Data[0] - 15) / 8);
                            if (team1Data[1] > 1)
                            {
                                team1Data[1] = 1;
                            }
                        }
                        else
                        {
                            team1Data[1] = 1;
                        }
                    }
                }
                else
                {
                    float temp = Vector3.Distance(players[i].transform.position, transform.position);
                    if (temp < team2Data[0])
                    {
                        team2Data[0] = temp;

                        if (team2Data[0] > 23)
                        {
                            team2Data[1] = 0;
                        }
                        else if (team2Data[0] > 15)
                        {
                            team2Data[1] = block.Evaluate((team2Data[0] - 15) / 8);
                            if (team2Data[1] > 1)
                            {
                                team2Data[1] = 1;
                            }
                        }
                        else
                        {
                            team2Data[1] = 1;
                        }
                    }
                }
            }
            else
            {
                float temp = Vector3.Distance(players[i].transform.position, transform.position);
                if (temp < FFABlockDistance)
                {
                    FFABlockDistance = temp;
                    if (FFABlockDistance > 23)
                    {
                        FFABlockPercentage = 0;
                    }
                    else if (FFABlockDistance > 15)
                    {
                        FFABlockPercentage = block.Evaluate((FFABlockDistance - 15) / 8);
                    }
                    else
                    {
                        FFABlockPercentage = 1;
                    }
                }
            }
        }
    }

    public float getFFAPercentage()
    {
        return FFABlockPercentage;
    }

    public float getTeamDistances(int team)
    {
        if (team == 0)
        {
            return team1Data[0];
        }
        else 
            return team2Data[0];
    }

    public string getName()
    {
        return gameObject.name;
    }

    public float sortArray(int team)
    {
        if (team == 0)
        {
            return team1Data[0];
        }
        else
            return team2Data[0];
    }

    private void OnTriggerEnter(Collider other)
    {
        Player p;
        if (other.TryGetComponent<Player>(out p))
        {
            if (!players.Contains(p))
            {
                players.Add(p);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Player p;
        if (other.TryGetComponent<Player>(out p))
        {
            if (players.Contains(p))
            {
                players.Remove(p);
            }
        }
    }
}