using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Segment
{
    public List<InertiaPoint> inBetween = new List<InertiaPoint>();
    public List<MiddlePoint> middlePoints = new List<MiddlePoint>();
    public Edge startingEdge;
    public Edge endingEdge;
    public float realLength;
    public float segmentLenght;
}
[System.Serializable]
public struct MiddlePoint
{
    public Edge edge;
    public int i;
    public MiddlePoint(Edge edge, int i)
    {
        this.edge = edge;
        this.i = i;
    }
}
[System.Serializable]
public struct InertiaPoint
{
    public Vector3 point;
    public Vector3 velocity;
    public InertiaPoint(Vector3 point, Vector3 velocity)
    {
        this.point = point;
        this.velocity = velocity;
    }
}

