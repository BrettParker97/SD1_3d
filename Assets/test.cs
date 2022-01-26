using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public GameObject roverGameObj;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 moveVec = new Vector3();
        moveVec.z = .01f;
        roverGameObj.transform.position += moveVec;
    }
}
