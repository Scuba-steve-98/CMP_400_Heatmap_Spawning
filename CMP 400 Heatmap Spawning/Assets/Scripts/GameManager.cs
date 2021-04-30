using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

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

    [SerializeField]
    public bool isHaloBattleCreek = false;

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

    string filename = "";

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
        if (gameType == GAMETYPE.TDM)
        {
            gameTime = 600;
            scoreLimit = 75;
        }
        else
        {
            gameTime = 300;
            scoreLimit = 35;
        }
        

        if (noOfPlayers % 2 == 0)
        {
            playersInTeam = noOfPlayers / 2;
        }
        else
        {
            playersInTeam2++;
        }

        ranking = new int[noOfPlayers];
        playersScore = new int[noOfPlayers];
        filename = Application.dataPath + "/CSV_Files/" + spawnType + "_" + gameType + "_" + SceneManager.GetActiveScene().name + ".csv";
        Debug.Log(filename);
        Time.timeScale = 2;
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
            Debug.Log("Game Over: " + highestScore + "   Time: " + Time.time);
            GameOver();
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

            gameProgress = (float)highestScore / (float)scoreLimit;
        }

        if (highestScore >= scoreLimit)
        {
            Debug.Log("Game Over: " + highestScore + "   Time: " + Time.time);
            GameOver();
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
            gameProgress = (float)highestScore / (float)scoreLimit;
        }

        if (teamDeaths[0] == 0)
        {
            team1KD = (float)teamScore[0] / 1;
        }
        else
        {
            team1KD = (float)teamScore[0] / (float)teamDeaths[0];
        }
        
        if (teamDeaths[1] == 0)
        {
            team2KD = (float)teamScore[1] / 1;
        }
        else
        {
            team2KD = (float)teamScore[1] / (float)teamDeaths[1];
        }
        float multiplier = 1 - ((float)Mathf.Abs(teamScore[0] - teamScore[1]) / (float)scoreLimit);


        if (team1KD == 0)
        {
            team1Threat = multiplier;
        }
        else
        {
            team1Threat = team1KD * multiplier;
        }

        if (team2KD == 0)
        {
            team2Threat = multiplier;
        }
        else
        {
            team2Threat = team2KD * multiplier;
        }

        if (highestScore == scoreLimit)
        {
            Debug.Log("Game Over: " + highestScore + "   Time: " + Time.time);
            GameOver();
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

    public int getGoal()
    {
        return scoreLimit;
    }


    void GameOver()
    {
        Debug.Log("Game Over");
        WriteToCSV();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    void WriteToCSV()
    {
        TextWriter playerData;
        int iteration;
        if (!File.Exists(filename))
        {
            playerData = new StreamWriter(filename, false);
            if (isTDM())
            {
                playerData.WriteLine("Player, Average Time to Encounter, Kills, Deaths, Current KD, Game RunTime, Team");
            }
            else
            {
                playerData.WriteLine("Player, Average Time to Encounter, Kills, Deaths, Current KD, Game RunTime");
            }
            
            playerData.Close();
            iteration = 1;
        }
        else
        {
            string[] i = File.ReadAllLines(filename);
            iteration = int.Parse(i[i.Length - 1]) + 1;
            Debug.Log(iteration);

            playerData = new StreamWriter(filename, true);
            playerData.WriteLine();
            playerData.WriteLine();
            playerData.Close();
        }

        playerData = new StreamWriter(filename, true);
        foreach (Player p in players)
        {
            if (isTDM())
            {
                playerData.WriteLine(p.gameObject.name + "," + p.OutputData().averageEC + "," + p.kills + "," + p.deaths + "," + p.OutputData().currentKD + "," + Time.timeSinceLevelLoad + "," + p.getTeam());
            }
            else
            {
                playerData.WriteLine(p.gameObject.name + "," + p.OutputData().averageEC + "," + p.kills + "," + p.deaths + "," + p.OutputData().currentKD + "," + Time.timeSinceLevelLoad);
            }
            
        }
        playerData.WriteLine(iteration);
        playerData.Close();

        if (iteration == 100 && spawnType == SPAWN_TYPE.FUZZY)
        {
            SceneManager.LoadScene(1);
        }        
        else if (iteration == 100 && spawnType == SPAWN_TYPE.RULE_BASED)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }
}