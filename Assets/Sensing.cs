using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensing : MonoBehaviour
{
    static public Sensing instance;
    static public float sensingTime = .4f;
    static public float maxDistance = 1200;
    private float updateTimer = 0;

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

    public void UpdateIRData()
    {
        // set timer then update Rover distance data
        updateTimer = sensingTime;
    }

    // runs 50 times a second
    void FixedUpdate()
    {
        // update rover data
        if (updateTimer > 0)
            updateTimer -= Time.deltaTime;
        else
        {
            if (updateTimer != -10)
            {
                // update rover data
                for (int i = 0; i < Rover.instance.distances.Count; i++)
                {
                    Rover.instance.distances[i] = distances[i];
                }
                updateTimer = -10;
            }
        }

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
}
