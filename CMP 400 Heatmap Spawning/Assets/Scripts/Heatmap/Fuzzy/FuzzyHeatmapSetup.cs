using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class FuzzyHeatmapSetup : MonoBehaviour
{    //declaring variables
    int width, bredth, scale;

    Vector3 planeSize, bottomLeft, bottomRight, topLeft, rayStart, rightIncrementation, forwardIncrementation;

    List<FuzzyTiles> tiles;

    FuzzyHeatmapData heatmapData;

    [SerializeField]
    GameObject bottomLeftObj;

    [SerializeField]
    GameObject bottomRightObj;

    [SerializeField]
    GameObject topLeftObj;

    // Start is called before the first frame update
    void Start()
    {
        // initialise variables
        scale = 2;

        tiles = new List<FuzzyTiles>();

        // initialise variables for the for loops
        planeSize = GetComponent<MeshRenderer>().bounds.size;
        width = (int)planeSize.x / scale;
        bredth = (int)planeSize.z / scale;

        bottomLeft = bottomLeftObj.transform.position;
        bottomRight = bottomRightObj.transform.position;
        topLeft = topLeftObj.transform.position;

        rightIncrementation = (bottomRight - bottomLeft) / width;
        forwardIncrementation = (topLeft - bottomLeft) / bredth;

        heatmapData = FindObjectOfType<FuzzyHeatmapData>();

        // call function to activate tiles
        setHeatmapUp();
    }


    private void setHeatmapUp()
    {
        // layer mask to only collide with objects on specific layer
        LayerMask tempLayerMask = 1 << 10;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < bredth; j++)
            {
                // gets location of each of the tiles
                rayStart = bottomLeft + rightIncrementation / 2 + forwardIncrementation / 2 + (((float)i * (rightIncrementation / 2)) * scale) + (((float)j * (forwardIncrementation / 2)) * scale);

                // creates the ray that will check if it collides with scenery
                Ray ray = new Ray(rayStart, Vector3.up);
                if (!Physics.Raycast(ray, 5, tempLayerMask))
                {
                    // initialises the array if it doesn't collide with an object on the scenery layer
                    tiles.Add(new FuzzyTiles(rayStart));
                }
            }
        }
        heatmapData.addTiles(tiles);
    }
}