using System.Collections;
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
    public float distance;

    void Start()
    {
        initalPosition = gameObject.transform.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        // return if sensing isnt created yet
        if (Sensing.instance == null)
            return;

        //Debug.Log(gameObject.name);
        if (other.gameObject.name == "Destination")
            Debug.LogError("Mission Complete");


        Sensing.instance.hit.Add(this);
        //SpriteShapeRenderer sRend = gameObject.GetComponent<SpriteShapeRenderer>();
        //sRend.color = Color.red;
        //Debug.Log("IM in " + gameObject.name);
        
    }

    private void OnTriggerExit(Collider other)
    {
        // return if sensing isnt created yet
        if (Sensing.instance == null)
            return;

        Sensing.instance.hit.Remove(this);
        //SpriteShapeRenderer sRend = gameObject.GetComponent<SpriteShapeRenderer>();
        //sRend.color = Color.green;
        //Debug.Log("IM out " + gameObject.name);
    }
}
