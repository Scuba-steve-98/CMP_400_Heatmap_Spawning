using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class RBSpawningSelector : MonoBehaviour
{
    GameManager gameManager_;
    PossibleSpawns[] demo;
    Player[] players_;
    Player[] team1_;
    Player[] team2_;
    Player[] enemy_;
    Player[] friendly_;

    Vector3 defaultVec;

    int numberOfTiles = 45;

    int layerMask = 0;

    // Start is called before the first frame update
    void Start()
    {
        defaultVec = new Vector3(1, 0.75f, 1) * 2;

        layerMask |= 1 << 9;
        layerMask |= 1 << 10;
        layerMask = ~layerMask;
    }

    public void init()
    {
        players_ = FindObjectsOfType<Player>();
        gameManager_ = FindObjectOfType<GameManager>();
        if (gameManager_.isTDM())
        {
            int team1Counter = 0;
            int team2counter = 0;
            foreach (Player p in players_)
            {
                if (p.getTeam() == 0)
                {
                    team1Counter++;
                }
                else
                {
                    team2counter++;
                }
            }

            team1_ = new Player[team1Counter];
            team2_ = new Player[team2counter];
            team1Counter = 0;
            team2counter = 0;

            foreach (Player p in players_)
            {
                if (p.getTeam() == 0)
                {
                    team1_[team1Counter] = p;
                    team1Counter++;
                }
                else
                {
                    team2_[team2counter] = p;
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


    public void chooseFFASpawnLocation(PossibleSpawns[] tiles_)
    {
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


    public void chooseTDMSpawnLocation(int team, PossibleSpawns[] tiles_)
    {
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
        }
        Array.Sort<PossibleSpawns>(tiles_, delegate (PossibleSpawns x, PossibleSpawns y) { return x.getClosestFriendly().CompareTo(y.getClosestFriendly()); });
        Array.Reverse(tiles_);
        Array.Sort<PossibleSpawns>(tiles_, delegate (PossibleSpawns x, PossibleSpawns y) { return x.getFriendliesSeen().CompareTo(y.getFriendliesSeen()); });
        Array.Reverse(tiles_);

        Array.Sort<PossibleSpawns>(tiles_, delegate (PossibleSpawns x, PossibleSpawns y) { return x.getClosest().CompareTo(y.getClosest()); });
        Array.Reverse(tiles_);
        Array.Sort<PossibleSpawns>(tiles_, delegate (PossibleSpawns x, PossibleSpawns y) { return x.getEnemiesSeen().CompareTo(y.getEnemiesSeen()); });

        for (int i = 0; i < tiles_.Length; i++)
        {
            Debug.Log("Close: " + tiles_[i].getClosest() + "   Enemies: " + tiles_[i].getEnemiesSeen());
        }

        if (tiles_[0].getEnemiesSeen() < tiles_[1].getEnemiesSeen())
        {
            tiles_[0].setSpawn();
            return;
        }
        else
        {
            for (int i = 0; i < tiles_.Length; i++)
            {
                if ((i+1) < tiles_.Length)
                {
                    if (tiles_[i].getClosest() > tiles_[i + 1].getClosest())
                    {
                        Debug.Log("yeet");
                        tiles_[UnityEngine.Random.Range(0, i)].setSpawn();
                        break;
                    }
                    if (tiles_[i].getFriendliesSeen() > tiles_[i + 1].getFriendliesSeen())
                    {
                        Debug.Log("meet");
                        tiles_[UnityEngine.Random.Range(0, i)].setSpawn();
                        break;
                    }
                    if (tiles_[i].getClosestFriendly() < tiles_[i + 1].getClosestFriendly())
                    {
                        Debug.Log("beat");
                        tiles_[UnityEngine.Random.Range(0, i)].setSpawn();
                        break;
                    }
                }
                else
                {
                    Debug.Log("NONE");
                    tiles_[UnityEngine.Random.Range(0, i)].setSpawn();
                }
            }
            // just for demonstating that it works and where it has chosen
            demo = tiles_;
            return;
        }
    }


    private void OnDrawGizmos()
    {
        // draws the heatmap (only going to be used for demos)
        if (demo != null)
        {
            for (int i = 0; i < demo.Length; i++)
            {
                // sets cube colour based on its threat value
                if (demo[i].getSpawn())
                {
                    Gizmos.color = Color.magenta;
                }
                else
                {
                    Gizmos.color = Color.green;
                }
                Gizmos.DrawCube(demo[i].getLocation() + Vector3.up * 0.5f, defaultVec);
            }
        }
    }
}
