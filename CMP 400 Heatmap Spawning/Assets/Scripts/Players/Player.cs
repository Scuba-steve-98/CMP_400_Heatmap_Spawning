using System.Collections;
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

    //protected Animator Animator;
    protected float Cooldown;
    protected AudioSource AudioSourcePlayer;

    [HideInInspector]
    public bool Debug = false;
    //---------------------------------------------
    //---------------------------------------------
    //---------------------------------------------

    float weaponDamage;

    public struct PlayerData
    {
        public List<float> engagedCounter;
        public float averageEC;
        public float currentKD;
    }

    [HideInInspector]
    public PlayerData pd;

    GameManager gameManager_;
    HeatmapData heatmapData;
    FuzzyHeatmapData fuzzyHeatmapData;
    HaloCESpawnSelector haloCESpawnSelector;

    Vector3 spawnLocation;
    Renderer rend;


    SPAWN_TYPE spawnType;

    [SerializeField, Range(0, 10)]
    float overallKD = 1;

    [SerializeField, Range(60, 100)]
    float baseThreat = 67;

    float health, threatLevel, friendlyLevel, deathCoolDown, currentEC;

    int kills, deaths;

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
        //Animator = GetComponent<Animator>();
        AudioSourcePlayer = GetComponent<AudioSource>();
        AKBack.SetActive(false);
        AKHand.SetActive(true);
        PistolBack.SetActive(true);
        PistolHand.SetActive(false);
        // --------------------------------------------------


        gameManager_ = FindObjectOfType<GameManager>().GetComponent<GameManager>();
        threatLevel = baseThreat;
        friendlyLevel = baseThreat / 2;
        rend = GetComponent<Renderer>();

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

        layerMask |= 1 << 9;
        layerMask |= 1 << 10;
        layerMask = ~layerMask;
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
        if (deaths != 0 && !hasBeenEngaged)
        {
            currentEC += Time.deltaTime;
        }

        if (isDead && deathCoolDown > 0)
        {
            deathCoolDown -= Time.deltaTime;
        }
        else if (isDead && deathCoolDown <= 0)
        {
            switch(gameManager_.getSpawnType())
            {
                case SPAWN_TYPE.RULE_BASED:
                    spawnLocation = heatmapData.getHeatmapData(team);
                    transform.position = spawnLocation;
                    break;

                case SPAWN_TYPE.FUZZY:
                    spawnLocation = fuzzyHeatmapData.getHeatmapData(team);
                    break;

                case SPAWN_TYPE.HALO:
                    if (gameManager_.isTDM())
                    {
                        haloCESpawnSelector.findTDMSpawn(team);
                    }
                    else
                    {
                        haloCESpawnSelector.findFFASpawn();
                    }
                    break;
                default:
                    break;
            }
        }


        // rest of code for this function is from the tutorial
        Cooldown -= Time.deltaTime;

        if (Input.Shoot || Debug)
        {
            if (Cooldown <= 0 || Debug)
            {
                var shootVariation = UnityEngine.Random.insideUnitSphere;

                //Animator.SetTrigger("Shoot");
                //if (Animator.GetBool("AK") == true)
                {
                    AudioSourcePlayer.PlayOneShot(AudioClipAK);
                    Cooldown = 0.2f;
                    shootVariation *= 0.02f;
                }

                var shootOrigin = transform.position + Vector3.up * 1.5f;
                var shootDirection = (Input.ShootTarget - shootOrigin).normalized;
                var shootRay = new Ray(shootOrigin, shootDirection + shootVariation);


                //do we hit anybody?
                var hitInfo = new RaycastHit();
                gameObject.layer = Physics.IgnoreRaycastLayer;
                if (Physics.SphereCast(shootRay, 0.1f, out hitInfo, layerMask))
                {
                    UnityEngine.Debug.DrawLine(shootRay.origin, hitInfo.point, Color.red);

                    var player = hitInfo.collider.GetComponent<Player>();
                    if (player != null && !Debug && player != this)
                    {
                        // My change to the tutorial code
                        if (player.isShot(weaponDamage))
                        {
                            kills++;
                            updateKD();
                            if (!hasBeenEngaged)
                            {
                                hasBeenEngaged = true;
                                currentEC += Time.deltaTime;
                                pd.engagedCounter.Add(currentEC);
                                currentEC = 0;
                            }
                        }
                        //--------------------------------
                    }
                }
                gameObject.layer = 0;


            }
        }


        //var charVelo = Quaternion.Inverse(transform.rotation) * Rigidbody.velocity;
        //Animator.SetFloat("SpeedForward", charVelo.z);
        //Animator.SetFloat("SpeedSideward", charVelo.x * Mathf.Sign(charVelo.z + 0.1f));

        if (Input.SwitchToAK)
        {
            Input.SwitchToAK = false;
            AKBack.SetActive(false);
            AKHand.SetActive(true);
            //Animator.SetBool("AK", true);
            //Animator.SetBool("Pistol", false);
        }

        if (Input.SwitchToPistol)
        {
            Input.SwitchToPistol = false;
            PistolBack.SetActive(false);
            PistolHand.SetActive(true);
            //Animator.SetBool("AK", false);
            //Animator.SetBool("Pistol", true);
        }
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
            pd.currentKD = kills;
        }
        else if (deaths > 0)
        {
            threatLevel = (kills / deaths) * overallKD;
            friendlyLevel = baseThreat / threatLevel;
            threatLevel *= baseThreat;
            pd.currentKD = kills / deaths;
        }
    }
    

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
        if (!hasBeenEngaged)
        {
            hasBeenEngaged = true;
            currentEC += Time.deltaTime;
            pd.engagedCounter.Add(currentEC);
            currentEC = 0;
        }

        if (health <= 0)
        {
            deaths++;
            isDead = true;
            gameObject.SetActive(false);
            updateKD();
        }
        return isDead;
    }

    public PlayerData OutputData()
    {
        pd.averageEC = 0;
        for (int i = 0; i < pd.engagedCounter.Count; i++)
        {
            pd.averageEC += pd.engagedCounter[i];
        }
        pd.averageEC /= pd.engagedCounter.Count;

        return pd;
    }
}
