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

    [SerializeField]
    public bool isTestingLifespan = true;

    [SerializeField]
    bool isDemoing = true;

    int noOfPlayers;
    int playersInTeam;
    int highestScore;

    int[] teamScore;
    int[] teamDeaths;

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

        if (!isDemoing)
        {
            // determines filepath based on testing parameters
            if (isTestingLifespan)
            {
                filename = Application.dataPath + "/CSV_Files/averageLifespan_" + gameType + "_" + SceneManager.GetActiveScene().name + ".csv";
            }
            else
            {
                filename = Application.dataPath + "/CSV_Files/" + spawnType + "_" + gameType + "_" + SceneManager.GetActiveScene().name + ".csv";
            }
            Debug.Log(filename);
            Time.timeScale = 2;
        }
        else
        {
            Time.timeScale = 1;
        }

    }

    // Update is called once per frame
    void Update()
    {
        // single use conditional statement used to ensure things are initialised correctly.
        if (!trigger)
        {
            trigger = true;

            // initialises based on the game mode and the spawning type
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

        // used to keep track of the game's runtime
        timer += Time.deltaTime;

        // ends game 
        if (timer >= gameTime)
        {
            GameOver();
        }

        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }
    }

    // sets players team based on game mode
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

    // keeps track of scoring to know if the game ends
    public void updateFFAScore(int team)
    {
        teamScore[team]++;

        if (teamScore[team] > highestScore)
        {
            highestScore = teamScore[team];

            gameProgress = (float)highestScore / (float)scoreLimit;
        }

        if (highestScore >= scoreLimit)
        {
            GameOver();
        }
    }
    
    // keeps track of scoring to know if the game ends
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
            gameProgress = (float)highestScore / (float)scoreLimit;
        }


        // sets team threat multipliers
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


        // ends game
        if (highestScore == scoreLimit)
        {
            GameOver();
        }
    }

    // checks game mode
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

    // gets how far the game is to completion
    public float getGameProgress()
    {
        return gameProgress;
    }

    // gets the threat multipliers for the teams
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

    // gets what spawn type is being used
    public SPAWN_TYPE getSpawnType()
    {
        return spawnType;
    }

    // returns how many kills is needed to end the game
    public int getGoal()
    {
        return scoreLimit;
    }

    // ends the game
    void GameOver()
    {
        if (isDemoing)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            WriteToCSV();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // exports the data from demoing
    void WriteToCSV()
    {
        TextWriter playerData;
        int iteration;

        // creates the headers if the file doesnt exist
        if (!File.Exists(filename))
        {
            playerData = new StreamWriter(filename, false);
            if (isTestingLifespan)
            {
                playerData.WriteLine("Player, Average Lifespan, Longest Life, Shortest Life, Kills, Deaths");
            }
            else
            {
                if (isTDM())
                {
                    playerData.WriteLine("Player, Average Time to Encounter, Kills, Deaths, Current KD, Game RunTime, Team");
                }
                else
                {
                    playerData.WriteLine("Player, Average Time to Encounter, Kills, Deaths, Current KD, Game RunTime");
                }
            }
            
            playerData.Close();
            iteration = 1;
        }
        // gets how many time the game has run
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

        // outputs the data to the CSV
        playerData = new StreamWriter(filename, true);
        foreach (Player p in players)
        {
            if (isTestingLifespan)
            {
                playerData.WriteLine(p.gameObject.name + "," + p.OutputData().averageLifespan + "," + p.OutputData().longestLifespan + "," + p.OutputData().shortestLifespan +"," + p.kills + "," + p.deaths);
            }
            else
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
        }
        playerData.WriteLine(iteration);
        playerData.Close();

        // closes the game
       if (iteration == 10)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}