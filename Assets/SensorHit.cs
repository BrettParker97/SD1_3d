﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class SensorHit : MonoBehaviour
{
    public BoxCollider boxCollider;
    public Vector3 initalPosition;

    public int direction;
    public int col;
    public int row;

    void Start()
    {
        initalPosition = gameObject.transform.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(gameObject.name);
        if (other.gameObject.name == "Destination")
            Debug.LogError("Mission Complete");


        Rover.instance.hit.Add(boxCollider);
        //SpriteShapeRenderer sRend = gameObject.GetComponent<SpriteShapeRenderer>();
        //sRend.color = Color.red;
        //Debug.Log("IM in " + gameObject.name);
        
    }

    private void OnTriggerExit(Collider other)
    {
        Rover.instance.hit.Remove(boxCollider);
        //SpriteShapeRenderer sRend = gameObject.GetComponent<SpriteShapeRenderer>();
        //sRend.color = Color.green;
        //Debug.Log("IM out " + gameObject.name);
    }
}
