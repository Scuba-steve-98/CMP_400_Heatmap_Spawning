using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class HeatmapSetup : MonoBehaviour
{
    //declaring variables
    int width, bredth, layerMask, scale, activeTiles;

    float widthTemp, bredthTemp, distance, colliderRadius, threatLevel, team1Threat, team2Threat, team1Friendly, team2Friendly;

    Vector3 planeSize, bottomLeft, rayStart, defaultVec;

    Tiles[] tiles;
    PossibleSpawns[] possibleFFASpawnAreas;
    PossibleSpawns[] possibleP1SpawnAreas;
    PossibleSpawns[] possibleP2SpawnAreas;

    GameManager gameManager_;

    [SerializeField, Range(0, 1)]
    int team = 0;

    [SerializeField, Range(0, 75)]
    float targetThreatValue = 32;

    // Start is called before the first frame update
    void Start()
    {
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
        possibleFFASpawnAreas = new PossibleSpawns[15];
        possibleP1SpawnAreas = new PossibleSpawns[15];
        possibleP2SpawnAreas = new PossibleSpawns[15];

        for (int i = 0; i < 15; i++)
        {
            possibleFFASpawnAreas[i] = new PossibleSpawns();
            possibleP1SpawnAreas[i] = new PossibleSpawns();
            possibleP2SpawnAreas[i] = new PossibleSpawns();
        }

            // call function to activate tiles
            setHeatmapUp();
    }

    // Update is called once per frame
    void Update()
    {
        resetArrays();

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
                            team2Friendly += Mathf.Lerp(friendly, 0, distance / (colliderRadius / 2f)) / 2;
                        }
                        else if (hit[c].collider.GetComponentInParent<Player>().getTeam() == 1)
                        {
                            float threat = hit[c].collider.GetComponentInParent<Player>().getThreatLevel();
                            float friendly = hit[c].collider.GetComponentInParent<Player>().getFriendLevel();

                            colliderRadius = hit[c].collider.transform.localScale.x;
                            distance = Vector3.Distance(rayStart, hit[c].transform.position);

                            team2Threat += Mathf.Lerp(threat, 0, distance / (colliderRadius / 2f));
                            team1Friendly += Mathf.Lerp(friendly, 0, distance / (colliderRadius / 2f)) / 2;
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
                    }
                }
            }

            if (gameManager_.isTDM() && Input.GetKeyDown(KeyCode.Space))
            {
                tiles[i].setValues(team1Threat, team2Threat, team1Friendly, team2Friendly);
                float close = Mathf.Abs(team1Threat - targetThreatValue);
                if (close < possibleP1SpawnAreas[14].getCloseness())
                {
                    possibleP1SpawnAreas[14].setCloseness(close);
                    possibleP1SpawnAreas[14].setLocation(tiles[i].getLocation());
                    Array.Sort<PossibleSpawns>(possibleP1SpawnAreas, delegate (PossibleSpawns x, PossibleSpawns y) { return x.getCloseness().CompareTo(y.getCloseness()); });
                }

                close = Mathf.Abs(team2Threat - targetThreatValue);
                if (close < possibleP2SpawnAreas[14].getCloseness())
                {
                    possibleP2SpawnAreas[14].setCloseness(close);
                    possibleP2SpawnAreas[14].setLocation(tiles[i].getLocation());
                    Array.Sort<PossibleSpawns>(possibleP2SpawnAreas, delegate (PossibleSpawns x, PossibleSpawns y) { return x.getCloseness().CompareTo(y.getCloseness()); });
                }
            }
            else
            {
                // stores the threat level for the tile after it has been calculated
                tiles[i].setValues(threatLevel);
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (gameManager_.getSpawnType() == SPAWN_TYPE.RULE_BASED)
            {
                if (gameManager_.isTDM())
                {
                    RBSpawningSelector rbss = FindObjectOfType<RBSpawningSelector>();
                    rbss.chooseTDMSpawnLocation(team);
                }
            }
        }
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
        tiles = new Tiles[activeTiles];

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
                    tiles[activeTiles] = new Tiles(rayStart);
                    activeTiles++;
                }
            }
        }
    }

    void resetArrays()
    {
        for (int i = 0; i < 15; i++)
        {
            possibleFFASpawnAreas[i].setCloseness(150);
            possibleP1SpawnAreas[i].setCloseness(150);
            possibleP2SpawnAreas[i].setCloseness(150);
        }
    }

    public PossibleSpawns[] getFFATiles()
    {
        return possibleFFASpawnAreas;
    }

    public PossibleSpawns[] getTDMTiles(int team)
    {
        if (team == 0)
        {
            return possibleP1SpawnAreas;
        }
        else 
        {
            return possibleP2SpawnAreas;
        }
    }

    private void OnDrawGizmos()
    {
        // draws the heatmap (only going to be used for demos)
        if (tiles != null)
        {
            foreach (Tiles tile in tiles)
            {
                {
                    if (gameManager_.isTDM())
                    {
                        // sets cube colour based on its threat value
                        Gizmos.color = Color.Lerp(Color.blue, Color.red, (tile.getTeamThreatLevel(team) / 100f));
                        Gizmos.DrawCube(tile.getLocation(), defaultVec);
                    }
                    else
                    {
                        // sets cube colour based on its threat value
                        Gizmos.color = Color.Lerp(Color.blue, Color.red, (tile.getThreatLevel() / 100f));
                        Gizmos.DrawCube(tile.getLocation(), defaultVec);
                    }
                }
            }
        }
    }
}