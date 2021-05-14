﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class HeatmapData : MonoBehaviour
{
    //declaring variables
    int layerMask;
    int numberOfTiles = 45;

    float distance, colliderRadius, team1Threat, team2Threat, team1Friendly, team2Friendly;

    Vector3 rayStart;

    List<Tiles> tilesList;

    PossibleSpawns[] possibleFFASpawnAreas;
    PossibleSpawns[] possibleP1SpawnAreas;
    PossibleSpawns[] possibleP2SpawnAreas;

    GameManager gameManager_;

    Vector3 location;

    float targetThreatValue;

    // Start is called before the first frame update
    void Start()
    {
        // initialise variables
        distance = 0f;
        layerMask = 1 << 9;
        targetThreatValue = 0;

        gameManager_ = FindObjectOfType<GameManager>().GetComponent<GameManager>();

        /// creates an array of tiles for possible spawns
        possibleFFASpawnAreas = new PossibleSpawns[numberOfTiles];
        possibleP1SpawnAreas = new PossibleSpawns[numberOfTiles];
        possibleP2SpawnAreas = new PossibleSpawns[numberOfTiles];

        // creates the individual tiles variables
        for (int i = 0; i < numberOfTiles; i++)
        {
            possibleFFASpawnAreas[i] = new PossibleSpawns();
            possibleP1SpawnAreas[i] = new PossibleSpawns();
            possibleP2SpawnAreas[i] = new PossibleSpawns();
        }
    }

    // creates the list of tiles
    public void addTiles(List<Tiles> ft)
    {
        if (tilesList == null)
        {
            tilesList = new List<Tiles>();
        }

        tilesList.AddRange(ft);
    }

    public Vector3 getHeatmapData(int team, int kills, int deaths, float threatLevel)
    {
        // resets tiles closeness
        resetArrays();

        // calculates the target level based on the game mode
        if (gameManager_.isTDM())
        {
            float currentThreatLevel = threatLevel * gameManager_.getTeamThreat(Mathf.Abs(team - 1));
            if (currentThreatLevel > 200)
            {
                currentThreatLevel = 200;
            }
            targetThreatValue = currentThreatLevel;
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
                targetThreatValue = currentThreatMultiplier;
            }
            else
            {
                float multiplier = Mathf.Lerp(0.6f, 0.8f, gameManager_.getGameProgress());
                float currentThreatMultiplier = threatLevel * multiplier;
                if (currentThreatMultiplier >= 200)
                {
                    currentThreatMultiplier = 200;
                }
                targetThreatValue = currentThreatMultiplier;
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
                // sets values for the tiles
                tilesList[i].setValues(team1Threat, team2Threat, team1Friendly, team2Friendly);

                // checks how close the threat is to the target and adds it if it is closer than any in the array
                float close = Mathf.Abs(team1Threat - targetThreatValue);
                if (close < possibleP1SpawnAreas[numberOfTiles - 1].getCloseness())
                {
                    possibleP1SpawnAreas[numberOfTiles - 1].setCloseness(close);
                    possibleP1SpawnAreas[numberOfTiles - 1].setLocation(tilesList[i].getLocation());
                    Array.Sort<PossibleSpawns>(possibleP1SpawnAreas, delegate (PossibleSpawns x, PossibleSpawns y) { return x.getCloseness().CompareTo(y.getCloseness()); });                }

                close = Mathf.Abs(team2Threat - targetThreatValue);
                if (close < possibleP2SpawnAreas[numberOfTiles - 1].getCloseness())
                {
                    possibleP2SpawnAreas[numberOfTiles - 1].setCloseness(close);
                    possibleP2SpawnAreas[numberOfTiles - 1].setLocation(tilesList[i].getLocation());
                    Array.Sort<PossibleSpawns>(possibleP2SpawnAreas, delegate (PossibleSpawns x, PossibleSpawns y) { return x.getCloseness().CompareTo(y.getCloseness()); });
                }
            }
            else
            {
                // stores the threat level for the tile after it has been calculated
                tilesList[i].setValues(threatLevel);

                // checks how close the threat is to the target and adds it if it is closer than any in the array
                float close = Mathf.Abs(threatLevel - targetThreatValue);
                if (close < possibleFFASpawnAreas[numberOfTiles - 1].getCloseness())
                {
                    possibleFFASpawnAreas[numberOfTiles - 1].setCloseness(close);
                    possibleFFASpawnAreas[numberOfTiles - 1].setLocation(tilesList[i].getLocation());
                    Array.Sort<PossibleSpawns>(possibleFFASpawnAreas, delegate (PossibleSpawns x, PossibleSpawns y) { return x.getCloseness().CompareTo(y.getCloseness()); });
                }
            }
        }

        // passes tiles through to the spawn selector
        if (gameManager_.isTDM())
        {
            RBSpawningSelector rbss = FindObjectOfType<RBSpawningSelector>();
            if (team == 0)
            {
                location = rbss.chooseTDMSpawnLocation(team, possibleP1SpawnAreas);
                location.y += 1.1f;
                return location;
            }
            else
            {
                location = rbss.chooseTDMSpawnLocation(team, possibleP2SpawnAreas);
                location.y += 1.1f;
                return location;
            }
        }
        else
        {
            RBSpawningSelector rbss = FindObjectOfType<RBSpawningSelector>();
            location = rbss.chooseFFASpawnLocation(possibleFFASpawnAreas);
            location.y += 1.1f;
            return location;
        }
    }

    void resetArrays()
    {
        for (int i = 0; i < numberOfTiles; i++)
        {
            possibleFFASpawnAreas[i].setCloseness(150);
            possibleP1SpawnAreas[i].setCloseness(150);
            possibleP2SpawnAreas[i].setCloseness(150);
        }
    }
}
