using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    int width, bredth, layerMask, index, scale;

    float widthTemp, bredthTemp, distance, colliderRadius;

    Vector3 planeSize, bottomLeft, rayStart, rayEnd, defaultVec;

    float[,] threatLevel;

    Tiles[] tiles;

    // Start is called before the first frame update
    void Start()
    {
        scale = 2;
        index = 0;
        distance = 0f;
        layerMask = 1 << 9;

        // just a vector for drawing the gizmos cubes for the demo
        defaultVec = new Vector3(1, 1, 1) * scale;
        defaultVec.y = 1;

        // initialise variables for the for loops
        planeSize = GetComponent<MeshRenderer>().bounds.size;
        width = (int)planeSize.x / scale;
        bredth = (int)planeSize.z / scale;
        bottomLeft = gameObject.transform.position - (planeSize / 2);
        //Debug.Log("Plane Size: " + planeSize.x + ", Width: " + width);

        // initialise variables for setting up the heatmap tiles
        // so it works for on the plane at any scale
        int platWidth = (int)transform.localScale.x * 5;
        int platBredth = (int)transform.localScale.z * 5;
        threatLevel = new float[platWidth, platBredth];
        int arrayIndexMax = platBredth * platWidth;
        tiles = new Tiles[arrayIndexMax];
    }

    // Update is called once per frame
    void Update()
    {
        // resets the variable so it can loop back through
        index = 0;

        // double for loop for the 2D array
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < bredth; j++)
            {
                // sets the location for centre of the tile for the ray to be cast from
                widthTemp = ((float)i + 0.5f) * scale;
                bredthTemp = ((float)j + 0.5f) * scale;
                rayStart = bottomLeft + Vector3.right * widthTemp + Vector3.forward * bredthTemp;
                rayEnd = rayStart + Vector3.up * 5;

                // casts a ray and stores all the instances of the ray colliding with the spawner collider
                RaycastHit[] hit;
                hit = Physics.RaycastAll(rayStart, rayEnd, 5, layerMask);

                //Debug.DrawLine(rayStart, rayEnd);

                // sets the tiles threat level to 0
                threatLevel[i, j] = 0f;

                // loops through all of the hits that tile got
                for (int c = 0; c < hit.Length; c++)
                {
                    if (hit[c].collider.transform != null)
                    {
                        // receives the treat level for the object and the radius of the players threat
                        float threat = hit[c].collider.gameObject.GetComponentInParent<ThreatData>().getThreatLevel();
                        colliderRadius = hit[c].collider.transform.localScale.x;

                        // gets the distance from the tile to the object and calculated the threat level
                        distance = Vector3.Distance(rayStart, hit[c].transform.position);
                        threatLevel[i, j] += Mathf.Lerp(threat, 0, distance / (colliderRadius / 2f));
                    }
                }
                // stores the threat level and the location for the tile
                tiles[index] = new Tiles(threatLevel[i, j], rayStart);
                index++;
            }
        }
    }


    private void OnDrawGizmos()
    {
        // draws the heatmap (only going to be used for demos)
        if (threatLevel != null)
        {
            foreach (Tiles tile in tiles)
            {
                Gizmos.color = Color.Lerp(Color.white, Color.red, (tile.getThreatLevel() / 100f));
                Gizmos.DrawCube(tile.getLocation(), defaultVec);
            }
        }

    }
}
