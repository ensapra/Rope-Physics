using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObjec : MonoBehaviour
{
    public Vector3 maxOffset;
    public Vector3 currenty;
    public Vector3 velocity;
    public bool going;
    public bool move;
    void FixedUpdate()
    {
        if(!move)
            return;
        velocity += Vector3.up*Time.deltaTime/5;
        if(going)
        {
            transform.position += velocity*Time.deltaTime;
            currenty += velocity*Time.deltaTime;
        }
        else
        {
            transform.position -= velocity*Time.deltaTime;
            currenty -= velocity*Time.deltaTime;
        }

        if(going && (currenty.x >= maxOffset.x && currenty.y >= maxOffset.y && currenty.z >= maxOffset.z))
            going = false;
        if(!going && currenty.x <= 0 && currenty.y <= 0 && currenty.z <= 0)
            going = true;

    }
    void Start()
    {
        
    }
}
