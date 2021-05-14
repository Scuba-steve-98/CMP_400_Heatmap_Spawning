using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class FuzzyHeatmapData : MonoBehaviour
{
    // initialises fuzzy variables
    DANGER dangerLevel = (DANGER)10;
    DANGER targetLevel = (DANGER)5;
    DANGER team1Level = (DANGER)10;
    DANGER team2Level = (DANGER)10;
    DANGER friendly1Level = (DANGER)10;
    DANGER friendly2Level = (DANGER)10;

    //declaring variables
    int width, bredth, layerMask, scale;

    float distance, colliderRadius, team1Threat, team2Threat, team1Friendly, team2Friendly;

    Vector3 rayStart;

    //FuzzyTiles[] tiles;
    List<FuzzyTiles> tilesList;

    List<PossibleFuzzySpawns> possibleTeam1;
    List<PossibleFuzzySpawns> possibleTeam2;
    List<PossibleFuzzySpawns> possibleFFA;

    PossibleFuzzySpawns[] array1;
    PossibleFuzzySpawns[] array2;

    List<PossibleFuzzySpawns> oldPossibleTeam1;
    List<PossibleFuzzySpawns> oldPossibleTeam2;
    List<PossibleFuzzySpawns> oldPossibleFFA;

    GameManager gameManager_;

    Vector3 location;

    //[SerializeField, Range(0, 1)]
    //int team = 0;

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
    AnimationCurve average;

    [SerializeField]
    AnimationCurve prettyFar;

    [SerializeField]
    AnimationCurve veryFar;

    [SerializeField]
    AnimationCurve extremelyFar;

    // fuzzy curves for enemies seen
    [Header("Enemies Seen")]
    [SerializeField]
    AnimationCurve alone;

    [SerializeField]
    AnimationCurve aCouple;

    [SerializeField]
    AnimationCurve aFew;

    [SerializeField]
    AnimationCurve crowded;

    [SerializeField]
    AnimationCurve swarmed;

    // Start is called before the first frame update
    void Start()
    {
        // initialise variables
        distance = 0f;
        layerMask = 1 << 9;
        scale = 2;

        // initialise variables for the for loops
        gameManager_ = FindObjectOfType<GameManager>().GetComponent<GameManager>();

        possibleFFA = new List<PossibleFuzzySpawns>();
        possibleTeam1 = new List<PossibleFuzzySpawns>();
        possibleTeam2 = new List<PossibleFuzzySpawns>();

        array1 = new PossibleFuzzySpawns[35];
        array2 = new PossibleFuzzySpawns[35];

        for (int i = 0; i < 35; i++)
        {
            array1[i] = new PossibleFuzzySpawns();
            array2[i] = new PossibleFuzzySpawns();
        }
    }

    // creates the list of tiles
    public void addTiles(List<FuzzyTiles> ft)
    {
        if (tilesList == null)
        {
            tilesList = new List<FuzzyTiles>();
        }

        tilesList.AddRange(ft);
    }

    public Vector3 getHeatmapData(int team, float threatLevel, int kills, int deaths)
    {
        // resets tiles closeness
        resetList();

        // calculates the target level based on the game mode
        if (gameManager_.isTDM())
        {
            float currentThreatLevel = threatLevel * gameManager_.getTeamThreat(Mathf.Abs(team - 1));
            if (currentThreatLevel > 200)
            {
                currentThreatLevel = 200;
            }
            currentThreatLevel /= 200;
            targetLevel = getFuzzyDangerLevel(currentThreatLevel);
        }
        else
        {
            if (kills > deaths)
            {
                float multiplier = Mathf.Lerp(0.6f, 0.8f, gameManager_.getGameProgress());
                float currentThreatMultiplier = threatLevel / multiplier;
                if (currentThreatMultiplier >= 200)
                {
                    currentThreatMultiplier = 200;
                }
                currentThreatMultiplier /= 200;
                targetLevel = getFuzzyDangerLevel(currentThreatMultiplier);
            }
            else
            {
                float multiplier = Mathf.Lerp(0.6f, 0.8f, gameManager_.getGameProgress());
                float currentThreatMultiplier = threatLevel * multiplier;
                if (currentThreatMultiplier >= 200)
                {
                    currentThreatMultiplier = 200;
                }
                currentThreatMultiplier /= 200;
                targetLevel = getFuzzyDangerLevel(currentThreatMultiplier);
            }
        }

        // loops through for the number of active tiles
        for (int i = 0; i < tilesList.Count; i++)
        {
            // creates a ray for detecting enemies
            rayStart = tilesList[i].getLocation();
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

            // loops through for the number of hits the tile has
            for (int c = 0; c < hit.Length; c++)
            {
                if (hit[c].collider.transform != null)
                {
                    if (gameManager_.isTDM())
                    {
                        if (hit[c].collider.GetComponentInParent<Player>().getTeam() == 0)
                        {
                            // gets the data for calculating the team threat and friendly levels
                            float threat = hit[c].collider.GetComponentInParent<Player>().getThreatLevel();
                            float friendly = hit[c].collider.GetComponentInParent<Player>().getFriendLevel();
                            colliderRadius = hit[c].collider.transform.localScale.x;

                            // calculates the threat level for the tile
                            distance = Vector3.Distance(rayStart, hit[c].transform.position);
                            team1Threat += Mathf.Lerp(threat, 0, distance / (colliderRadius / 2f));
                            if (team1Threat >= 200)
                            {
                                team1Threat = 200;
                            }
                            team2Friendly += Mathf.Lerp(friendly, 0, distance / (colliderRadius / 2f)) / 2;
                            if (team2Friendly >= 90)
                            {
                                team2Friendly = 90;
                            }
                        }
                        else if (hit[c].collider.GetComponentInParent<Player>().getTeam() == 1)
                        {
                            // gets the data for calculating the team threat and friendly levels
                            float threat = hit[c].collider.GetComponentInParent<Player>().getThreatLevel();
                            float friendly = hit[c].collider.GetComponentInParent<Player>().getFriendLevel();
                            colliderRadius = hit[c].collider.transform.localScale.x;

                            // calculates the threat level for the tile
                            distance = Vector3.Distance(rayStart, hit[c].transform.position);
                            team2Threat += Mathf.Lerp(threat, 0, distance / (colliderRadius / 2f));
                            if (team2Threat >= 200)
                            {
                                team2Threat = 200;
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
                // Fuzzifies the values
                team1Level = getFuzzyDangerLevel(team1Threat / 200);
                team2Level = getFuzzyDangerLevel(team2Threat / 200);
                friendly1Level = getFuzzyDangerLevel(team1Friendly / 90);
                friendly2Level = getFuzzyDangerLevel(team2Friendly / 90);

                // sets values for the tiles
                tilesList[i].setValues(team1Level, team2Level, friendly1Level, friendly2Level);

                // checks to see if the fuzzy outputs match and adds if it does
                int close = Mathf.Abs((int)team1Level - (int)targetLevel);
                if (close == 0)
                {
                    possibleTeam1.Add(new PossibleFuzzySpawns(tilesList[i].getLocation()));
                }

                close = Mathf.Abs((int)team2Level - (int)targetLevel);
                if (close == 0)
                {
                    possibleTeam2.Add(new PossibleFuzzySpawns(tilesList[i].getLocation()));
                }
            }
            else if (!gameManager_.isTDM())
            {
                // stores the threat level for the tile after it has been calculated
                dangerLevel = getFuzzyDangerLevel(threatLevel / 200);
                tilesList[i].setValues(dangerLevel);
                int close = Mathf.Abs((int)dangerLevel - (int)targetLevel);
                if (close == 0)
                {
                    possibleFFA.Add(new PossibleFuzzySpawns(tilesList[i].getLocation()));
                }
            }
        }
        // checks to see if there is enough tiles
        if (possibleFFA.Count < 35 && !gameManager_.isTDM())
        {
            // repeats the last steps by checking values to get closest
            for (int i = 0; i < possibleFFA.Count; i++)
            {
                array1[i] = possibleFFA[i];
            }
            foreach (FuzzyTiles t in tilesList)
            {
                int close = Mathf.Abs(t.getThreatLevel() - targetLevel);
                if (close < array1[34].getCloseness())
                {
                    array1[34].setCloseness(close);
                    array1[34].setLocation(t.getLocation());
                    Array.Sort<PossibleFuzzySpawns>(array1, delegate (PossibleFuzzySpawns x, PossibleFuzzySpawns y) { return x.getCloseness().CompareTo(y.getCloseness()); });
                }
            }
            possibleFFA.Clear();
            possibleFFA.AddRange(array1);
        }

        // checks to see if there is enough tiles
        if (possibleTeam1.Count < 35 && gameManager_.isTDM())
        {
            // repeats the last steps by checking values to get closest
            for (int i = 0; i < possibleTeam1.Count; i++)
            {
                array1[i] = possibleTeam1[i];
            }
            foreach (FuzzyTiles t in tilesList)
            {
                int close = Mathf.Abs(t.getTeamThreatLevel(Mathf.Abs(0)) - targetLevel);
                if (array1[34].getCloseness() > close)
                {
                    array1[34].setCloseness(close);
                    array1[34].setLocation(t.getLocation());
                    Array.Sort<PossibleFuzzySpawns>(array1, delegate (PossibleFuzzySpawns x, PossibleFuzzySpawns y) { return x.getCloseness().CompareTo(y.getCloseness()); });
                }
            }
            possibleTeam1.Clear();
            possibleTeam1.AddRange(array1);
        }

        if (possibleTeam2.Count < 35 && gameManager_.isTDM())
        {
            // repeats the last steps by checking values to get closest
            for (int i = 0; i < possibleTeam2.Count; i++)
            {
                array2[i] = possibleTeam2[i];
            }
            foreach (FuzzyTiles t in tilesList)
            {
                int close = Mathf.Abs(t.getTeamThreatLevel(Mathf.Abs(1)) - targetLevel);
                if (array2[34].getCloseness() > close)
                {
                    array2[34].setCloseness(close);
                    array2[34].setLocation(t.getLocation());
                    Array.Sort<PossibleFuzzySpawns>(array2, delegate (PossibleFuzzySpawns x, PossibleFuzzySpawns y) { return x.getCloseness().CompareTo(y.getCloseness()); });
                }
            }
            possibleTeam2.Clear();
            possibleTeam2.AddRange(array2);
        }

        // passes tiles through to the spawn selector
        if (gameManager_.isTDM())
        {
            FuzzySpawnSelector flss = FindObjectOfType<FuzzySpawnSelector>();
            if (team == 0)
            {
                location = flss.chooseTDMSpawnLocation(team, possibleTeam1);
                location.y += 1.1f;
                return location;
            }
            else
            {
                location = flss.chooseTDMSpawnLocation(team, possibleTeam2);
                location.y += 1.1f;
                return location;
            }
        }
        else
        {
            FuzzySpawnSelector flss = FindObjectOfType<FuzzySpawnSelector>();
            location = flss.chooseFFASpawnLocation(possibleFFA);
            location.y += 1.1f;
            return location;
        }
    }

    void resetList()
    {
        for (int i = 0; i < array1.Length; i++)
        {
            array1[i].setCloseness(1000);
            array2[i].setCloseness(1000);
        }

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
        else if (prettyClose.Evaluate(x) > average.Evaluate(x))
        {
            return (CLOSENESS)2;
        }
        else if (average.Evaluate(x) > prettyFar.Evaluate(x))
        {
            return (CLOSENESS)3;
        }
        else if (prettyFar.Evaluate(x) > veryFar.Evaluate(x))
        {
            return (CLOSENESS)4;
        }
        else if (veryFar.Evaluate(x) > extremelyFar.Evaluate(x))
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
        else if (x < 1)
        {
            return (ENEMIES_SEEN)1;
        }
        else if (x < 3)
        {
            return (ENEMIES_SEEN)2;
        }
        else if (x < 5)
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
}
