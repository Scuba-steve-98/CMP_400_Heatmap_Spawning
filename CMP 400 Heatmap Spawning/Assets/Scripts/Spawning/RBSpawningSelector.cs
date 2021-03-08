using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class RBSpawningSelector : MonoBehaviour
{
    PossibleSpawns[] tiles_;
    HeatmapSetup heatmap_;
    GameManager gameManager_;
    Player[] players_;
    Player[] team1_;
    Player[] team2_;
    Player[] enemy_;
    Player[] friendly_;

    Vector3 defaultVec;

    int noOfPlayers;
    int numberOfTiles = 15;

    int layerMask = 0;

    // Start is called before the first frame update
    void Start()
    {
        heatmap_ = FindObjectOfType<HeatmapSetup>();
        gameManager_ = FindObjectOfType<GameManager>();
        tiles_ = new PossibleSpawns[numberOfTiles];
        defaultVec = new Vector3(1, 0.75f, 1) * 2;
        players_ = FindObjectsOfType<Player>();
        noOfPlayers = players_.Length;

        layerMask |= 1 << 9;
        layerMask |= 1 << 10;
        layerMask = ~layerMask;
    }

    public void init()
    {
        if (gameManager_.isTDM())
        {
            if (noOfPlayers % 2 == 0)
            {
                team1_ = new Player[noOfPlayers / 2];
                team2_ = new Player[noOfPlayers / 2];
            }
            else
            {
                team1_ = new Player[noOfPlayers / 2];
                team2_ = new Player[noOfPlayers / 2 + 1];
            }

            int team1counter = 0;
            int team2counter = 0;
            for (int i = 0; i < noOfPlayers; i++)
            {
                if (players_[i].getTeam() == 0)
                {
                    team1_[team1counter] = players_[i];
                    team1counter++;
                }
                else
                {
                    team2_[team2counter] = players_[i];
                    team2counter++;
                }
            }

            enemy_ = team2_;
            friendly_ = team1_;
        }
    }

    private void Update()
    {

    }


    public void chooseFFASpawnLocation()
    {
        tiles_ = heatmap_.getFFATiles();

        for (int i = 0; i < numberOfTiles; i++)
        {
            tiles_[i].setSpawn(false);
        }

        for (int i = 0; i < numberOfTiles; i++)
        {
            int enemiesSeen = 0;
            float closest = 10000;

            foreach (Player enemy in players_)
            {
                Ray ray = new Ray(tiles_[i].getLocation(), (enemy.transform.position - tiles_[i].getLocation()));
                RaycastHit hit;
                Physics.Raycast(ray, out hit, 100, layerMask);
                if (hit.collider.gameObject == enemy.gameObject)
                {
                    enemiesSeen++;
                    float dist = Vector3.Distance(tiles_[i].getLocation(), enemy.transform.position);
                    if (dist < closest)
                    {
                        closest = dist;
                    }
                }
            }
            Debug.Log(enemiesSeen);
            tiles_[i].setEnemiesSeen(enemiesSeen);
            tiles_[i].setClosest(closest);

            // change this to have farthest with no enemies seen
            if (enemiesSeen == 0)
            {
                tiles_[i].setSpawn();
                return;
            }
        }

        Array.Sort<PossibleSpawns>(tiles_, delegate (PossibleSpawns x, PossibleSpawns y) { return x.getClosest().CompareTo(y.getClosest()); });
        Array.Reverse(tiles_);
        Array.Sort<PossibleSpawns>(tiles_, delegate (PossibleSpawns x, PossibleSpawns y) { return x.getEnemiesSeen().CompareTo(y.getEnemiesSeen()); });

        if (tiles_[0].getEnemiesSeen() < tiles_[1].getEnemiesSeen())
        {
            tiles_[0].setSpawn();
            return;
        }
        else
        {
            int x = 0;
            while (tiles_[x].getEnemiesSeen() == tiles_[x + 1].getEnemiesSeen())
            {
                if (tiles_[x].getClosest() > tiles_[x + 1].getClosest())
                {
                    Debug.Log("yeet");
                    break;
                }
                Debug.Log("none yet");
                x++;
            }
            tiles_[UnityEngine.Random.Range(0, x)].setSpawn();
            Debug.Log(x);
            return;
        }
    }


    public void chooseTDMSpawnLocation(int team)
    {
        tiles_ = heatmap_.getTDMTiles(team);

        for (int i = 0; i < numberOfTiles; i++)
        {
            tiles_[i].setSpawn(false);
        }

        if (team == 0)
        {
            enemy_ = team1_;
            friendly_ = team2_;
        }
        else
        {
            enemy_ = team2_;
            friendly_ = team1_;
        }

        for (int i = 0; i < numberOfTiles; i++)
        {
            int enemiesSeen = 0;
            int friendliesSeen = 0;
            float closest = 10000;
            float closestFriendly = 10000;
            foreach (Player enemy in enemy_)
            {
                Ray ray = new Ray(tiles_[i].getLocation(), (enemy.transform.position - tiles_[i].getLocation()));
                RaycastHit hit;
                Physics.Raycast(ray, out hit, 100, layerMask);
                if (hit.collider.gameObject == enemy.gameObject)
                {
                    enemiesSeen++;
                    float dist = Vector3.Distance(tiles_[i].getLocation(), enemy.transform.position);
                    if (dist < closest)
                    {
                        closest = dist;
                    }
                }
            }
            foreach (Player friendly in friendly_)
            {
                Ray ray = new Ray(tiles_[i].getLocation(), (friendly.transform.position - tiles_[i].getLocation()));
                RaycastHit hit;
                Physics.Raycast(ray, out hit, 100, layerMask);
                if (hit.collider.gameObject == friendly.gameObject)
                {
                    friendliesSeen++;
                    float dist = Vector3.Distance(tiles_[i].getLocation(), friendly.transform.position);
                    if (dist < closestFriendly)
                    {
                        closestFriendly = dist;
                    }
                }
            }
            tiles_[i].setEnemiesSeen(enemiesSeen);
            tiles_[i].setClosest(closest);
            tiles_[i].setFriendliesSeen(friendliesSeen);
            tiles_[i].setClosestFriendly(closestFriendly);

            // change this to have farthest with no enemies seen
            //if (enemiesSeen == 0)
            //{
            //    tiles_[i].setSpawn();
            //    Debug.Log("Found");
            //    return;
            //}
        }
        Array.Sort<PossibleSpawns>(tiles_, delegate (PossibleSpawns x, PossibleSpawns y) { return x.getClosestFriendly().CompareTo(y.getClosestFriendly()); });
        Array.Reverse(tiles_);
        Array.Sort<PossibleSpawns>(tiles_, delegate (PossibleSpawns x, PossibleSpawns y) { return x.getFriendliesSeen().CompareTo(y.getFriendliesSeen()); });
        Array.Reverse(tiles_);

        Array.Sort<PossibleSpawns>(tiles_, delegate (PossibleSpawns x, PossibleSpawns y) { return x.getClosest().CompareTo(y.getClosest()); });
        Array.Reverse(tiles_);
        Array.Sort<PossibleSpawns>(tiles_, delegate (PossibleSpawns x, PossibleSpawns y) { return x.getEnemiesSeen().CompareTo(y.getEnemiesSeen()); });

        if (tiles_[0].getEnemiesSeen() < tiles_[1].getEnemiesSeen())
        {
            tiles_[0].setSpawn();
            return;
        }
        else
        {
            int x = 0;
            while (tiles_[x].getEnemiesSeen() == tiles_[x + 1].getEnemiesSeen())
            {
                if (tiles_[x].getClosest() > tiles_[x + 1].getClosest())
                {
                    Debug.Log("yeet");
                    break;
                }
                Debug.Log("a");
                if (tiles_[x].getFriendliesSeen() > tiles_[x + 1].getFriendliesSeen())
                {
                    Debug.Log("meet");
                    break;
                }
                Debug.Log("b");
                if (tiles_[x].getClosestFriendly() < tiles_[x + 1].getClosestFriendly())
                {
                    Debug.Log("beat");
                    break;
                }
                Debug.Log("none yet");
                x++;
            }
            tiles_[UnityEngine.Random.Range(0, x)].setSpawn();
            Debug.Log(x);
            return;
        }
    }


    private void OnDrawGizmos()
    {
        // draws the heatmap (only going to be used for demos)
        if (tiles_ != null)
        {
            //int i = 0;
            for (int i = 0; i < numberOfTiles; i++)
            //foreach (PossibleSpawns tile in tiles_)
            {
                //if (tiles_[i] != null)
                {
                    // sets cube colour based on its threat value
                    if (tiles_[i].getSpawn())
                    {
                        Gizmos.color = Color.magenta;
                    }
                    else
                    {
                        Gizmos.color = Color.green;
                    }


                    Gizmos.DrawCube(tiles_[i].getLocation(), defaultVec);
                }
            }
        }
    }
}
