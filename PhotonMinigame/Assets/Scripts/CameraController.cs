using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform targetLook;   //target to look at
    public Transform targetPos;    //target position to move to

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        

        //lerp towards target position
        var newPos = Vector3.Lerp(transform.position, targetPos.position, .1f);
        transform.position = newPos;
        //look at target
        transform.LookAt(targetLook, ShipController.LocalDir(targetPos, Vector3.up));

    }
}
