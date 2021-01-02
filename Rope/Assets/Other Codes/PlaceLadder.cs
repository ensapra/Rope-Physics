using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceLadder : MonoBehaviour
{
    public PlayerControl control;
    public bool placeLadder;
    private bool placed;
    public GameObject ladderObject;
    public GameObject loded;
    private float yValue;
    public void Start()
    {
        control = GetComponent<PlayerControl>();
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
            placeLadder = !placeLadder;
        if(placeLadder)
        {
            yValue += Input.GetAxis("Scroll");
        }
        else
        {
            yValue =1;
        }
    }
    void FixedUpdate()
    {
        if(placeLadder)
        {
            if(loded == null)
            {
                loded = Instantiate(ladderObject, transform.position+transform.forward+transform.up,transform.rotation, transform);
                BoxCollider box = loded.GetComponent<BoxCollider>();
                box.isTrigger = true;
                Rigidbody boxRB = box.GetComponent<Rigidbody>();
                boxRB.isKinematic = true;
            }
            loded.transform.localScale = new Vector3(1,yValue,0.2f);
            loded.transform.localPosition = new Vector3(0,yValue/2,1);
        }
        else
        {
            if(loded != null)
            {
                BoxCollider box = loded.GetComponent<BoxCollider>();
                box.isTrigger = false;
                Rigidbody boxRB = box.GetComponent<Rigidbody>();
                boxRB.isKinematic = false;
                boxRB.AddForceAtPosition(transform.position+box.transform.localScale.y/2*Vector3.up, transform.forward*500);
                loded.transform.SetParent(null);
                loded = null;
            }
        }
    }
}
