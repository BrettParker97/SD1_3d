using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rover : MonoBehaviour
{
    public static Rover instance;
    public GameObject roverGameobject;
    public GameObject roverBody;
    public GameObject destination;

    public List<BoxCollider> hit = new List<BoxCollider>();

    public List<BoxCollider> inner;
    public List<BoxCollider> outter;
    public float speed = 0.04f;

    private Vector3 roverSpeed = new Vector3();
    private int roverDirection = 1;
    // 0 = normal, 1 = outter, 2 = inner
    public int currentLoop = 0;
    private Vector3 initalHitPosition;
    public float initalHitDistance;
    public bool initalDetachHit = false;

    private Vector3 initalRoverPosition;
    float timer = 1;

    public int innerHit;
    public bool movingAway = false;
    public int oldRoverDirection;
    public int oldCurrentLoop;
    public float innerLoopTimer;
    public float innerLoopX = 0.1f;

    int changeCounter = 0;
    public Vector3 positionDelta;

    void RotateRover(int direction)
    {
        bool turnRight;

        if (direction == roverDirection)
            return;

        // right turn
        int temp = roverDirection;
        int count = 0;
        while (temp != direction)
        {
            temp++;
            count++;
            if (temp >= 9)
                temp -= 8;
        }

        // if right turn > 4 then turn left instead
        if (count > 4)
        {
            count = 8 - count;
            turnRight = false;
        }    
        else
            turnRight = true;

        // rotate the damn rover
        if (turnRight)
            roverGameobject.transform.Rotate(0, (count * 45), 0);
        else
            roverGameobject.transform.Rotate(0, -(count * 45), 0);

        // set new rover direction
        
        roverDirection = direction;

        // wait 
        timer = 0;
    }

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

        if (changeCounter <= 5)
            return;
            
        // rotate rover
        RotateRover(best);
        return;
    }

    void MoveRover()
    {

        // create vector for movement
        Vector3 moveVec = new Vector3();
        if (roverDirection == 1)                    // up
            moveVec = new Vector3(0, 0, speed);
        else if (roverDirection == 3)               // right
            moveVec = new Vector3(speed, 0, 0);
        else if (roverDirection == 5)               // down
            moveVec = new Vector3(0, 0, -speed);
        else if (roverDirection == 7)               // left
            moveVec = new Vector3(-speed, 0, 0);
        else if (roverDirection == 2)               // quad 1
            moveVec = new Vector3(speed/2, 0, speed / 2);
        else if (roverDirection == 4)               // quad 4
            moveVec = new Vector3(speed / 2, 0, -speed / 2);
        else if (roverDirection == 6)               // quad 3
            moveVec = new Vector3(-speed / 2, 0, -speed / 2);
        else if (roverDirection == 8)               // quad 2
            moveVec = new Vector3(-speed / 2, 0, speed / 2);

        // move rover
        roverGameobject.transform.position += moveVec;
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
                if (hit.Contains(outter[0]) || hit.Contains(outter[1]) || hit.Contains(outter[2]))
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
            if (hit.Contains(outter[0]) || hit.Contains(outter[1]) || hit.Contains(outter[2]))
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

        // move rover if not hitting something
        MoveRover();

        return;
    }

    void ControlOutterLoop()
    {
        // dont change things if we dont need to
        int res = OutterLoop();
        if (res == 1)   // continue outterloop
            return;
        if (res == 2)   // we just disconnected
        {
            // check distance is positive to return to normalloop
            Vector3 currentPos = roverGameobject.transform.position;
            float currentDistance = Vector3.Distance (currentPos, destination.transform.position);
            if (currentDistance < initalHitDistance)
            {
                currentLoop = 0;
                return;
            }

            // turn to last connection - 1 and keep running outterloop
            int newDir = 1;
            if (roverDirection - 4 > 0)
                newDir = roverDirection - 4;
            else
                newDir = roverDirection - 4 + 8;
            RotateRover(newDir);
            initalDetachHit = false;

            currentLoop = 2;
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
            int left = (newDirection * 3) - 3;
            int mid = (newDirection * 3) - 2;
            int right = (newDirection * 3) - 1;
            if (!hit.Contains(outter[left]) || !hit.Contains(outter[mid]) || !hit.Contains(outter[right]))
                break;

            // increment direction
            newDirection++;
        }

        // update rover direction
        int overAll = roverDirection + newDirection - 1;
        if (overAll >= 9)
            overAll -= 8;

        RotateRover(overAll);
        initalDetachHit = false;
    }

    int OutterLoop()
    {
        int left, mid, right;

        // check if our detech point hasnt hit yet
        if (!initalDetachHit)
        {
            // check this direction
            // 6 is direction we are checking
            // 3 for amount of boxes per direction
            // 1,2,3 for array offset
            left = (6 * 3) - 3;
            mid = (6 * 3) - 2;
            right = (6 * 3) - 1;
            if (hit.Contains(outter[left]) || hit.Contains(outter[mid]) || hit.Contains(outter[right]))
                initalDetachHit = true;
        }

        MoveRover();
        // check if we hit somthing in our direction
        // check this direction
        left = (1 * 3) - 3;
        mid = (1 * 3) - 2;
        right = (1 * 3) - 1;
        if (hit.Contains(outter[left]) || hit.Contains(outter[mid]) || hit.Contains(outter[right]))
            return -1;

        // check if we detached from object
        if (initalDetachHit)
        {
            left = (6 * 3) - 3;
            mid = (6 * 3) - 2;
            right = (6 * 3) - 1;
            if (!hit.Contains(outter[left]) && !hit.Contains(outter[mid]) && !hit.Contains(outter[right]))
            {
                left = (7 * 3) - 3;
                mid = (7 * 3) - 2;
                right = (7 * 3) - 1;
                if (!hit.Contains(outter[left]) && !hit.Contains(outter[mid]) && !hit.Contains(outter[right]))
                {
                    left = (8 * 3) - 3;
                    mid = (8 * 3) - 2;
                    right = (8 * 3) - 1;
                    if (!hit.Contains(outter[left]) && !hit.Contains(outter[mid]) && !hit.Contains(outter[right]))
                    {
                        Debug.Log("here3");
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
        int left = (2 * 3) - 3;
        int mid = (2 * 3) - 2;
        int right = (2 * 3) - 1;
        if (hit.Contains(outter[left]) || hit.Contains(outter[mid]) || hit.Contains(outter[right]))
        {
            // turn the right direction
            int newDir = roverDirection + 2;
            if (newDir >= 9)
                newDir -= 8;
            RotateRover(newDir);

            // start outter obj avoidance
            currentLoop = 1;
            return;
        }

        // move rover if not hitting something
        MoveRover();
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

            RotateRover(newDirection);
            movingAway = true;
            innerLoopTimer = 0f;
            initalDetachHit = false;
        }

        // move rover till object is outside of inner circle
        if (innerLoopTimer < innerLoopX)
        {
            MoveRover();
            innerLoopTimer += Time.deltaTime;
            return;
        }
        // return to loop that called us
        else
        {
            RotateRover(oldRoverDirection);
            Debug.Log(currentLoop+" curr " + oldCurrentLoop);
            currentLoop = oldCurrentLoop;
            movingAway = false;
            return;
        }
    }

    public bool onSlope = false;
    public bool hitOnSlope = false;

    void FixedUpdate()
    {
        Debug.Log(currentLoop);

        // dont run a loop if we need to wait for 
        // an update
        if (timer < .3)
        {
            timer += Time.deltaTime;
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
            Debug.Log("here1");
            // reset values
            onSlope = false;
            hitOnSlope = false;
        }

        // if youre on a slope and nothing is touching anything. go to normal loop
        if (onSlope && hit.Count <= 0)
        {
            currentLoop = 0;
            Debug.Log("here2");
            hitOnSlope = false;
        }    

        // check for inner circle hit reguardless of loop
        if (currentLoop != 3)
        {
            foreach (BoxCollider bc in hit)
            {
                for (int i = 0; i < inner.Count; i++)
                {
                    if (bc == inner[i])
                    {
                        innerHit = Mathf.CeilToInt((i + 1) / 3);
                        if (currentLoop != 3)
                            oldCurrentLoop = currentLoop;
                        currentLoop = 3;
                        if (onSlope)
                            hitOnSlope = true;
                    }
                }
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

        initalRoverPosition = roverBody.transform.position;
        positionDelta = new Vector3();
    }
}
