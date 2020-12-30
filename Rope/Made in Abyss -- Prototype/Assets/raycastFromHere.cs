using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class raycastFromHere : MonoBehaviour
{
    void LateUpdate()
    {
        Debug.DrawRay(transform.position, Vector3.up, Color.black);
    }
}
