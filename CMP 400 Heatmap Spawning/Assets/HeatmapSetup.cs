using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapSetup : MonoBehaviour
{
    int width, bredth, layerMask, index;

    float widthTemp, bredthTemp, distance, colliderRadius;

    bool isNear;

    Vector3 planeSize, bottomLeft, rayStart, rayEnd, defaultVec;

    Color32 defaultColour, peakThreat;

    float[,] threatLevel;

    Tiles[] tiles;

    [SerializeField]
    [Range(0, 1)]
    int scale = 3;

    // Start is called before the first frame update
    void Start()
    {
        index = 0;
        isNear = false;
        distance = 0f;
        defaultVec = new Vector3(1, 1, 1);
        planeSize = GetComponent<MeshRenderer>().bounds.size;
        width = (int)planeSize.x;// * scale;
        bredth = (int)planeSize.z;// * scale;
        bottomLeft = gameObject.transform.position - (planeSize / 2);
        layerMask = 1 << 9;
        defaultColour = new Color32(0, 0, 0, 1);
        peakThreat = new Color32(255, 0, 0, 1);
        threatLevel = new float[20, 20];
        tiles = new Tiles[400];
    }

    // Update is called once per frame
    void Update()
    {
        index = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < bredth; j++)
            {
                widthTemp = (float)i + 0.5f;// / scale;
                bredthTemp = (float)j + 0.5f;// / scale;
                rayStart = bottomLeft + Vector3.right * widthTemp + Vector3.forward * bredthTemp;
                rayEnd = rayStart + Vector3.up * 5;
                RaycastHit[] hit;
                hit = Physics.RaycastAll(rayStart, rayEnd, 5, layerMask);
                //Debug.DrawLine(rayStart, rayEnd);
                //threatLevel[i, j] = rayStart;
                threatLevel[i, j] = 0f;
                
                for (int c = 0; c < hit.Length; c++)
                {
                    if (hit[c].collider.transform != null)
                    {
                        float threat = hit[c].collider.gameObject.GetComponentInParent<ThreatData>().getThreatLevel();
                        colliderRadius = hit[c].collider.transform.localScale.x;
                        distance = Vector3.Distance(rayStart, hit[c].transform.position);
                        Debug.Log("Distance: " + distance);
                        threatLevel[i, j] += Mathf.Lerp(threat, 0, distance / (colliderRadius / 2f));
                        Debug.Log(threatLevel[i, j]);
                    }
                }
                tiles[index] = new Tiles(threatLevel[i, j], rayStart);
                //Debug.Log(threatLevel[i, j]);
                index++;
            }
        }
        //OnDrawGizmos();
    }


    private void OnDrawGizmos()
    {
        if (threatLevel != null)
        {
            foreach (Tiles tile in tiles)
            {
                Gizmos.color = Color.Lerp(Color.white, Color.red, (tile.getThreatLevel() / 100f));
                //Gizmos.color = Color32.Lerp(defaultColour, peakThreat, (tile.getThreatLevel() / 200f));
                Gizmos.DrawCube(tile.getLocation(), defaultVec);
            }
        }

    }
}
