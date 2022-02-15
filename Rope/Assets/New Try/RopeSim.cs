using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeSim : MonoBehaviour
{
    public List<SegmentSim> currentSegments = new List<SegmentSim>();
    public Transform startingPoint; 
    public Transform endingPoint;

    public float ropeGravity = 4;
    public int maximumIterations = 30;
    public float maximumDistance = 1;
    public float ropeLength = 5;
    public float ropeRadious = 0.2f;
    public float currentLenght;
    public float ropeFlexibility;
    public float groundFriction;
    public LayerMask collisionLayer;
    private LineRenderer lineRenderer;
    public int maxAmountOfPoints = 100;
    private float minDistane = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        if(startingPoint != null && endingPoint != null)
        {
            PointSim startingPointSim = new PointSim(startingPoint);
            PointSim endingPositionSim = new PointSim(endingPoint);
            SegmentSim baseSegment = new SegmentSim(startingPointSim, endingPositionSim);
            currentSegments.Add(baseSegment);
        }
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {   
        UpdateStartAndEnd();
        CreateNewSegments();
        SimulateSegments();
        Visualize();
    }
    private void CreateNewSegments()
    {
        if(maximumDistance < minDistane)
            maximumDistance = minDistane;
        int amountOfSegments = (int)(ropeLength/maximumDistance);
        int segmentDifference = amountOfSegments-currentSegments.Count;
        if(segmentDifference < 0)
            RemoveSegments(segmentDifference);
        if(segmentDifference > 0)
            AddSegments(segmentDifference);
    }
    private void RemoveSegments(int amount)
    {
        amount = amount*-1;
        PointSim originalStarting = currentSegments[0].startingPoint;
        for(int i = 0; i < amount; i++)
        {
            currentSegments.RemoveAt(0);
        }
        currentSegments[0].startingPoint.RepurposeObject(originalStarting);
    }
    private void AddSegments(int amount)
    {
        //Modify correctly initial segment
        SegmentSim intialSegment;
        if(amount > maxAmountOfPoints)
            amount = maxAmountOfPoints;
        intialSegment = currentSegments[0];
        PointSim originalStartingPoint = intialSegment.startingPoint;
        PointSim previousCopy = new PointSim(originalStartingPoint);
        originalStartingPoint.RepurposeObject(new PointSim(originalStartingPoint.getPosition(), false));
        for(int i = 0; i < amount-1; i++)
        {
            PointSim currentCopy = new PointSim(originalStartingPoint);
            SegmentSim newStep = new SegmentSim(previousCopy, currentCopy);
            previousCopy = currentCopy;
            currentSegments.Insert(i,newStep);
        }
        SegmentSim finalStep = new SegmentSim(previousCopy, originalStartingPoint);
        currentSegments.Insert(amount-1, finalStep);
    }
    private void UpdateStartAndEnd()
    {
        if(startingPoint != null)
            currentSegments[0].startingPoint.UpdatePosition();

        if(endingPoint != null)
            currentSegments[currentSegments.Count-1].endingPoint.UpdatePosition();
    }
    private void SimulateSegments()
    {
        //First do a pass where we will set the final position of the rope without collisions taking into account physics
        for(int z = 0; z < maximumIterations/2; z++)
        {
            for(int i = 0; i< currentSegments.Count; i++)
            {
                SegmentSim selected = currentSegments[i];
                if(z == 0 && i+1 < currentSegments.Count)
                {
                    if(i == 0)
                        selected.AddPhysics(Vector3.down*ropeGravity,ropeFlexibility);
                    currentSegments[i+1].AddPhysics(Vector3.down*ropeGravity,ropeFlexibility);
                }
                selected.ConstrainRopeFuture(maximumDistance);
            }
        }
        //Second pass will set the rope taking into account collisions
        for(int z = 0; z < maximumIterations/2; z++)
        {
            for(int i = 0; i< currentSegments.Count; i++)
            {
                SegmentSim selected = currentSegments[i];
                selected.ConstrainRope(maximumDistance);
                selected.CollisionCheck(ropeRadious,collisionLayer, groundFriction);
                if(z == maximumIterations/2-1)
                    selected.ApplyForces(collisionLayer);
            }
        }

        for(int i = 0; i < currentSegments.Count; i++)
        {
            currentSegments[i].VisualizeFuture();
            currentSegments[i].Visualize();
        }
    }
    private void Visualize()
    {
        List<Vector3> points = new List<Vector3>();
        for(int i = 0; i< currentSegments.Count; i++)
        {
            SegmentSim segment = currentSegments[i];
            points.AddRange(segment.getPoints());
            if(i == currentSegments.Count-1)
                points.Add(segment.endingPoint.getPosition());
        }
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
}
