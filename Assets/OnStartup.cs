using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnStartup : MonoBehaviour
{
    static public OnStartup instance;
    public GameObject roverGameObject;
    public List<GameObject> rowList = new List<GameObject>();

    // holds all gameobjects in a key:direction
    public List<List<SensorHit>> gameObjectsInDir = new List<List<SensorHit>>();

    void Awake()
    {
        // instance check
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        // init vars
        // add 8 lists to gameobjectsindir for each direction
        for (int i = 0; i < 8; i++)
        {
            List<SensorHit> list = new List<SensorHit>();
            gameObjectsInDir.Add(list);
        }

        // get rover position
        Vector3 roverPosition = roverGameObject.transform.position;

        int countd = 0;
        foreach (GameObject direction in rowList)
        {
            // get 3 children
            List<GameObject> res = new List<GameObject>();
            res.Add(direction.transform.GetChild(0).gameObject);
            res.Add(direction.transform.GetChild(1).gameObject);
            res.Add(direction.transform.GetChild(2).gameObject);

            int countc = 1;
            foreach (GameObject go in res)
            {
                // get all sensors on gameobject
                SensorHit[] res2 = go.GetComponentsInChildren<SensorHit>();

                int countr = 1;
                foreach (SensorHit sh in res2)
                {
                    // save gameobject to dictionary
                    gameObjectsInDir[countd].Add(sh);
                    sh.gameObject.GetComponent<MeshRenderer>().forceRenderingOff = true;

                    // set ID
                    sh.direction = countd + 1;
                    sh.col = countc;
                    sh.row = countr;
                    countr++;

                    // set distance from rover (in mm)
                    sh.distance = Vector3.Distance(roverPosition, sh.gameObject.transform.position) * 100;
                }

                // Id management
                countc++;
                if (countc == 4)
                    countc = 1;
            }
            countd++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
