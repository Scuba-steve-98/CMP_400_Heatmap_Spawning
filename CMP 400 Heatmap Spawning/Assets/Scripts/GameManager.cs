using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    SPAWN_TYPE spawnType = SPAWN_TYPE.RULE_BASED;
    
    [SerializeField]
    GAMETYPE gameType = GAMETYPE.TDM;

    [SerializeField, Range(35, 75)]
    int scoreLimit = 35;

    [SerializeField, Range(5, 15)]
    int gameTime = 10;

    int noOfPlayers;
    int playersInTeam;
    int playersInTeam2;
    int highestScore;
    int teamLead, teamLoss;

    int[] teamScore;
    int[] teamDeaths;
    int[] ranking;
    int[] playersScore;

    bool trigger = false;

    float gameProgress, team1KD, team2KD, timer, team1Threat, team2Threat;
    
    Player[] players;

    // Start is called before the first frame update
    void Start()
    {
        players = FindObjectsOfType<Player>();
        noOfPlayers = players.Length;
        playersInTeam = noOfPlayers / 2;
        team1KD = 0;
        team2KD = 0;
        timer = 0;

        if (noOfPlayers % 2 == 0)
        {
            playersInTeam = noOfPlayers / 2;
        }
        else
        {
            playersInTeam2++;
        }

        Debug.Log(noOfPlayers + " " + playersInTeam);
        ranking = new int[noOfPlayers];
        playersScore = new int[noOfPlayers];
    }

    // Update is called once per frame
    void Update()
    {
        if (!trigger)
        {
            trigger = true;
            switch (gameType)
            {
                case GAMETYPE.TEST:
                    break;

                case GAMETYPE.FFA:
                    FFAInit();
                    teamScore = new int[noOfPlayers];
                    teamDeaths = new int[noOfPlayers];
                    break;

                case GAMETYPE.TDM:
                    TDMInit();
                    teamScore = new int[2];
                    teamDeaths = new int[2];
                    break;

                default:
                    break;
            }

            switch (spawnType)
            {
                case SPAWN_TYPE.RULE_BASED:
                    RBSpawningSelector rbss = FindObjectOfType<RBSpawningSelector>();
                    rbss.init();
                    break;

                case SPAWN_TYPE.FUZZY:
                    FuzzySpawnSelector fss = FindObjectOfType<FuzzySpawnSelector>();
                    fss.init();
                    break;

                case SPAWN_TYPE.HALO:
                    HaloCESpawnSelector hcess = FindObjectOfType<HaloCESpawnSelector>();
                    break;
                default:
                    break;
            }
        }

        timer += Time.deltaTime;

        if (timer >= gameTime)
        {
            //game Over
        }

        switch (gameType)
        {
            case GAMETYPE.TEST:
                break;

            case GAMETYPE.FFA:
                FFA();
                break;

            case GAMETYPE.TDM:
                TDM();
                break;

            default:
                break;
        }
    }

    void FFAInit()
    {
        for (int i = 0; i < noOfPlayers; i++)
        {
            players[i].GetComponent<Player>().setTeam(i);
        }
    }

    void TDMInit()
    {
        if (noOfPlayers % 2 == 0)
        {
            playersInTeam = noOfPlayers / 2;
        }
        else
        {
            playersInTeam2++;
        }

        for (int i = 0; i < noOfPlayers; i++)
        {
            if (i < playersInTeam)
            {
                players[i].GetComponent<Player>().setTeam(0);
            }
            else
            {
                players[i].GetComponent<Player>().setTeam(1);
            }
        }
    }

    void FFA()
    {

    }

    void TDM()
    {

    }

    public void updateFFAScore(int team)
    {
        teamScore[team]++;

        if (teamScore[team] > highestScore)
        {
            highestScore = teamScore[team];
            teamLead = team;

            gameProgress = scoreLimit / highestScore;
        }

        if (highestScore == scoreLimit)
        {
            // game over
        }
    }

    public void updateTDMScore(int team)
    {
        teamScore[team]++;
        if (team == 0)
        {
            teamDeaths[1]++;
        }
        else
        {
            teamDeaths[0]++;
        }

        if (teamScore[team] > highestScore)
        {
            highestScore = teamScore[team];
            teamLead = team;
            if (teamLead == 0)
            {
                teamLoss = 1;
            }
            else
            {
                teamLoss = 0;
            }
            gameProgress = scoreLimit / highestScore;
        }
        team1KD = teamScore[0] / teamDeaths[0];
        team2KD = teamScore[1] / teamDeaths[1];

        team1Threat = (1 - ((teamScore[0] - teamScore[1]) / scoreLimit * (team1KD - team2KD)));

        team2Threat = (1 - ((teamScore[1] - teamScore[0]) / scoreLimit * (team2KD - team1KD)));

        if (highestScore == scoreLimit)
        {
            // game over
        }
    }

    public bool isTDM()
    {
        if (gameType == GAMETYPE.TDM)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public float getGameProgress()
    {
        return gameProgress;
    }

    public float getTeamKD(int team)
    {
        if (team == 0)
        {
            return team1KD;
        }
        else
        {
            return team2KD;
        }
    }

    public float getTeamThreat(int team)
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


    public SPAWN_TYPE getSpawnType()
    {
        return spawnType;
    }
}