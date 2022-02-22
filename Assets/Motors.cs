using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motors : MonoBehaviour
{
    public static Motors instance;

    private static int UPDATESPEED = 50; // 50 calls a second
    private static float MPHtoCMF = .89408f; // mph to cm / frame
    private static float CHANGESPEEDTIME = 1f; // time it takes to change speed

    private float roverSpeed;   // cm / frame
    private float changeSpeed;  // cm / frame

    private bool motorsStarted;

    void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        roverSpeed = 0;
        motorsStarted = false;
    }

    public void setRoverSpeed(float newSpeedMPH)
    {
        // set new change speed to change roverspeed over time
        changeSpeed = newSpeedMPH * MPHtoCMF / 10;
    }

    public float getRoverSpeed()
    {
        // convert back to mhp
        return roverSpeed / MPHtoCMF * 10;
    }

    public void StartMotors(float startSpeed)
    {
        setRoverSpeed(startSpeed);
        motorsStarted = true;
    }

    public void StartRover(float startSpeed)
    {
        setRoverSpeed(startSpeed);
    }

    public void StopRover()
    {
        setRoverSpeed(0);
    }

    // this is cheating but oh well
    public void InstantStop()
    {
        changeSpeed = 0;
        roverSpeed = 0;
    }

    public void RotateRover(int direction)
    {
        if (Rover.instance == null)
            return;
        InstantStop();
        int roverDirection = Rover.instance.roverDirection;
        

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
        if (count >= 4)
        {
            count = 8 - count;
            Rover.instance.turnRight = false;
        }
        else
            Rover.instance.turnRight = true;

        // set new rover direction
        Rover.instance.roverDirection = direction;

        // wait (this is for sim)
        Rover.instance.timer = 0;

        // set rotation count
        Rover.instance.rotateAmount = count * 45;
        Rover.instance.rotating = true;
        return;
    }

    public void MoveRover()
    {
        if (Rover.instance == null)
            return;

        int roverDirection = Rover.instance.roverDirection;

        // create vector for movement
        Vector3 moveVec = new Vector3();
        if (roverDirection == 1)                    // up
            moveVec = new Vector3(0, 0, roverSpeed);
        else if (roverDirection == 3)               // right
            moveVec = new Vector3(roverSpeed, 0, 0);
        else if (roverDirection == 5)               // down
            moveVec = new Vector3(0, 0, -roverSpeed);
        else if (roverDirection == 7)               // left
            moveVec = new Vector3(-roverSpeed, 0, 0);
        else if (roverDirection == 2)               // quad 1
            moveVec = new Vector3(roverSpeed / 2, 0, roverSpeed / 2);
        else if (roverDirection == 4)               // quad 4
            moveVec = new Vector3(roverSpeed / 2, 0, -roverSpeed / 2);
        else if (roverDirection == 6)               // quad 3
            moveVec = new Vector3(-roverSpeed / 2, 0, -roverSpeed / 2);
        else if (roverDirection == 8)               // quad 2
            moveVec = new Vector3(-roverSpeed / 2, 0, roverSpeed / 2);

        // move rover
        Rover.instance.roverGameobject.transform.position += moveVec;
        return;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // increase/decrease speed if needed
        if (changeSpeed > roverSpeed)
            roverSpeed += changeSpeed / (UPDATESPEED * CHANGESPEEDTIME);
        else if (changeSpeed < roverSpeed)
            roverSpeed -= changeSpeed / (UPDATESPEED * CHANGESPEEDTIME);

        if (motorsStarted)
            MoveRover();
    }
}
