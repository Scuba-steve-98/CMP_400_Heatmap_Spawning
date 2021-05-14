using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class FuzzySpawnSelector : MonoBehaviour
{
    List<PossibleFuzzySpawns> demo;
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

    float mapWidth;

    int layerMask = 0;

    // Start is called before the first frame update
    void Start()
    {
        heatmap_ = FindObjectOfType<FuzzyHeatmapData>();
        demo = new List<PossibleFuzzySpawns>();
        defaultVec = new Vector3(1, 0.75f, 1) * 2;

        layerMask = 1 << 9;
        layerMask |= 1 << 10;
        layerMask = ~layerMask;
    }

    public void init()
    {
        gameManager_ = FindObjectOfType<GameManager>();
        players_ = FindObjectsOfType<Player>();
        if (gameManager_.isTDM())
        {
            // initialises variables
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

            // sets team arrays
            team1_ = new Player[team1Counter];
            team2_ = new Player[team2counter];
            team1Counter = 0;
            team2counter = 0;

            // sets what players on what team
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
        mapWidth = heatmap_.getWidth();
    }


    public Vector3 chooseFFASpawnLocation(List<PossibleFuzzySpawns> tiles_)
    {
        // resets list
        for (int i = 0; i < tiles_.Count; i++)
        {
            tiles_[i].setSpawn(false);
        }

        for (int i = 0; i < tiles_.Count; i++)
        {
            // resets variables
            int enemies = 0;
            float closest = 10000;

            // loops through every enemy
            foreach (Player enemy in enemy_)
            {
                // checks if the enemy is visible and stores data if seen
                Ray ray = new Ray(tiles_[i].getLocation(), (enemy.transform.position - tiles_[i].getLocation()));
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100, layerMask))
                {
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
            }
            // sets values for the tiles
            enemiesSeen = heatmap_.getFuzzyEnemiesSeen(enemies);
            closenessToEnemy = heatmap_.getFuzzyCloseness(closest / mapWidth);
            tiles_[i].setEnemiesSeen(enemiesSeen);
            tiles_[i].setClosest(closenessToEnemy);
        }

        // sorts array by importance of variables
        tiles_.Sort(delegate (PossibleFuzzySpawns x, PossibleFuzzySpawns y) { return x.getClosest().CompareTo(y.getClosest()); });
        tiles_.Reverse();
        tiles_.Sort(delegate (PossibleFuzzySpawns x, PossibleFuzzySpawns y) { return x.getEnemiesSeen().CompareTo(y.getEnemiesSeen()); });


        // if the first array member is better than the second it outputs the first
        if (tiles_[0].getEnemiesSeen() < tiles_[1].getEnemiesSeen())
        {
            tiles_[0].setSpawn();
            demo = tiles_;
            return tiles_[0].getLocation();
        }
        else
        {
            // checks to see if the current member is better than the next
            // other wise randomly selects from how many were looped through
            int x = 0;
            int q = 0;
            for (int i = 0; i < tiles_.Count; i++)
            {
                if ((i + 1) < tiles_.Count)
                {
                    if (tiles_[i].getEnemiesSeen() == tiles_[i + 1].getEnemiesSeen())
                    {
                        if (tiles_[x].getClosest() > tiles_[x + 1].getClosest())
                        {
                            q = UnityEngine.Random.Range(0, i);
                            tiles_[q].setSpawn();
                            return tiles_[q].getLocation();
                        }
                    }
                    else
                    {
                        q = UnityEngine.Random.Range(0, i);
                        tiles_[q].setSpawn();
                        return tiles_[q].getLocation();
                    }
                }
                else
                {
                    q = UnityEngine.Random.Range(0, i);
                    tiles_[q].setSpawn();
                    demo = tiles_;
                    return tiles_[q].getLocation();
                }
            }

            q = UnityEngine.Random.Range(0, tiles_.Count - 1);
            tiles_[q].setSpawn();
            demo = tiles_;
            return tiles_[q].getLocation();
        }
    }


    public Vector3 chooseTDMSpawnLocation(int team, List<PossibleFuzzySpawns> tiles_)
    {
        // resets list
        for (int i = 0; i < tiles_.Count; i++)
        {
            tiles_[i].setSpawn(false);
        }

        // sets team lists
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
            // resets variables
            int enemies = 0;
            int friendlies = 0;
            float closest = 10000;
            float closestFriendly = 10000;

            // loops through every enemy
            foreach (Player enemy in enemy_)
            {
                // checks if the enemy is visible and stores data if seen
                Ray ray = new Ray(tiles_[i].getLocation(), (enemy.transform.position - tiles_[i].getLocation()));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, layerMask))
                {
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
            }
            // loops through every friendly
            foreach (Player friendly in friendly_)
            {
                // checks if the friendly is visible and stores data if seen
                Ray ray = new Ray(tiles_[i].getLocation(), (friendly.transform.position - tiles_[i].getLocation()));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, layerMask))
                {
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
            }
            // Fuzzifies values
            enemiesSeen = heatmap_.getFuzzyEnemiesSeen(enemies);
            closenessToEnemy = heatmap_.getFuzzyCloseness(closest / mapWidth);

            friendliesSeen = heatmap_.getFuzzyEnemiesSeen(friendlies);
            closenessToFriendly = heatmap_.getFuzzyCloseness(closestFriendly / mapWidth);

            // sets values for the tiles
            tiles_[i].setEnemiesSeen(enemiesSeen);
            tiles_[i].setClosest(closenessToEnemy);
            tiles_[i].setFriendliesSeen(friendliesSeen);
            tiles_[i].setClosestFriendly(closenessToFriendly);
        }

        // sorts array by importance of variables
        tiles_.Sort(delegate (PossibleFuzzySpawns x, PossibleFuzzySpawns y) { return x.getClosestFriendly().CompareTo(y.getClosestFriendly()); });
        tiles_.Reverse();
        tiles_.Sort(delegate (PossibleFuzzySpawns x, PossibleFuzzySpawns y) { return x.getFriendliesSeen().CompareTo(y.getFriendliesSeen()); });
        tiles_.Reverse();

        tiles_.Sort(delegate (PossibleFuzzySpawns x, PossibleFuzzySpawns y) { return x.getClosest().CompareTo(y.getClosest()); });
        tiles_.Reverse();
        tiles_.Sort(delegate (PossibleFuzzySpawns x, PossibleFuzzySpawns y) { return x.getEnemiesSeen().CompareTo(y.getEnemiesSeen()); });

        // if the first array member is better than the second it outputs the first
        if (tiles_[0].getEnemiesSeen() < tiles_[1].getEnemiesSeen())
        {
            tiles_[0].setSpawn();
            demo = tiles_;
            return tiles_[0].getLocation();
        }
        else
        {
            // checks to see if the current member is better than the next
            // other wise randomly selects from how many were looped through
            int q = 0;
            for (int i = 0; i < tiles_.Count; i++)
            {
                if ((i + 1) < tiles_.Count)
                {
                    if (tiles_[i].getEnemiesSeen() != tiles_[i + 1].getEnemiesSeen())
                    {
                        q = UnityEngine.Random.Range(0, i);
                        tiles_[q].setSpawn();
                        break;
                    }
                    if (tiles_[i].getClosest() > tiles_[i + 1].getClosest())
                    {
                        q = UnityEngine.Random.Range(0, i);
                        tiles_[q].setSpawn();
                        break;
                    }
                    if (tiles_[i].getFriendliesSeen() > tiles_[i + 1].getFriendliesSeen())
                    {
                        q = UnityEngine.Random.Range(0, i);
                        tiles_[q].setSpawn();
                        break;
                    }
                    if (tiles_[i].getClosestFriendly() < tiles_[i + 1].getClosestFriendly())
                    {
                        q = UnityEngine.Random.Range(0, i);
                        tiles_[q].setSpawn();
                        break;
                    }
                }
                else
                {
                    q = UnityEngine.Random.Range(0, i);
                    tiles_[q].setSpawn();
                }
            }
            // just for demonstating that it works and where it has chosen
            demo = tiles_;
            return tiles_[q].getLocation();
        }
    }


    private void OnDrawGizmos()
    {
        // draws the heatmap (only going to be used for demos)
        if (demo != null)
        {
            for (int i = 0; i < demo.Count; i++)
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

                Gizmos.DrawCube(demo[i].getLocation(), defaultVec);
            }
        }
    }
}
