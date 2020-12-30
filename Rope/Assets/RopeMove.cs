using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeMove : MonoBehaviour
{
    Rigidbody rb;
    public bool ObjectClinged = false;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        
    }
    void FixedUpdate()
    {
        if(!rb.isKinematic)
        {
            if(Physics.Raycast(transform.position+rb.velocity.normalized*0.3f, rb.velocity.normalized, rb.velocity.magnitude*Time.deltaTime))
            {
                rb.isKinematic = true;
                ObjectClinged = true;
            }
        }
    }
}
