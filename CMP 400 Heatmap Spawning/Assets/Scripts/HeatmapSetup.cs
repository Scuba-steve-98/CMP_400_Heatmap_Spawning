using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapSetup : MonoBehaviour
{
    //declaring variables
    int width, bredth, layerMask, scale, activeTiles;

    float widthTemp, bredthTemp, distance, colliderRadius, threatLevel;

    Vector3 planeSize, bottomLeft, rayStart, defaultVec;

    Tiles[] tiles;

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

        // call function to activate tiles
        setHeatmapUp();
    }

    // Update is called once per frame
    void Update()
    {
        // loops through for the number of active tiles
        for (int i = 0; i < activeTiles; i++)
        {
            // creates a ray for detecting enemies
            rayStart = tiles[i].getLocation();
            Ray ray = new Ray(rayStart, Vector3.up);

            // stores all of the hits that the rays collide with
            RaycastHit[] hit;
            hit = Physics.RaycastAll(ray, 5, layerMask);

            // resets the threat level
            threatLevel = 0;

            // loops through for the number of hits the tile has
            for (int c = 0; c < hit.Length; c++)
            {
                if (hit[c].collider.transform != null)
                {
                    // gets the data for calculating the threat level
                    float threat = hit[c].collider.gameObject.GetComponentInParent<ThreatData>().getThreatLevel();
                    colliderRadius = hit[c].collider.transform.localScale.x;

                    // calculates the threat level for the tile
                    distance = Vector3.Distance(rayStart, hit[c].transform.position);
                    threatLevel += Mathf.Lerp(threat, 0, distance / (colliderRadius / 2f));
                }
            }
            // stores the threat level for the tile after it has been calculated
            tiles[i].setValues(threatLevel);
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

    private void OnDrawGizmos()
    {
        // draws the heatmap (only going to be used for demos)
        if (tiles != null)
        {
            foreach (Tiles tile in tiles)
            {
                {
                    // sets cube colour based on its threat value
                    Gizmos.color = Color.Lerp(Color.white, Color.red, (tile.getThreatLevel() / 100f));
                    Gizmos.DrawCube(tile.getLocation(), defaultVec);
                }
            }
        }
    }
}