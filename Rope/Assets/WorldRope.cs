using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldRope : MonoBehaviour
{
    RopeGenerator2 ropeGenerator;
    public GameObject point1;
    public GameObject point2;
    public float length;
    void Start()
    {
        ropeGenerator = GetComponent<RopeGenerator2>();
    }
    void FixedUpdate()
    {
        ropeGenerator.GenerateTwoPointRope(point1.transform.position, point2.transform.position, length);
    }
}
