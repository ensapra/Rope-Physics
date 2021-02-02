using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObjec : MonoBehaviour
{
    public Vector3 maxOffset;
    private Vector3 currenty;
    public Vector3 velocity;
    private Rigidbody rb;
    public bool going;
    public bool move;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        if(!move)
            return;
        velocity += velocity.normalized*Time.deltaTime;
        if(going)
        {
            rb.velocity = velocity;
            currenty += velocity*Time.deltaTime;
        }
        else
        {
            rb.velocity = -velocity;
            currenty -= velocity*Time.deltaTime;
        }

        if(going && (currenty.x >= maxOffset.x && currenty.y >= maxOffset.y && currenty.z >= maxOffset.z))
            going = false;
        if(!going && currenty.x <= 0 && currenty.y <= 0 && currenty.z <= 0)
            going = true;

    }
}
