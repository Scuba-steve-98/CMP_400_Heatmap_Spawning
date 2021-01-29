using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    GameManager gameManager_;

    [SerializeField, Range(0, 10)]
    float overallKD;

    [SerializeField, Range(0, 10)]
    float currentKD;

    [SerializeField, Range(60, 100)]
    float baseThreat;

    float health, threatLevel, friendlyLevel;

    int kills, deaths, team;

    // Start is called before the first frame update
    void Start()
    {
        gameManager_ = FindObjectOfType<GameManager>().GetComponent<GameManager>();
        threatLevel = baseThreat;
        friendlyLevel = baseThreat / 2;
        Debug.Log(threatLevel);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void killedEnemy()
    {
        kills++;
        if (deaths == 0)
        {
            if (gameManager_.getGameProgress() < 0.4f)
            {
                threatLevel = (kills / (1 / 3)) * overallKD;
                friendlyLevel = baseThreat / threatLevel;
                threatLevel *= baseThreat;
            }
            else if (gameManager_.getGameProgress() >= 0.4f)
            {
                threatLevel = (kills / 1) * overallKD;
                friendlyLevel = baseThreat / threatLevel;
                threatLevel *= baseThreat;
            }
        }
        else if (deaths > 0)
        {
            threatLevel = (kills / deaths) * overallKD;
            friendlyLevel = baseThreat / threatLevel;
            threatLevel *= baseThreat;
        }
        if (gameManager_.isTDM())
        {
            //threatLevel += gameManager_.getTeamThreat(team);
        }
    }

    void die()
    {
        deaths++;
    }
    

    public void setTeam(int teamNo)
    {
        team = teamNo;
        Debug.Log(team);
    }
    

    public int getTeam()
    {
        return team;
    }

    public float getThreatLevel()
    {
        return threatLevel;
    }

    public float getFriendLevel()
    {
        return friendlyLevel;
    }
}
