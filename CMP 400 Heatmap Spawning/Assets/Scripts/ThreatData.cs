using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreatData : MonoBehaviour
{
    [SerializeField]
    [Range(50, 100)]
    float baseThreatLevel = 75f;


    // will be used later for calculating players current threat level
    public void setThreatLevel()
    {

    }

    public float getThreatLevel()
    {
        return baseThreatLevel;
    }
}
