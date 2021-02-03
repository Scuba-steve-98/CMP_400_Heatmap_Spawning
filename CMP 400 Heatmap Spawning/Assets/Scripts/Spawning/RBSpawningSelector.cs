using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBSpawningSelector : MonoBehaviour
{
    PossibleSpawns[] tiles_;
    HeatmapSetup heatmap_;
    GameManager gameManager_;

    Vector3 defaultVec;

    // Start is called before the first frame update
    void Start()
    {
        heatmap_ = FindObjectOfType<HeatmapSetup>();
        gameManager_ = FindObjectOfType<GameManager>();
        tiles_ = new PossibleSpawns[10];
        defaultVec = new Vector3(1, 0.75f, 1) * 2;
    }

    private void Update()
    {
        
    }


    public void chooseFFASpawnLocation()
    {
        tiles_ = heatmap_.getFFATiles();
        Debug.Log("Yeet");
    }


    public void chooseTDMSpawnLocation(int team)
    {
        tiles_ = heatmap_.getTDMTiles(team);
        Debug.Log("Yeet");
    }


    private void OnDrawGizmos()
    {
        // draws the heatmap (only going to be used for demos)
        if (tiles_ != null)
        {
            foreach (PossibleSpawns tile in tiles_)
            {
                if (tile != null)
                {
                    // sets cube colour based on its threat value
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(tile.getLocation(), defaultVec);
                }
            }
        }
    }
}
