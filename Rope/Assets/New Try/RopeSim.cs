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

    // Start is called before the first frame update
    void Start()
    {
        if(startingPoint != null && endingPoint != null)
        {
            PointSim startingPointSim = new PointSim(startingPoint.position, true);
            PointSim endingPositionSim = new PointSim(endingPoint.position, true);
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
        currentSegments[0].startingPoint.setStatic(originalStarting.isStatic());
    }
    private void AddSegments(int amount)
    {
        //Modify correctly initial segment
        SegmentSim intialSegment;
        intialSegment = currentSegments[0];
        PointSim originalStartingPoint = intialSegment.startingPoint;
        PointSim previousCopy = new PointSim(originalStartingPoint);
        originalStartingPoint.setStatic(false);
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
            currentSegments[0].startingPoint.UpdatePosition(startingPoint.position);

        if(endingPoint != null)
            currentSegments[currentSegments.Count-1].endingPoint.UpdatePosition(endingPoint.position);
    }
    private void SimulateSegments()
    {
        for(int z = 0; z < maximumIterations; z++)
        {
            for(int i = 0; i< currentSegments.Count; i++)
            {
                if(z == 0 && i+1 < currentSegments.Count)
                {
                    if(i == 0)
                        currentSegments[i].AddPhysics(Vector3.down*ropeGravity,ropeFlexibility);
                    currentSegments[i+1].AddPhysics(Vector3.down*ropeGravity,ropeFlexibility);
                }
                currentSegments[i].ConstrainRope(maximumDistance);
                if(z == maximumIterations-1)
                    currentSegments[i].CollisionCheck(ropeRadious,collisionLayer, groundFriction);
            }
        }
        //also for the ending point required to do
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
