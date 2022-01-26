using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class SensorHit : MonoBehaviour
{
    public BoxCollider boxCollider;


    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("called");
        if (gameObject.name == "destination")
            Debug.LogError("Mission Complete");

        Rover.instance.hit.Add(boxCollider);
        //SpriteShapeRenderer sRend = gameObject.GetComponent<SpriteShapeRenderer>();
        //sRend.color = Color.red;
        //Debug.Log("IM in " + gameObject.name);
        
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("uncalled");
        Rover.instance.hit.Remove(boxCollider);
        //SpriteShapeRenderer sRend = gameObject.GetComponent<SpriteShapeRenderer>();
        //sRend.color = Color.green;
        //Debug.Log("IM out " + gameObject.name);
    }
}
