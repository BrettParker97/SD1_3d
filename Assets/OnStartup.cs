using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnStartup : MonoBehaviour
{
    public GameObject roverGameObject;
    public GameObject row11;
    public GameObject row12;
    public GameObject row13;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 roverPosition = roverGameObject.transform.position;
        SensorHit[] res = row11.GetComponentsInChildren<SensorHit>();
        foreach (SensorHit sh in res)
        {

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
