using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    GameManager gameManager_;
    RBSpawningSelector rbSpawningSelector;
    FuzzySpawnSelector fuzzySpawnSelector;
    CODSpawnSelector codSpawnSelector;
    Renderer rend;

    [SerializeField, Range(0, 10)]
    float overallKD = 1;

    [SerializeField, Range(0, 10)]
    float currentKD = 1;

    [SerializeField, Range(60, 100)]
    float baseThreat = 67;

    float health, threatLevel, friendlyLevel;

    int kills, deaths;

    bool isDead = false;

    public int team;

    // Start is called before the first frame update
    void Start()
    {
        //rbSpawningSelector = FindObjectOfType<RBSpawningSelector>();
        gameManager_ = FindObjectOfType<GameManager>().GetComponent<GameManager>();
        threatLevel = baseThreat;
        friendlyLevel = baseThreat / 2;
        rend = GetComponent<Renderer>();

        health = 100;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void killedEnemy()
    {
        kills++;
        updateKD();
    }


    void updateKD()
    {
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
    

    public void setTeam(int teamNo)
    {
        team = teamNo;
        Debug.Log(team);
        if (team == 0)
        {
            rend.material.color = Color.green;
        }
        else
        {
            rend.material.color = Color.blue;
        }
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

    public bool isShot(float dam)
    {
        health -= dam;
        if (health <= 0)
        {
            deaths++;
            isDead = true;
            gameObject.SetActive(false);
            updateKD();
        }
        return isDead;
    }
}
