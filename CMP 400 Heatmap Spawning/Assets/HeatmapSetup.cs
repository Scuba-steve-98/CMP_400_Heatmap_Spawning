using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapSetup : MonoBehaviour
{
    int width, bredth, layerMask, index;

    float widthTemp, bredthTemp, distance, colliderRadius;

    Vector3 planeSize, bottomLeft, rayStart, rayEnd, defaultVec;

    float[,] threatLevel;

    Tiles[] tiles;

    [SerializeField]
    [Range(1, 10)]
    int scale = 5;

    // Start is called before the first frame update
    void Start()
    {
        index = 0;
        distance = 0f;
        defaultVec = new Vector3(1, 1, 1) * scale;
        defaultVec.y = 1;
        planeSize = GetComponent<MeshRenderer>().bounds.size;
        width = (int)planeSize.x / scale;
        bredth = (int)planeSize.z / scale;
        Debug.Log("Plane Size: " + planeSize.x + ", Width: " + width);
        bottomLeft = gameObject.transform.position - (planeSize / 2);
        layerMask = 1 << 9;
        int platWidth = (int)transform.localScale.x * 5;
        int platBredth = (int)transform.localScale.z * 5;
        threatLevel = new float[platWidth, platBredth];
        int arrayIndexMax = platBredth * platWidth;
        tiles = new Tiles[arrayIndexMax];
    }

    // Update is called once per frame
    void Update()
    {
        index = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < bredth; j++)
            {
                widthTemp = (float)i * scale + (0.5f * scale);
                bredthTemp = (float)j * scale + 0.5f * scale;
                rayStart = bottomLeft + Vector3.right * widthTemp + Vector3.forward * bredthTemp;
                rayEnd = rayStart + Vector3.up * 5;
                RaycastHit[] hit;
                hit = Physics.RaycastAll(rayStart, rayEnd, 5, layerMask);
                Debug.DrawLine(rayStart, rayEnd);
                //threatLevel[i, j] = rayStart;
                threatLevel[i, j] = 0f;
                
                for (int c = 0; c < hit.Length; c++)
                {
                    if (hit[c].collider.transform != null)
                    {
                        float threat = hit[c].collider.gameObject.GetComponentInParent<ThreatData>().getThreatLevel();
                        colliderRadius = hit[c].collider.transform.localScale.x;
                        distance = Vector3.Distance(rayStart, hit[c].transform.position);
                        //Debug.Log("Distance: " + distance);
                        threatLevel[i, j] += Mathf.Lerp(threat, 0, distance / (colliderRadius / 2f));
                        //Debug.Log(threatLevel[i, j]);
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
