using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderReaction : MonoBehaviour
{
    Rigidbody rb;
    public bool climable;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        
    }
    void FixedUpdate()
    {
        if(rb.isKinematic == false && rb.IsSleeping())
        {
            rb.isKinematic = true;
        }
    }
}
