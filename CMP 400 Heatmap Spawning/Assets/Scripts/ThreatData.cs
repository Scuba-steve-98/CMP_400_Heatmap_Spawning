using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreatData : MonoBehaviour
{
    [SerializeField]
    [Range(50, 100)]
    float baseThreatLevel = 75f;

    int kills;
    int deaths;
    float gameKD;
    float overAllKD;

    float threatLevel;


    // will be used later for calculating players current threat level
    public void setThreatLevel(float KD)
    {
        //threatLevel = 
    }

    public float getThreatLevel()
    {
        return baseThreatLevel;
    }

    public float getFriendlyLevel()
    {
        return baseThreatLevel;
    }
}
