using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensing : MonoBehaviour
{
    static public Sensing instance;
    static public float sensingTime = .2f;
    static private float maxDistance = 1200;

    // list of all current box colliders that are "hitting" an object
    public List<SensorHit> hit = new List<SensorHit>();

    // list of distances in the 8 directions
    public List<float> distances = new List<float>();

    void Awake()
    {
        // check for 1 instance
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        // inintalize distances to 0
        for (int i = 0; i < 8; i++)
            distances.Add(maxDistance);

        // init
        hit = new List<SensorHit>();
    }

    // runs 50 times a second
    void FixedUpdate()
    {
        // keep distances up to date at all times
        for (int i = 0; i < 8 ; i++)
        {
            float minDistance = maxDistance;

            // loop through each sensorHit currently
            foreach (SensorHit sh in hit)
            {
                // only check 1 direction at a time
                if (sh.direction != i + 1)
                    continue;

                // is this distance shorter than minDistance
                if (sh.distance < minDistance)
                    minDistance = sh.distance;
            }

            // save minDistance in this direction
            distances[i] = minDistance;
        }
    }

    public float getDirDist(int dir)
    {
        return distances[dir - 1];
    }
}
