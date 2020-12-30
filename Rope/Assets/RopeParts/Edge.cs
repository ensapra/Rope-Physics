using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Edge
{
    static int amount = 0;
    public Transform realPosition;

    public Vector3 normal1;
    public Vector3 normal2;
    private Transform realTransform;
    static List<Transform> enabled = new List<Transform>();
    static List<Transform> disabled = new List<Transform>();
    static Transform genericDeads;
    private Vector3 tempPoint;
    private Vector3 in_velocity;

    public Vector3 point
    {get{if(realPosition != null)
        return realPosition.transform.position;
    else
        return tempPoint;}
    set{
        if(realPosition == null)
            GeneratePoint();
        in_velocity += value-realPosition.position;
        realPosition.position = value;
    }
    }
    public Vector3 velocity
    {get {return in_velocity;}}
    public InertiaPoint inertiaPoint{get{return new InertiaPoint(point, velocity);}}
    
    public Edge(Vector3 point, Vector3 normal1,Vector3 normal2, Transform realTransform)
    {
        this.in_velocity = Vector3.zero;
        this.tempPoint = point;
        this.normal1 = normal1;
        this.normal2 = normal2;
        this.realTransform = realTransform;
        if(genericDeads == null)
            genericDeads = new GameObject("Dead Edges Parent").transform;
        if(realTransform == null)
            GeneratePoint();
    }
    public Edge(Rigidbody rb, Vector3 normal1,Vector3 normal2)
    {
        this.in_velocity = rb.velocity;
        this.tempPoint = rb.transform.position;
        this.normal1 = normal1;
        this.normal2 = normal2;
        this.realTransform = rb.transform;
        if(genericDeads == null)
            genericDeads = new GameObject("Dead Edges Parent").transform;
        if(realTransform == null)
            GeneratePoint();
    }
    public void GeneratePoint()
    {
        if(this.realPosition != null)
            return;
        if(disabled.Count <= 0)
        {
            amount += 1;
            this.realPosition = new GameObject("TEdge-" + amount).transform;
        }
        else
        {
            this.realPosition = disabled[0];
            disabled.RemoveAt(0);
        }
        this.realPosition.gameObject.SetActive(true);
        if(realTransform != null)
            this.realPosition.SetParent(realTransform);
        else
        {
            this.realPosition.SetParent(genericDeads);
            this.realPosition.name += " Punta";
        }
        this.realPosition.position = tempPoint;
        enabled.Add(this.realPosition);
    }
    public void DestroyPoint()
    {
        this.realPosition.gameObject.SetActive(false);
        this.realPosition.SetParent(genericDeads);
        enabled.Remove(this.realPosition);
        disabled.Add(this.realPosition);
    }
}
