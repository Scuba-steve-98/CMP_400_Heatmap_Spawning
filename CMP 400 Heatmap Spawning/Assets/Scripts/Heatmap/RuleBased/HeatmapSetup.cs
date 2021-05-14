using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class HeatmapSetup : MonoBehaviour
{
    //declaring variables
    int width, bredth, scale;

    Vector3 planeSize, bottomLeft, bottomRight, topLeft, rayStart, rightIncrementation, forwardIncrementation;

    List<Tiles> tilesList;

    HeatmapData heatmapData;

    GameManager gm;

    [SerializeField]
    GameObject bottomLeftObj;

    [SerializeField]
    GameObject bottomRightObj;

    [SerializeField]
    GameObject topLeftObj;


    // Start is called before the first frame update
    void Start()
    {
        scale = 2;

        tilesList = new List<Tiles>();

        // initialise variables for the for loops
        planeSize = GetComponent<MeshRenderer>().bounds.size;
        width = (int)planeSize.x / scale;
        bredth = (int)planeSize.z / scale;

        // gets the positions of the plane corners
        bottomLeft = bottomLeftObj.transform.position;
        bottomRight = bottomRightObj.transform.position;
        topLeft = topLeftObj.transform.position;

        // calculates the vector for moving the tiles position
        rightIncrementation = (bottomRight - bottomLeft) / width;
        forwardIncrementation = (topLeft - bottomLeft) / bredth;

        heatmapData = FindObjectOfType<HeatmapData>();

        // call function to activate tiles
        setHeatmapUp();
    }


    private void setHeatmapUp()
    {
        // layer mask to only collide with objects on specific layer
        LayerMask tempLayerMask = 1 << 10;

        // loops through and adds tiles to the list if it does not collide with any scenery
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < bredth; j++)
            {
                // gets location of each of the tiles
                rayStart = bottomLeft + rightIncrementation / 2 + forwardIncrementation / 2 + (((float)i * (rightIncrementation / 2)) * scale) + (((float)j * (forwardIncrementation / 2)) * scale);


                // creates the ray that will check if it collides with scenery
                Ray ray = new Ray(rayStart - Vector3.up * 0.2f, Vector3.up);
                if (!Physics.Raycast(ray, 5, tempLayerMask))
                {
                    // adds the tile to the list
                    tilesList.Add(new Tiles(rayStart));
                }
            }
        }
        // passes the tiles through to the spawn selector
        heatmapData.addTiles(tilesList);
        gm = FindObjectOfType<GameManager>();
        if (gm.isHaloBattleCreek)
        {
            if (!TryGetComponent<BoxCollider>(out BoxCollider bc))
            {
                Destroy(GetComponent<MeshRenderer>());
            }
        }
    }
}