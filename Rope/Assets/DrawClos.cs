using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawClos : MonoBehaviour
{
    void Start()
    {
        
    }
    void Update()
    {
        Physics.queriesHitBackfaces =true;
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1);
        for(int i = 0; i < colliders.Length; i++)
        {
            Debug.DrawLine(transform.position, colliders[i].ClosestPointOnBounds(transform.position));
        }
    }
}
