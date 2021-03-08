using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class FuzzySpawnSelector : MonoBehaviour
{
    List<PossibleFuzzySpawns> tiles_;
    FuzzyHeatmapData heatmap_;
    GameManager gameManager_;
    Player[] players_;
    Player[] team1_;
    Player[] team2_;
    Player[] enemy_;
    Player[] friendly_;

    ENEMIES_SEEN enemiesSeen = (ENEMIES_SEEN)3;
    ENEMIES_SEEN friendliesSeen = (ENEMIES_SEEN)3;

    CLOSENESS closenessToEnemy = (CLOSENESS)7;
    CLOSENESS closenessToFriendly = (CLOSENESS)7;

    Vector3 defaultVec;

    int noOfPlayers;
    int numberOfTiles = 15;
    float mapWidth;

    int layerMask = 0;

    // Start is called before the first frame update
    void Start()
    {
        heatmap_ = FindObjectOfType<FuzzyHeatmapData>();
        gameManager_ = FindObjectOfType<GameManager>();
        tiles_ = new List<PossibleFuzzySpawns>();
        defaultVec = new Vector3(1, 0.75f, 1) * 2;

        layerMask |= 1 << 9;
        layerMask |= 1 << 10;
        layerMask = ~layerMask;
    }

    public void init()
    {
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
        players_ = FindObjectsOfType<Player>();
        noOfPlayers = players_.Length;
        mapWidth = heatmap_.getWidth();
        Debug.Log("Map Width: " + mapWidth);
    }

    private void Update()
    {

    }


    public void chooseFFASpawnLocation()
    {
        tiles_ = heatmap_.getFFATiles();

        for (int i = 0; i < tiles_.Count; i++)
        {
            tiles_[i].setSpawn(false);
        }

        for (int i = 0; i < tiles_.Count; i++)
        {
            int enemies = 0;
            float closest = 10000;
            foreach (Player enemy in players_)
            {
                Ray ray = new Ray(tiles_[i].getLocation(), (enemy.transform.position - tiles_[i].getLocation()));
                RaycastHit hit;
                Physics.Raycast(ray, out hit, 100, layerMask);
                if (hit.collider.gameObject == enemy.gameObject)
                {
                    enemies++;
                    float dist = Vector3.Distance(tiles_[i].getLocation(), enemy.transform.position);
                    if (dist < closest)
                    {
                        closest = dist;
                    }
                }
            }
            enemiesSeen = heatmap_.getFuzzyEnemiesSeen(enemies);
            closenessToEnemy = heatmap_.getFuzzyCloseness(closest / mapWidth);

            // change this to have farthest with no enemies seen
            //if (enemiesSeen == 0)
            //{
            //    tiles_[i].setSpawn();
            //    return;
            //}
        }

        tiles_.Sort(delegate (PossibleFuzzySpawns x, PossibleFuzzySpawns y) { return x.getClosest().CompareTo(y.getClosest()); });
        tiles_.Reverse();
        tiles_.Sort(delegate (PossibleFuzzySpawns x, PossibleFuzzySpawns y) { return x.getEnemiesSeen().CompareTo(y.getEnemiesSeen()); });

        if (tiles_[0].getEnemiesSeen() < tiles_[1].getEnemiesSeen())
        {
            tiles_[0].setSpawn();
            Debug.Log("OOF" + tiles_[0].getEnemiesSeen() + " " + tiles_[1].getEnemiesSeen());
            return;
        }
        else
        {
            Debug.Log(tiles_.Count);
            int x = 0;
            for (int i = 0; i < tiles_.Count; i++)
            {
                if ((i + 1) < tiles_.Count)
                {
                    if (tiles_[i].getEnemiesSeen() == tiles_[i + 1].getEnemiesSeen())
                    {
                        if (tiles_[x].getClosest() > tiles_[x + 1].getClosest())
                        {
                            tiles_[UnityEngine.Random.Range(0, i)].setSpawn();
                            return;
                        }
                    }
                    else
                    {
                        tiles_[UnityEngine.Random.Range(0, i)].setSpawn();
                        return;
                    }
                }
                else
                {
                    tiles_[UnityEngine.Random.Range(0, i)].setSpawn();
                    return;
                }
            }
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

        for (int i = 0; i < tiles_.Count; i++)
        {
            int enemies = 0;
            int friendlies = 0;
            float closest = 10000;
            float closestFriendly = 10000;
            foreach (Player enemy in enemy_)
            {
                Ray ray = new Ray(tiles_[i].getLocation(), (enemy.transform.position - tiles_[i].getLocation()));
                RaycastHit hit;
                Physics.Raycast(ray, out hit, 100, layerMask);
                if (hit.collider.gameObject == enemy.gameObject)
                {
                    enemies++;
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
            //float temp = (float)enemies / (float)enemy_.Length;
            enemiesSeen = heatmap_.getFuzzyEnemiesSeen(enemies);
            closenessToEnemy = heatmap_.getFuzzyCloseness(closest / mapWidth);

            //temp = (float)friendlies / (float)friendly_.Length;
            friendliesSeen = heatmap_.getFuzzyEnemiesSeen(friendlies);
            closenessToFriendly = heatmap_.getFuzzyCloseness(closestFriendly / mapWidth);

            tiles_[i].setEnemiesSeen(enemiesSeen);
            tiles_[i].setClosest(closenessToEnemy);
            tiles_[i].setFriendliesSeen(friendliesSeen);
            tiles_[i].setClosestFriendly(closenessToFriendly);
        }

        tiles_.Sort(delegate (PossibleFuzzySpawns x, PossibleFuzzySpawns y) { return x.getClosestFriendly().CompareTo(y.getClosestFriendly()); });
        tiles_.Reverse();
        tiles_.Sort(delegate (PossibleFuzzySpawns x, PossibleFuzzySpawns y) { return x.getFriendliesSeen().CompareTo(y.getFriendliesSeen()); });
        tiles_.Reverse();

        tiles_.Sort(delegate (PossibleFuzzySpawns x, PossibleFuzzySpawns y) { return x.getClosest().CompareTo(y.getClosest()); });
        tiles_.Reverse();
        tiles_.Sort(delegate (PossibleFuzzySpawns x, PossibleFuzzySpawns y) { return x.getEnemiesSeen().CompareTo(y.getEnemiesSeen()); });

        foreach (PossibleFuzzySpawns pfs in tiles_)
        {
            Debug.Log("Enemies Seen: " + pfs.getEnemiesSeen() + " Enemy Closeness: " + pfs.getClosest());
        }

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
            for (int i = 0; i < tiles_.Count; i++)
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
