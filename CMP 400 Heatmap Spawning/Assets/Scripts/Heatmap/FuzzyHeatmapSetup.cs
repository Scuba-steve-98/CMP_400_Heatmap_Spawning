using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class FuzzyHeatmapSetup : MonoBehaviour
{
    DANGER dangerLevel = (DANGER)10;
    DANGER targetLevel = (DANGER)5;
    DANGER team1Level = (DANGER)10;
    DANGER team2Level = (DANGER)10;
    DANGER friendly1Level = (DANGER)10;
    DANGER friendly2Level = (DANGER)10;

    FUZZY_OUTPUT fOutput = (FUZZY_OUTPUT)6;
    CLOSENESS closeness = (CLOSENESS)6;
    CLOSENESS friendlyCloseness = (CLOSENESS)6;
    ENEMIES_SEEN enemiesSeen = (ENEMIES_SEEN)3;
    ENEMIES_SEEN friendliesSeen = (ENEMIES_SEEN)3;

    //declaring variables
    int width, bredth, layerMask, scale, activeTiles;
    int numberOfTiles = 15;

    float widthTemp, bredthTemp, distance, colliderRadius, threatLevel, team1Threat, team2Threat, team1Friendly, team2Friendly;

    Vector3 planeSize, bottomLeft, rayStart, defaultVec;

    FuzzyTiles[] tiles;

    List<PossibleFuzzySpawns> possibleTeam1;
    List<PossibleFuzzySpawns> possibleTeam2;
    List<PossibleFuzzySpawns> possibleFFA;

    GameManager gameManager_;

    [SerializeField, Range(0, 1)]
    int team = 0;

    [SerializeField, Range(0, 75)]
    float targetThreatValue = 32;

    // fuzzy curves for threat levels
    [Header("Danger")]
    [SerializeField]
    AnimationCurve certainDeath;

    [SerializeField]
    AnimationCurve seriouslyDangerous;

    [SerializeField]
    AnimationCurve veryDangerous;

    [SerializeField]
    AnimationCurve prettyDangerous;

    [SerializeField]
    AnimationCurve littleDangerous;

    [SerializeField]
    AnimationCurve neutral;

    [SerializeField]
    AnimationCurve littleSafe;

    [SerializeField]
    AnimationCurve prettySafe;

    [SerializeField]
    AnimationCurve verySafe;

    [SerializeField]
    AnimationCurve seriouslySafe;

    [SerializeField]
    AnimationCurve safest;

    // fuzzy curves for closeness
    [Header("Closeness")]
    [SerializeField]
    AnimationCurve pointBlank;

    [SerializeField]
    AnimationCurve veryClose;

    [SerializeField]
    AnimationCurve prettyClose;

    [SerializeField]
    AnimationCurve sociallyDistanced;

    [SerializeField]
    AnimationCurve prettyFar;

    [SerializeField]
    AnimationCurve veryFar;

    [SerializeField]
    AnimationCurve heGone;

    // fuzzy curves for enemies seen
    [Header("Enemies Seen")]
    [SerializeField]
    AnimationCurve alone;

    [SerializeField]
    AnimationCurve aCouple;

    [SerializeField]
    AnimationCurve aFew;

    [SerializeField]
    AnimationCurve crouded;

    [SerializeField]
    AnimationCurve breakingRonaLaws;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(neutral);
        // initialise variables
        distance = 0f;
        layerMask = 1 << 9;
        scale = 2;
        activeTiles = 0;
        threatLevel = 0;

        // just a vector for drawing the gizmos cubes for the demo
        defaultVec = new Vector3(1, 0.5f, 1) * scale;

        // initialise variables for the for loops
        planeSize = GetComponent<MeshRenderer>().bounds.size;
        width = (int)planeSize.x / scale;
        bredth = (int)planeSize.z / scale;
        bottomLeft = gameObject.transform.position - (planeSize / 2);
        gameManager_ = FindObjectOfType<GameManager>().GetComponent<GameManager>();

        ///// creates an array of tiles for possible spawns
        //possibleFFASpawnAreas = new PossibleFuzzySpawns[numberOfTiles];
        //possibleP1SpawnAreas = new PossibleFuzzySpawns[numberOfTiles];
        //possibleP2SpawnAreas = new PossibleFuzzySpawns[numberOfTiles];

        possibleFFA = new List<PossibleFuzzySpawns>();
        possibleTeam1 = new List<PossibleFuzzySpawns>();
        possibleTeam2 = new List<PossibleFuzzySpawns>();

        // call function to activate tiles
        setHeatmapUp();
    }


    private void setHeatmapUp()
    {
        // layer mask to only collide with objects on specific layer
        LayerMask tempLayerMask = 1 << 10;

        // loop through to find number of tiles not under scenery
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < bredth; j++)
            {
                // gets location of each of the tiles
                widthTemp = ((float)i + 0.5f) * scale;
                bredthTemp = ((float)j + 0.5f) * scale;
                rayStart = bottomLeft + Vector3.right * widthTemp + Vector3.forward * bredthTemp - Vector3.up * 0.1f;

                // creates the ray that will check if it collides with scenery
                Ray ray = new Ray(rayStart, Vector3.up);
                if (!Physics.Raycast(ray, 5, tempLayerMask))
                {
                    // increases the value if it collides with an object on the scenery layer
                    activeTiles++;
                }
            }
        }
        // creates an array with an index equal to the number of active tiles
        tiles = new FuzzyTiles[activeTiles];

        // resets to use it as an index counter
        activeTiles = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < bredth; j++)
            {
                // gets location of each of the tiles
                widthTemp = ((float)i + 0.5f) * scale;
                bredthTemp = ((float)j + 0.5f) * scale;
                rayStart = bottomLeft + Vector3.right * widthTemp + Vector3.forward * bredthTemp - Vector3.up * 0.1f;

                // creates the ray that will check if it collides with scenery
                Ray ray = new Ray(rayStart, Vector3.up);
                if (!Physics.Raycast(ray, 5, tempLayerMask))
                {
                    // initialises the array if it doesn't collide with an object on the scenery layer
                    tiles[activeTiles] = new FuzzyTiles(rayStart);
                    activeTiles++;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            getHeatmapData();
        }
    }

    void getHeatmapData()
    {
        // resets tiles closeness
        resetList();

        // loops through for the number of active tiles
        for (int i = 0; i < activeTiles; i++)
        {
            // creates a ray for detecting enemies
            rayStart = tiles[i].getLocation();
            Ray ray = new Ray(rayStart, Vector3.up);

            // stores all of the hits that the rays collide with
            RaycastHit[] hit;
            hit = Physics.RaycastAll(ray, 5, layerMask);

            // resets the threat levels
            threatLevel = 0;
            team1Threat = 0;
            team1Friendly = 0;
            team2Threat = 0;
            team2Friendly = 0;

            dangerLevel = (DANGER)10;
            targetLevel = (DANGER)8;
            team1Level = (DANGER)10;
            team2Level = (DANGER)10;
            friendly1Level = (DANGER)10;
            friendly2Level = (DANGER)10;

            // loops through for the number of hits the tile has
            for (int c = 0; c < hit.Length; c++)
            {
                if (hit[c].collider.transform != null)
                {
                    if (gameManager_.isTDM())
                    {
                        if (hit[c].collider.GetComponentInParent<Player>().getTeam() == 0)
                        {
                            float threat = hit[c].collider.GetComponentInParent<Player>().getThreatLevel();
                            float friendly = hit[c].collider.GetComponentInParent<Player>().getFriendLevel();

                            colliderRadius = hit[c].collider.transform.localScale.x;
                            distance = Vector3.Distance(rayStart, hit[c].transform.position);

                            team1Threat += Mathf.Lerp(threat, 0, distance / (colliderRadius / 2f));
                            if (team1Threat >= 150)
                            {
                                team1Threat = 150;
                            }
                            team2Friendly += Mathf.Lerp(friendly, 0, distance / (colliderRadius / 2f)) / 2;
                            if (team2Friendly >= 90)
                            {
                                team2Friendly = 90;
                            }
                        }
                        else if (hit[c].collider.GetComponentInParent<Player>().getTeam() == 1)
                        {
                            float threat = hit[c].collider.GetComponentInParent<Player>().getThreatLevel();
                            float friendly = hit[c].collider.GetComponentInParent<Player>().getFriendLevel();

                            colliderRadius = hit[c].collider.transform.localScale.x;
                            distance = Vector3.Distance(rayStart, hit[c].transform.position);

                            team2Threat += Mathf.Lerp(threat, 0, distance / (colliderRadius / 2f));
                            if (team2Threat >= 150)
                            {
                                team2Threat = 150;
                            }
                            team1Friendly += Mathf.Lerp(friendly, 0, distance / (colliderRadius / 2f)) / 2;
                            if (team1Friendly >= 90)
                            {
                                team1Friendly = 90;
                            }
                        }
                    }
                    else
                    {
                        // gets the data for calculating the threat level
                        float threat = hit[c].collider.GetComponentInParent<Player>().getThreatLevel();
                        colliderRadius = hit[c].collider.transform.localScale.x;

                        // calculates the threat level for the tile
                        distance = Vector3.Distance(rayStart, hit[c].transform.position);
                        threatLevel += Mathf.Lerp(threat, 0, distance / (colliderRadius / 2f));
                        if (threatLevel >= 200)
                        {
                            threatLevel = 200;
                        }
                    }
                }
            }

            if (gameManager_.isTDM())
            {
                team1Level = getFuzzyDangerLevel(team1Threat / 150);
                team2Level = getFuzzyDangerLevel(team2Threat / 150);
                friendly1Level = getFuzzyDangerLevel(team1Friendly / 90);
                friendly2Level = getFuzzyDangerLevel(team2Friendly / 90);
                //Debug.Log(team1Level + " " + targetLevel);

                tiles[i].setValues(team1Level, team2Level, friendly1Level, friendly2Level);
                int close = Mathf.Abs((int)team1Level - (int)targetLevel);
                if (close == 0)
                {
                    possibleTeam1.Add(new PossibleFuzzySpawns(tiles[i].getLocation()));
                }

                close = Mathf.Abs((int)team2Level - (int)targetLevel);
                if (close == 0)
                {
                    possibleTeam2.Add(new PossibleFuzzySpawns(tiles[i].getLocation()));
                }
            }
            else if (!gameManager_.isTDM())
            {
                // stores the threat level for the tile after it has been calculated
                dangerLevel = getFuzzyDangerLevel(threatLevel / 200);
                tiles[i].setValues(dangerLevel);
                int close = Mathf.Abs((int)dangerLevel - (int)targetLevel);
                if (close == 0)
                {
                    possibleFFA.Add(new PossibleFuzzySpawns(tiles[i].getLocation()));
                }
            }
        }
        if (gameManager_.getSpawnType() == SPAWN_TYPE.FUZZY)
        {
            if (gameManager_.isTDM())
            {
                Debug.Log("No. of spawns in list" + possibleTeam1.Count + " " + possibleTeam2.Count);

                //for (int i = 0; i < possibleTeam1.Count; i++)
                //{
                //    Debug.Log("Possible Team 1 Spawn Location " + i + ": " + possibleTeam1[i].getLocation());
                //}
                //for (int i = 0; i < possibleTeam2.Count; i++)
                //{
                //    Debug.Log("Possible Team 2 Spawn Location " + i + ": " + possibleTeam2[i].getLocation());
                //}

                FuzzySpawnSelector flss = FindObjectOfType<FuzzySpawnSelector>();
                flss.chooseTDMSpawnLocation(team);
            }
            else
            {
                FuzzySpawnSelector flss = FindObjectOfType<FuzzySpawnSelector>();
                flss.chooseFFASpawnLocation();
            }
        }
    }

    void resetList()
    {
        possibleFFA.Clear();
        possibleTeam1.Clear();
        possibleTeam2.Clear();
    }

    DANGER getFuzzyDangerLevel(float x)
    {
        if (certainDeath.Evaluate(x) > seriouslyDangerous.Evaluate(x))
        {
            return 0;
        }
        else if (seriouslyDangerous.Evaluate(x) > veryDangerous.Evaluate(x))
        {
            return (DANGER)1;
        }
        else if (veryDangerous.Evaluate(x) > prettyDangerous.Evaluate(x))
        {
            return (DANGER)2;
        }
        else if (prettyDangerous.Evaluate(x) > littleDangerous.Evaluate(x))
        {
            return (DANGER)3;
        }
        else if (littleDangerous.Evaluate(x) > neutral.Evaluate(x))
        {
            return (DANGER)4;
        }
        else if (neutral.Evaluate(x) > littleSafe.Evaluate(x))
        {
            return (DANGER)5;
        }
        else if (littleSafe.Evaluate(x) > prettySafe.Evaluate(x))
        {
            return (DANGER)6;
        }
        else if (prettySafe.Evaluate(x) > verySafe.Evaluate(x))
        {
            return (DANGER)7;
        }
        else if (verySafe.Evaluate(x) > seriouslySafe.Evaluate(x))
        {
            return (DANGER)8;
        }
        else if (seriouslySafe.Evaluate(x) > safest.Evaluate(x))
        {
            return (DANGER)9;
        }
        else
        {
            return (DANGER)10;
        }
    }

    public CLOSENESS getFuzzyCloseness(float x)
    {
        if (pointBlank.Evaluate(x) > veryClose.Evaluate(x))
        {
            return (CLOSENESS)0;
        }
        else if (veryClose.Evaluate(x) > prettyClose.Evaluate(x))
        {
            return (CLOSENESS)1;
        }
        else if (prettyClose.Evaluate(x) > sociallyDistanced.Evaluate(x))
        {
            return (CLOSENESS)2;
        }
        else if (sociallyDistanced.Evaluate(x) > prettyFar.Evaluate(x))
        {
            return (CLOSENESS)3;
        }
        else if (prettyFar.Evaluate(x) > veryFar.Evaluate(x))
        {
            return (CLOSENESS)4;
        }
        else if (veryFar.Evaluate(x) > heGone.Evaluate(x))
        {
            return (CLOSENESS)5;
        }
        else
        {
            return (CLOSENESS)6;
        }
    }

    public ENEMIES_SEEN getFuzzyEnemiesSeen(int x)
    {
        if (x == 0)
        {
            return (ENEMIES_SEEN)0;
        }
        else if (x < 3)
        {
            return (ENEMIES_SEEN)1;
        }
        else if (x < 5)
        {
            return (ENEMIES_SEEN)2;
        }
        else if (x < 7)
        {
            return (ENEMIES_SEEN)3;
        }
        else
        {
            return (ENEMIES_SEEN)4;
        }
    }

    public float getWidth()
    {
        if (width < bredth)
            return width * scale;
        else
            return bredth * scale;
    }

    public void setTargetDangerLevel(float x)
    {
        targetLevel = getFuzzyDangerLevel(x);
    }

    public List<PossibleFuzzySpawns> getFFATiles()
    {
        return possibleFFA;
    }

    public List<PossibleFuzzySpawns> getTDMTiles(int team)
    {
        if (team == 0)
        {
            return possibleTeam1;
        }
        else
        {
            return possibleTeam2;
        }
    }

    private void OnDrawGizmos()
    {
        // draws the heatmap (only going to be used for demos)
        if (tiles != null)
        {
            int i = 0;
            foreach (FuzzyTiles tile in tiles)
            {
                {
                    if (gameManager_.isTDM())
                    {
                        // sets cube colour based on its threat value
                        //Gizmos.color = Color.Lerp(Color.blue, Color.red, (tile.getTeamThreatLevel(team) / 100f));

                        Gizmos.DrawCube(tile.getLocation(), defaultVec);
                    }
                    else
                    {
                        // sets cube colour based on its threat value
                        //Gizmos.color = Color.Lerp(Color.blue, Color.red, (tile.getThreatLevel() / 100f));
                        Gizmos.DrawCube(tile.getLocation(), defaultVec);
                    }
                }
                i++;
            }
            //foreach (PossibleFuzzySpawns pfs in possibleTeam1)
            //{
            //    Gizmos.color = Color.green;
            //    Gizmos.DrawCube(pfs.getLocation(), defaultVec);
            //}
            //foreach (PossibleFuzzySpawns pfs in possibleTeam2)
            //{
            //    Gizmos.color = Color.blue;
            //    Gizmos.DrawCube(pfs.getLocation(), defaultVec);
            //}
        }
    }
}