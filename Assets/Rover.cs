using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rover : MonoBehaviour
{
    public static Rover instance;

    public GameObject roverGameobject;
    public GameObject destination;

    private Vector3 initalHitPosition;
    public Vector3 positionDelta;
    public Vector3 initalRoverPosition;

    public bool initalDetachHit = false;
    public bool onSlope = false;
    public bool hitOnSlope = false;
    public bool movingAway = false;
    public bool rotating = false;
    public bool turnRight;

    public int roverDirection = 1;
    // 0 = normal, 1 = outter, 2 = inner
    private int currentLoop = 0;
    public int innerHit;
    public int oldRoverDirection;
    public int oldCurrentLoop;
    public static int DIRECTIONSAMPLEMIN = 10; // fixedUpdate called 50/second
    public int changeCounter = 0;
    public int turnCount;
    public int rotateAmount = 0;

    public float innerLoopTimer;
    public float innerLoopX = 0.1f;
    public float initalHitDistance;
    public float timer = 0;
    public float manuallySetSpeed = .5f; // mph

    void PointTowardsDestination()
    {
        // pick a direction
        Vector3 desPos = destination.transform.position;
        Vector3 roverPos = roverGameobject.transform.position;

        // angle calc for traingle
        float hyp = Mathf.Sqrt(Mathf.Pow(desPos.x - roverPos.x, 2) + Mathf.Pow(desPos.z - roverPos.z, 2));
        
        float opp = 0;
        if (roverPos.z >= desPos.z && roverPos.z > 0)
            opp = roverPos.z - desPos.z;
        else if (roverPos.z >= desPos.z && roverPos.z < 0)
            opp = -desPos.z + roverPos.z;
        else if (roverPos.z < desPos.z && desPos.z > 0)
            opp = desPos.z - roverPos.z;
        else
            opp = -roverPos.z + desPos.z;

        float angle = Mathf.Abs(Mathf.Asin(opp / hyp)) * (180 / Mathf.PI);

        // calc which quad this angle is in
        int whatQuad = 1;
        if (roverPos.x <= desPos.x && roverPos.z <= desPos.z)
            whatQuad = 1;
        else if (roverPos.x >= desPos.x && roverPos.z <= desPos.z)
            whatQuad = 2;
        else if (roverPos.x >= desPos.x && roverPos.z >= desPos.z)
            whatQuad = 3;
        else
            whatQuad = 4;

        // finish angle calc
        float pointAngle= 1;
        if (whatQuad == 1)
            pointAngle = angle;
        else if (whatQuad == 2)
            pointAngle = 180 - angle;
        else if (whatQuad == 3)
            pointAngle = 180 + angle;
        else
            pointAngle = 360 - angle;


        // pick a direction
        int[] angles = {90, 45, 0, 315, 270, 225, 180, 135};
        int best = 1;
        float closest = 360;
        for (int i = 0; i < angles.Length; i++)
        {
            float temp = Mathf.Abs(pointAngle - angles[i]);
            if (temp < closest)
            {
                closest = temp;
                best = i + 1;
            }
        }

        if (best != roverDirection)
            changeCounter++;
        else
            changeCounter--;

        if (changeCounter < 0)
            changeCounter = 0;

        if (changeCounter <= DIRECTIONSAMPLEMIN)
            return;

        // rotate rover
        Motors.instance.RotateRover(best);
        return;
    }

    void NormalLoop ()
    {
        // check if we hit somthing in our direction
        // in 3d version we have 3 boxes per direction, ergo check first 3 not 1
        if (onSlope)
        {
            if (hitOnSlope)
            {
                if (checkDirection(1, 2))
                {
                    // start outter obj avoidance
                    currentLoop = 1;
                    initalHitPosition = roverGameobject.transform.position;
                    initalHitDistance = Vector3.Distance(initalHitPosition, destination.transform.position);
                    return;
                }
            }
        }
        else
        {
            if (checkDirection(1, 2))
            {
                // start outter obj avoidance
                currentLoop = 1;
                initalHitPosition = roverGameobject.transform.position;
                initalHitDistance = Vector3.Distance(initalHitPosition, destination.transform.position);
                return;
            }
        }

        // point toward direction
        PointTowardsDestination();

        return;
    }

    void ControlOutterLoop()
    {
        // dont change things if we dont need to
        int res = OutterLoop();

        //Debug.Log(res);

        if (res == 1)   // continue outterloop
        {
            return;
        }
        if (res == 2)   // we are about to disconnect
        {
            // check distance is positive to return to normalloop
            Vector3 currentPos = roverGameobject.transform.position;
            float currentDistance = Vector3.Distance (currentPos, destination.transform.position);
            if (currentDistance < initalHitDistance)
            {
                currentLoop = 0;
                return;
            }

            // turn to breakoff point + 1
            int newDir = roverDirection + 6;
            if (newDir >= 9)
                newDir -= 8;
            Motors.instance.RotateRover(newDir);
            initalDetachHit = false;
            currentLoop = 1;
            return;
        }

        // find first cw direction not hit
        int newDirection = 2;
        while (true)
        {
            // end case (error)
            if (newDirection == 9)
                Debug.LogError("No new direction that isn't in hit buffer");

            // check this direction
            if (!checkDirection(newDirection, 2))
                break;

            // increment direction
            newDirection++;
        }

        // update rover direction
        int overAll = roverDirection + newDirection - 1;
        if (overAll >= 9)
            overAll -= 8;

        Motors.instance.RotateRover(overAll);
        initalDetachHit = false;
    }

    int OutterLoop()
    {
        // check if our detech point hasnt hit yet
        if (!initalDetachHit)
        {
            if (checkDirection(6, 2) || checkDirection(7, 2))
                initalDetachHit = true;
        }

        // check if we hit somthing in our direction
        // check this direction
        if (checkDirection(1, 2))
            return -1;
            

        // check if we detached from object
        if (initalDetachHit)
        {
            if (!checkDirection(6, 2))
            {
                if (!checkDirection(7, 2))
                {
                    if (distances[7] > 570)
                    {
                        hitOnSlope = false;
                        return 2;
                    }
                }
            }
        }

        return 1;
    }

    void MoveTillHit()
    {
        // check if we hit somthing in our direction
        if (checkDirection(2, 2))
        {
            // turn the right direction
            int newDir = roverDirection + 1;
            if (newDir >= 9)
                newDir -= 8;
            Motors.instance.RotateRover(newDir);

            // start outter obj avoidance
            currentLoop = 1;
            return;
        }

        return;
    }

    void InnerLoop()
    {
        // set up excape plan
        if (!movingAway)
        {
            // witch magic dont worry about it
            int newDirection = innerHit - 5;
            if (newDirection < 1)
                newDirection += 8;
            newDirection += roverDirection;
            if (newDirection >= 9)
                newDirection -= 8;
            oldRoverDirection = roverDirection;

            Motors.instance.RotateRover(newDirection);
            movingAway = true;
            innerLoopTimer = 0f;
            initalDetachHit = false;
        }

        // move rover till object is outside of inner circle
        if (innerLoopTimer < innerLoopX)
        {
            innerLoopTimer += Time.deltaTime;
            return;
        }
        // return to loop that called us
        else
        {
            Motors.instance.RotateRover(oldRoverDirection);
            currentLoop = oldCurrentLoop;
            movingAway = false;
            return;
        }
    }

    private bool checkInnerLoop()
    {
        foreach (float f in distances)
        {
            if (f <= innerLoopDis)
                return true;
        }
        return false;
    }
    private bool checkMiddleLoop()
    {
        foreach (float f in distances)
        {
            if (f <= middleLoopDis)
                return true;
        }
        return false;
    }
    private bool checkOuterLoop()
    {
        foreach (float f in distances)
        {
            if (f <= outerLoopDis)
                return true;
        }
        return false;
    }

    // checks if distance in a direction is <= certian 
    private bool checkDirection(int direction, int level)
    {
        switch (level)
        {
            // inner loop
            case 1:
                if (distances[direction - 1] <= innerLoopDis)
                    return true;
                break;
            // middle loop
            case 2:
                if (distances[direction - 1] <= middleLoopDis)
                    return true;
                break;
            // outer loop
            case 3:
                if (distances[direction - 1] <= outerLoopDis)
                    return true;
                break;
        }
        return false;
    }

    public float sensorUpdateSpeed = .5f;       // updates a second
    private float sensorTimeToNextUpdate = 0f;  // time to next sensor update
    private float sensorUpdateTimer = 0f;       // this pauses main loop to sim sensor update time 
    public List<float> distances;

    public float outerLoopDis = 1200;   // in mm
    public float middleLoopDis = 600;   // in mm
    public float innerLoopDis = 160;    // in mm

    void FixedUpdate()
    {
        // check all timers that would stop the main proc
        // waiting for sensor data to update
        if (sensorUpdateTimer > 0)
        {
            sensorUpdateTimer -= Time.deltaTime;
            return;
        }
        else
            sensorTimeToNextUpdate -= Time.deltaTime;

        // rover is rotating
        if (rotating)
        {
            if (turnRight)
            {
                roverGameobject.transform.Rotate(0, 1, 0);
                rotateAmount -= 1;
                if (rotateAmount <= 0)
                {
                    rotating = false;
                }
            }
            else
            {
                roverGameobject.transform.Rotate(0, -1, 0);
                rotateAmount -= 1;
                if (rotateAmount <= 0)
                {
                    rotating = false;
                }
            }

            if (rotating == false)
                Motors.instance.StartRover(manuallySetSpeed);
            return;
        }

        // check if we need to update anything
        if (sensorTimeToNextUpdate <= 0)
        {
            Sensing.instance.UpdateIRData();
            sensorUpdateTimer = Sensing.sensingTime;
            sensorTimeToNextUpdate = sensorUpdateSpeed - sensorUpdateTimer;
            return;
        }

        // check rover rotation
        float xVal = roverGameobject.transform.localEulerAngles.x;
        if (xVal > 359)
            xVal -= 359;
        if (xVal >= 20)
            onSlope = true;
        else
        {
            // reset values
            onSlope = false;
            hitOnSlope = false;
        }

        // if youre on a slope and nothing is touching anything. go to normal loop
        if (onSlope && checkInnerLoop() && checkMiddleLoop())
        {
            currentLoop = 0;
            hitOnSlope = false;
        }

        Debug.Log("currentLoop: " + currentLoop);
        // check for inner circle hit reguardless of loop
        if (currentLoop != 3)
        {
            if (checkInnerLoop())
            {
                // what direction did we hit
                int innerHit;
                for (innerHit = 1; innerHit < 9; innerHit++)
                {
                    if (checkDirection(innerHit, 1))   // could break if somehow were not hitting anything
                        break;
                }

                // change loop
                if (currentLoop != 3)
                    oldCurrentLoop = currentLoop;
                currentLoop = 3;
                if (onSlope)
                    hitOnSlope = true;
            }
        }

        // do stuff based on currentLoop
        switch (currentLoop)
        {
            case 0:
                NormalLoop();
                break;
            case 1:
                ControlOutterLoop();
                break;
            case 2:
                MoveTillHit();
                break;
            case 3:
                InnerLoop();
                break;
        }
    }

    void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        positionDelta = new Vector3();
        distances = new List<float>();
        for (int i = 0; i < 8; i++)
            distances.Add(1200f);
        if (Motors.instance != null)
            Motors.instance.StartMotors(manuallySetSpeed);
        else
            Debug.LogError("Motors cant be started, they dont exists");
    }
}
