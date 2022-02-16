using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeSim : MonoBehaviour
{
    public List<SegmentSim> currentSegments = new List<SegmentSim>();
    public Transform startingPoint; 
    public Vector3 startOffset;
    public Transform endingPoint;
    public Vector3 endingOffset;

    public float ropeGravity = 4;
    public int maximumIterations = 30;
    public float currentLenght;
    public float ropeLength = 5;
    [Range(0.05f, 1f)] public float ropeRadious = 0.2f;
    public float ropeFlexibility;
    public float groundFriction;
    public LayerMask collisionLayer;
    private LineRenderer lineRenderer;
    public int maxAmountOfPoints = 100;
    private float minmaxDistaneAccepted = 0.02f;
    public float maxTension;
    public float currentTension;
    public Vector2 distanceMinMax = new Vector2(0,1);

    // Start is called before the first frame update
    void Start()
    {
        PointSim startingPointSim = startingPoint != null ? new PointSim(startingPoint, startOffset): new PointSim(transform.position, false);
        PointSim endingPositionSim = endingPoint != null ? new PointSim(endingPoint, endingOffset) :new PointSim(transform.position+Vector3.forward*ropeLength, false);
        SegmentSim baseSegment = new SegmentSim(startingPointSim, endingPositionSim);
        currentSegments.Add(baseSegment);
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
        if(distanceMinMax.y < minmaxDistaneAccepted)
            distanceMinMax.y = minmaxDistaneAccepted;
        currentLenght = GetRopeLength();
        currentTension = currentLenght/ropeLength-1;
        int amountOfSegments = (int)(ropeLength/distanceMinMax.y)+1;
        int segmentDifference = amountOfSegments-currentSegments.Count;
        if(segmentDifference < 0)
            RemoveSegments(segmentDifference);
        if(segmentDifference > 0)
            AddSegments(segmentDifference);
    }
    private float GetRopeLength()
    {
        float Lenght = 0;
        foreach(SegmentSim segment in currentSegments)
        {
            Lenght += segment.getCurrentLenght();
        }
        return Lenght;
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
        currentSegments[0].startingPoint.UpdatePosition(startingPoint, startOffset);
        currentSegments[currentSegments.Count-1].endingPoint.UpdatePosition(endingPoint, endingOffset);
    }
    private void SimulateSegments()
    {
        int segmentsCount = currentSegments.Count;
        float distanceEdges = (ropeLength-distanceMinMax.y*(segmentsCount-1));
        distanceEdges = Mathf.Clamp(distanceEdges, 0, distanceMinMax.y);
        float extraDistance = (currentLenght-ropeLength)/(segmentsCount-1);

        //First do a pass where we will set the final position of the rope without collisions taking into account physics
        for(int z = 0; z < maximumIterations/2; z++)
        {
            for(int i = 0; i< segmentsCount; i++)
            {
                SegmentSim selected = currentSegments[i];
                if(z == 0 && i+1 < segmentsCount)
                {
                    if(i == 0)
                        selected.AddPhysics(Vector3.down*ropeGravity,ropeFlexibility);
                    currentSegments[i+1].AddPhysics(Vector3.down*ropeGravity,ropeFlexibility);
                    if(i+1 == segmentsCount-1)
                         currentSegments[i+1].endingPoint.UpdatePhysics(Vector3.down*ropeGravity,ropeFlexibility);
                }
                if(i == 0)
                    selected.ConstrainRopeFuture(distanceEdges, 0);
                else
                    selected.ConstrainRopeFuture(distanceMinMax.y-extraDistance, distanceMinMax.x);
            }
        }
        //Second pass will set the rope taking into account collisions
        for(int z = 0; z < maximumIterations/2; z++)
        {
            for(int i = 0; i< segmentsCount; i++)
            {
                SegmentSim selected = currentSegments[i];
                if(i == 0)
                    selected.ConstrainRope(distanceEdges, 0);
                else
                    selected.ConstrainRope(distanceMinMax.y-extraDistance, distanceMinMax.x);
                
                selected.CollisionCheck(ropeRadious,collisionLayer, groundFriction);
                if(i == segmentsCount-1)
                    selected.endingPoint.CollisionCheck(ropeRadious, collisionLayer, groundFriction);
                
                if(z == maximumIterations/2-1)
                    selected.ApplyForces(collisionLayer);
                if(i==segmentsCount-1)
                    selected.endingPoint.ApplyForcesToRigidbodies(collisionLayer);
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
        lineRenderer.widthMultiplier = ropeRadious;
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
