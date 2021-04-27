using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    //---------------------- This code is mostly taken from a youtube tutorial for AI enemies
    //---------------------- tutorial is found at URL https://www.youtube.com/watch?v=qL1toYXm_Sc&ab_channel=DitzelGames
    //---------------------- youtube channel is DitzelGames
    protected Player player;
    protected Controller controller;
    protected NavMeshAgent agent;
    protected bool isRunning;
    protected Locations[] potentialLocations;

    [HideInInspector]
    public Locations target;
    protected float nextState;

    int layerMask;
    bool enemyFound, respawned;
    Player targetPlayer;
    List<Player> enemies;
    // Start is called before the first frame update
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        controller = FindObjectOfType<Controller>();
        player = GetComponent<Player>();
        potentialLocations = FindObjectsOfType<Locations>();
        target = potentialLocations[Random.Range(0, potentialLocations.Length)];
        agent.SetDestination(target.transform.position);
        isRunning = true;
        enemies = new List<Player>();
        enemyFound = false;
        respawned = false;

        layerMask |= 1 << 9;
        layerMask |= 1 << 10;
        layerMask = ~layerMask;
    }

    // Update is called once per frame
    void Update()
    {
        agent.updatePosition = true;
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        nextState -= Time.deltaTime;

        if (isRunning)
        {
            player.Input.LookX = Mathf.Sign(agent.desiredVelocity.x);
            player.Input.LookZ = Mathf.Sign(agent.desiredVelocity.z);

            if (Vector3.Distance(target.transform.position, transform.position) < 1f)
            {
                isRunning = false;
                nextState = Random.Range(3f, 12f);
            }
            enemyFound = false;
        }
        else
        {
            // My code ------------------------------------------------------------------
            if (!enemyFound)
            {
                float closest = 10000;
                foreach (Player p in enemies)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(gameObject.transform.position, (p.transform.position - gameObject.transform.position), out hit, 50, layerMask))
                    {
                        if (hit.collider.TryGetComponent<Player>(out Player r))
                        {
                            float c = Vector3.Distance(transform.position, r.transform.position);
                            if (c < closest)
                            {
                                closest = c;
                                targetPlayer = r;
                                enemyFound = true;
                            }
                        }
                    }
                }
            }

            if (enemyFound)
            {
                var look = targetPlayer.transform.position - transform.position;
                player.Input.LookX = look.x;
                player.Input.LookZ = look.z;
            }
            //--------------------------------------------------------------------------


            if (nextState < 0)
            {
                isRunning = true;
                var targetIndex = Random.Range(0, potentialLocations.Length);
                for (var i = 0; i < potentialLocations.Length && potentialLocations[targetIndex].occupied; i++)
                    targetIndex = (targetIndex + 1) % potentialLocations.Length;
                target = potentialLocations[targetIndex];
                target.enemyGoal = this;
                agent.SetDestination(target.transform.position);
                respawned = false;
            }
        }

        //transform.position += Agent.desiredVelocity * Time.deltaTime;
        if (Vector3.Distance(target.transform.position, transform.position) > 0.5f)
        {
            player.Input.RunX = Mathf.Abs(agent.desiredVelocity.x) > 0.1f ? Mathf.Sign(agent.desiredVelocity.x) : 0;
            player.Input.RunZ = Mathf.Abs(agent.desiredVelocity.z) > 0.1f ? Mathf.Sign(agent.desiredVelocity.z) : 0;
        }
        else
        {
            player.Input.RunX = 0;
            player.Input.RunZ = 0;
        }

        if (respawned)
        {
            player.Input.Shoot = false;
        }
        else 
        {
            player.Input.Shoot = !isRunning;
        }

        Debug.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + agent.desiredVelocity * 10);
    }

    // code onwards is mine
    private void OnTriggerEnter(Collider other)
    {
        Player p;
        if (other.gameObject.TryGetComponent<Player>(out p))
        {
            if (p.getTeam() != player.getTeam())
            {
                if (!enemies.Contains(p))
                {
                    enemies.Add(p);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //int x = enemies.Count;
        //if (x > 0)
        //{ 
        //    for (int i = 0; i < x; i++)
        //    {
        //        if (enemies[i].IsDead())
        //        {
        //            enemies.Remove(enemies[i]);
        //        }
        //    }
        //}
    }

    private void OnTriggerExit(Collider other)
    {
        Player p;
        if (other.gameObject.TryGetComponent<Player>(out p))
        {
            if (enemies.Contains(p))
            {
                enemies.Remove(p);
            }
        }
    }

    public void Respawned()
    {
        isRunning = false;
        nextState = 0.2f;
        respawned = true;
    }
}
