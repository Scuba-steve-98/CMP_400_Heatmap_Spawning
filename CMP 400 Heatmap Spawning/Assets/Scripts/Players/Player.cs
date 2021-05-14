﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //---------------------- Code concerning these variables are from the the youtube tutorial for enemy AI
    //---------------------- tutorial is found at URL https://www.youtube.com/watch?v=qL1toYXm_Sc&ab_channel=DitzelGames
    //---------------------- youtube channel is DitzelGames
    [HideInInspector]
    public InputStr Input;
    public struct InputStr
    {
        public float LookX;
        public float LookZ;
        public float RunX;
        public float RunZ;
        public bool Jump;

        public bool SwitchToAK;
        public bool SwitchToPistol;

        public bool Shoot;
        public Vector3 ShootTarget;
    }

    public float Speed = 10f;
    public const float JumpForce = 7f;

    protected Rigidbody Rigidbody;
    protected Quaternion LookRotation;

    public GameObject AKBack;
    public GameObject AKHand;
    public GameObject PistolBack;
    public GameObject PistolHand;

    public AudioClip AudioClipAK;
    public AudioClip AudioClipShot;

    protected float Cooldown;
    protected AudioSource AudioSourcePlayer;
    // code above is from tutorial
    //---------------------------------------------
    //---------------------------------------------

    bool isPlayer;
    float weaponDamage;

    [SerializeField]
    GameObject deathPoint;

    // data for testing and outputting data
    public struct PlayerData
    {
        public List<float> engagedCounter;
        public float averageEC;
        public float currentKD;
        public float averageLifespan;
        public float shortestLifespan;
        public float longestLifespan;
    }

    [HideInInspector]
    public PlayerData playerData;

    GameManager gameManager_;
    HeatmapData heatmapData;
    FuzzyHeatmapData fuzzyHeatmapData;
    HaloCESpawnSelector haloCESpawnSelector;
    LineRenderer lineRenderer;

    Vector3 spawnLocation;
    Renderer rend;


    [SerializeField, Range(0, 10)]
    float overallKD = 1;

    [SerializeField, Range(60, 100)]
    float baseThreat = 67;

    float health, threatLevel, friendlyLevel, deathCoolDown, currentEC, threatLevelMultiplier, lifespan;

    [HideInInspector]
    public int kills, deaths;

    bool isDead = false;
    bool hasBeenEngaged = false;

    [HideInInspector]
    public int team;

    [HideInInspector]
    public int layerMask = 0;

    // Start is called before the first frame update
    void Start()
    {
        // ------------------------- tutorial variables setup
        Rigidbody = GetComponent<Rigidbody>();
        AudioSourcePlayer = GetComponent<AudioSource>();
        AKBack.SetActive(false);
        AKHand.SetActive(true);
        PistolBack.SetActive(true);
        PistolHand.SetActive(false);
        // --------------------------------------------------

        weaponDamage = 12.5f;

        gameManager_ = FindObjectOfType<GameManager>().GetComponent<GameManager>();
        lineRenderer = GetComponent<LineRenderer>();
        threatLevel = baseThreat;
        friendlyLevel = baseThreat / 2;
        rend = GetComponent<Renderer>();
        deaths = 0;
        threatLevelMultiplier = 1;
        lifespan = 0;
        currentEC = 0;

        switch (gameManager_.getSpawnType())
        {
            case SPAWN_TYPE.RULE_BASED:
                heatmapData = FindObjectOfType<HeatmapData>();
                break;
            case SPAWN_TYPE.FUZZY:
                fuzzyHeatmapData = FindObjectOfType<FuzzyHeatmapData>();
                break;
            case SPAWN_TYPE.HALO:
                haloCESpawnSelector = FindObjectOfType<HaloCESpawnSelector>();
                break;
            default:
                break;
        }

        health = 100;
        deathCoolDown = 2.0f;

        layerMask = (1 << 9) | (1 << 10);
        layerMask = ~layerMask;

        if (TryGetComponent<Controller>(out Controller c))
        {
            isPlayer = true;
        }
        else
        {
            isPlayer = false;
        }

        playerData.engagedCounter = new List<float>();
        playerData.averageEC = 0;
        playerData.averageLifespan = 0;
        playerData.shortestLifespan = 0;
        playerData.longestLifespan = 100;

        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
    }

    private void FixedUpdate()
    {
        // this whole function is from the tutorial
        if (isDead)
            return;

        var inputRun = Vector3.ClampMagnitude(new Vector3(Input.RunX, 0, Input.RunZ), 1);
        var inputLook = Vector3.ClampMagnitude(new Vector3(Input.LookX, 0, Input.LookZ), 1);

        Rigidbody.velocity = new Vector3(inputRun.x * Speed, Rigidbody.velocity.y, inputRun.z * Speed);

        //rotation to go target
        if (inputLook.magnitude > 0.01f)
            LookRotation = Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.forward, inputLook, Vector3.up), Vector3.up);

        transform.rotation = LookRotation;
    }

    // Update is called once per frame
    void Update()
    {
        // data for testing
        if (!isDead)
        {
            lifespan += Time.deltaTime;
        }
        if (deaths > 0 && !hasBeenEngaged && !isDead)
        {
            currentEC += Time.deltaTime;
        }

        // time for respawn
        if (isDead && deathCoolDown > 0)
        {
            deathCoolDown -= Time.deltaTime;
        }
        else if (isDead && deathCoolDown <= 0)
        {
            // respawns player based on spawning type
            switch(gameManager_.getSpawnType())
            {
                case SPAWN_TYPE.RULE_BASED:
                    isDead = false;
                    spawnLocation = heatmapData.getHeatmapData(team, kills, deaths, threatLevel);
                    transform.position = spawnLocation;
                    GetComponent<Enemy>().Respawned();
                    health = 100;
                    break;

                case SPAWN_TYPE.FUZZY:
                    isDead = false;
                    spawnLocation = fuzzyHeatmapData.getHeatmapData(team, threatLevel, kills, deaths);
                    transform.position = spawnLocation;
                    GetComponent<Enemy>().Respawned();
                    health = 100;
                    break;

                case SPAWN_TYPE.HALO:
                    if (gameManager_.isTDM())
                    {
                        isDead = false;
                        spawnLocation = haloCESpawnSelector.findTDMSpawn(team);
                        transform.position = spawnLocation;
                        GetComponent<Enemy>().Respawned();
                        health = 100;
                    }
                    else
                    {
                        isDead = false;
                        spawnLocation = haloCESpawnSelector.findFFASpawn();
                        transform.position = spawnLocation;
                        GetComponent<Enemy>().Respawned();
                        health = 100;
                    }
                    break;
                default:
                    break;
            }
            // was meant to be for player testing but didn't get to that 
            if (!isPlayer)
            {
                GetComponent<Rigidbody>().useGravity = true;
            }
        }


        // the rest of the code in this function was inspired by the tutorial, many edits were made to make it work in this project
        Cooldown -= Time.deltaTime;

        // my little function for the bullet lines
        if (Cooldown < 0.1f)
        {
            lineRenderer.enabled = false;
        }

        if (Input.Shoot && !isDead) // added death condition
        {
            if (Cooldown <= 0)
            {
                // removed the animation and changed it to play based on what weapon is active although no audio is implemented 
                if (AKHand)
                {
                    AudioSourcePlayer.PlayOneShot(AudioClipAK);
                    Cooldown = 0.2f;
                }
                else
                {
                    AudioSourcePlayer.PlayOneShot(AudioClipShot);
                    Cooldown = 1f;
                }

                Vector3 shootOrigin = AKHand.transform.position + Vector3.forward * -0.2f;
                Vector3 shootDirection = AKHand.transform.position;
                shootDirection += AKHand.transform.forward * 10;
                shootDirection += Vector3.up * Random.Range(-0.5f, 0.5f) + Vector3.right * Random.Range(-0.5f, 0.5f);
                shootDirection = shootDirection - shootOrigin;
                Ray shootRay = new Ray(shootOrigin, shootDirection);

                lineRenderer.SetPosition(0, AKHand.transform.position);

                //do we hit anybody?
                var hitInfo = new RaycastHit();
                gameObject.layer = Physics.IgnoreRaycastLayer;
                if (Physics.Raycast(shootRay, out hitInfo, 100, layerMask)) // changed to raycasting, some errors occurred with spherecasting
                {
                    Debug.DrawLine(shootRay.origin, hitInfo.point, Color.red);
                    lineRenderer.SetPosition(1, hitInfo.point);
                    lineRenderer.enabled = true;

                    var player = hitInfo.collider.GetComponent<Player>();
                    if (player != null && player != this)
                    {

                        // changed how player takes damage--------------------------------------------
                        if (!player.IsDead())
                        {

                            if (player.isShot(weaponDamage))
                            {
                                kills++;
                                if (gameManager_.isTDM())
                                {
                                    gameManager_.updateTDMScore(team);

                                }
                                else
                                {
                                    gameManager_.updateFFAScore(team);
                                }

                                updateKD();
                                // data for testing
                                if (!hasBeenEngaged && deaths > 0)
                                {
                                    hasBeenEngaged = true;
                                    currentEC += Time.deltaTime;
                                    playerData.engagedCounter.Add(currentEC);
                                    currentEC = 0;
                                }
                            }
                            else
                            {
                                // data for testing
                                if (!hasBeenEngaged && deaths > 0)
                                {
                                    hasBeenEngaged = true;
                                    currentEC += Time.deltaTime;
                                    playerData.engagedCounter.Add(currentEC);
                                    currentEC = 0;
                                }
                            }
                        }
                        //--------------------------------
                    }
                }
                gameObject.layer = 0;
            }
        }

        // changed to fit there being no animations
        if (Input.SwitchToAK)
        {
            Input.SwitchToAK = false;
            AKBack.SetActive(false);
            AKHand.SetActive(true);
            PistolBack.SetActive(true);
            PistolHand.SetActive(false);
            weaponDamage = 12.5f;
        }

        if (Input.SwitchToPistol)
        {
            Input.SwitchToPistol = false;
            PistolBack.SetActive(false);
            PistolHand.SetActive(true);
            AKBack.SetActive(true);
            AKHand.SetActive(false);
            weaponDamage = 34f;
        }
    }

    // all code onwards is mine
    void updateKD()
    {
        threatLevel = 0;
        friendlyLevel = 0;

        // calculates KD based on flow charts in dissertation
        if (deaths == 0)
        {
            if (gameManager_.getGameProgress() > 0.4f)
            {
                threatLevel = (kills / 0.6f) * overallKD;
                friendlyLevel = baseThreat / threatLevel;
                threatLevel *= baseThreat;
            }
            else if (gameManager_.getGameProgress() <= 0.4f)
            {
                threatLevel = (kills / 1) * overallKD;
                friendlyLevel = baseThreat / threatLevel;
                threatLevel *= baseThreat;
            }
            playerData.currentKD = kills;
        }
        else if (deaths > 0)
        {
            if (gameManager_.getGameProgress() < 0.6f && kills == 0)
            {
                threatLevel = overallKD / deaths;
                friendlyLevel = baseThreat / threatLevel;
                threatLevel *= baseThreat;
                playerData.currentKD = kills / deaths;
            }
            else if (gameManager_.getGameProgress() >= 0.6f && kills == 0)
            {
                threatLevel = overallKD / (deaths * 2);
                friendlyLevel = baseThreat / threatLevel;
                threatLevel *= baseThreat;
                playerData.currentKD = kills / deaths;
            }
            else if (kills > (deaths + 2))
            {
                threatLevel = Mathf.Lerp(1f, 3f, ((kills / deaths) / 6));
                if (threatLevel == 0)
                {
                    Debug.LogError("Huh: " + kills + "  :  " + deaths);
                }
                friendlyLevel = baseThreat / threatLevel;
                threatLevel *= baseThreat;
                playerData.currentKD = kills / deaths;
            }
            else
            {
                threatLevel = (overallKD * kills) / deaths;
                if (threatLevel == 0)
                {
                    Debug.LogError("Huh: " + kills + "  :  " + deaths);
                }
                friendlyLevel = baseThreat / threatLevel;
                threatLevel *= baseThreat;
                playerData.currentKD = kills / deaths;
            }
        }
    }
    

    // sets players team
    public void setTeam(int teamNo)
    {
        team = teamNo;
        if (team == 0)
        {
            rend.material.color = Color.green;
        }
        else
        {
            rend.material.color = Color.blue;
        }
    }
    
    // gets what team the player is on
    public int getTeam()
    {
        return team;
    }

    // returns the players threat multiplied by health multiplier
    public float getThreatLevel()
    {
        return threatLevel * threatLevelMultiplier;
    }

    // returns the players friendly level
    public float getFriendLevel()
    {
        return friendlyLevel;
    }

    // returns if the player is dead and damages the player
    public bool isShot(float dam)
    {
        // ensures the player isn't killed again after being already dead
        if (isDead)
        {
            return false;
        }

        // reduces players health
        health -= dam;

        // sets health multiplier based on players health
        if (health == 100)
        {
            threatLevelMultiplier = 1;
        }
        else if (health > 50)
        {
            threatLevelMultiplier = 0.75f;
        }
        else if (health > 25)
        {
            threatLevelMultiplier = 0.6f;
        }
        else
        {
            threatLevelMultiplier = 0.5f;
        }

        // updates player data for testing
        if (!hasBeenEngaged && deaths > 0)
        {
            hasBeenEngaged = true;
            currentEC += Time.deltaTime;
            playerData.engagedCounter.Add(currentEC);
            currentEC = 0;
        }

        // kills player and updates testing data
        if (health <= 0 && !isDead)
        {
            playerData.averageLifespan += lifespan;

            if (deaths == 1)
            {
                playerData.shortestLifespan = lifespan;
                playerData.longestLifespan = lifespan;
            }
            else if (deaths > 1)
            {
                if (lifespan < playerData.shortestLifespan)
                {
                    playerData.shortestLifespan = lifespan;
                }

                if (lifespan > playerData.longestLifespan)
                {
                    playerData.longestLifespan = lifespan;
                }
            }
            lifespan = 0;
            deaths++;
            isDead = true;
            updateKD();
            Dead();
            deathCoolDown = 1f;
        }
        return isDead;
    }

    // was meant to remove players if theyre dead but doesn't really work for some reason
    void Dead()
    {
        if (isPlayer)
        {
            transform.position = deathPoint.transform.position;
        }
        else
        {
            transform.position = new Vector3(transform.position.x, -5, transform.position.z);
            GetComponent<Rigidbody>().useGravity = false;
        }
    }

    // returns if the player is dead
    public bool IsDead()
    {
        return isDead;
    }

    // returns players data for outputting to CSV
    public PlayerData OutputData()
    {
        playerData.averageEC = 0;
        for (int i = 0; i < playerData.engagedCounter.Count; i++)
        {
            playerData.averageEC += playerData.engagedCounter[i];
        }

        if (playerData.engagedCounter.Count > 0)
        {
            playerData.averageEC /= playerData.engagedCounter.Count;
        }


        if (deaths == 0)
        {
            playerData.currentKD = kills / 1;
        }
        else
        {
            playerData.currentKD = kills / deaths;
            playerData.averageLifespan /= deaths;
        }

        return playerData;
    }
}
