using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreatData : MonoBehaviour
{
    [SerializeField]
    [Range(50, 100)]
    float baseThreatLevel = 75f;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.eulerAngles = new Vector3(0f, 0f, 0f);
    }

    // will be used later for calculating players current threat level
    public void setThreatLevel()
    {

    }

    public float getThreatLevel()
    {
        return baseThreatLevel;
    }
}
