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
    public float maxAcomulatedTension;
    public float currentTension;
    public Vector2 distanceMinMax = new Vector2(0,1);
    public int VisualizeIteration = -1;
    public bool overallDebug;
    private float delay;
    private bool justBroke;
    public bool canBreak;
    public RopeSim attachRope;
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
        if(canBreak)
            CheckBreakPoints();
        CheckAttachment();
        Visualize();
    }
    public void CheckAttachment()
    {
        if(attachRope != null)
        {
            this.ropeLength += attachRope.ropeLength;
            this.endingPoint = attachRope.endingPoint;
            this.endingOffset = attachRope.endingOffset;
            Destroy(attachRope.gameObject);
            attachRope = null;
        }
    }
    public void CheckBreakPoints()
    {
        if(justBroke)
        {
            delay += Time.deltaTime;
            if(delay < 2f)
                return;
            else
            {
                delay = 0;
                justBroke = false;
            }
        }
        int breakingPoints = -1;
        int brokenPoints = 0;
        for(int i = 0; i < currentSegments.Count; i++)
        {
            SegmentSim segmentSim = currentSegments[i];
            if(segmentSim.breakRope(maxTension, maxAcomulatedTension))
            {
                breakingPoints += i;
                brokenPoints++;
            }
        }
        if(breakingPoints != -1)
            BreakRope((int)breakingPoints/brokenPoints);
    }
    private void BreakRope(int placeOfBreak)
    {
        this.justBroke = true;
        GameObject newRope = Instantiate(this.gameObject);
        RopeSim newRopeSim = newRope.GetComponent<RopeSim>();
        newRopeSim.currentSegments = new List<SegmentSim>();
        newRopeSim.startingPoint = null;
        newRopeSim.ropeLength = ropeLength-placeOfBreak*distanceMinMax.y;
        this.endingPoint = null;
        this.ropeLength = placeOfBreak*distanceMinMax.y;

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
            Lenght += segment.getCurrentLenght(distanceMinMax.y);
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
        for(int z = 0; z < maximumIterations/4; z++)
        {
/*             //Single pass
            for(int i = 0; i< segmentsCount; i++)
            {
                SegmentSim selected = currentSegments[i];
                SegmentSimulation(selected, i, extraDistance, distanceEdges, z, segmentsCount);
            }  */
            //Full two way
            for(int i = 0; i< segmentsCount; i++)
            {
                SegmentSim selected = currentSegments[i];
                SegmentSimulation(selected, i, extraDistance, distanceEdges, z, segmentsCount);
            } 
            for(int i = segmentsCount-1; i>= 0; i--)
            {
                SegmentSim selected = currentSegments[i];
                SegmentSimulation(selected, i, extraDistance, distanceEdges, z, segmentsCount);
            } 
//Center to extremes
/*             for(int i = segmentsCount/2; i< segmentsCount; i++)
            {
                SegmentSim selected = currentSegments[i];
                SegmentSimulation(selected, i, extraDistance, distanceEdges, z, segmentsCount);
            } 
            for(int i = segmentsCount/2; i>= 0; i--)
            {
                SegmentSim selected = currentSegments[i];
                SegmentSimulation(selected, i, extraDistance, distanceEdges, z, segmentsCount);
            }  */
            //Extremes to center (Looks better than the others, without weird vibrations)
/*              for(int i = 0; i< segmentsCount/2; i++)
            {
                SegmentSim selected = currentSegments[i];
                SegmentSimulation(selected, i, extraDistance, distanceEdges, z, segmentsCount);
            } 
            for(int i = segmentsCount-1; i>= segmentsCount/2; i--)
            {
                SegmentSim selected = currentSegments[i];
                SegmentSimulation(selected, i, extraDistance, distanceEdges, z, segmentsCount);
            }   */
        }

        if(overallDebug)
        {
            for(int i = 0; i < currentSegments.Count; i++)
            {
                currentSegments[i].VisualizeFuture();
                currentSegments[i].Visualize();
            }
        }
    }
    private void SegmentSimulation(SegmentSim segment, int i, float extraDistance, float distanceEdges, int z, int segmentsCount)
    {
        if(i == 0)
            segment.ConstrainRope(distanceEdges, 0);
        else
            segment.ConstrainRope(distanceMinMax.y-extraDistance, distanceMinMax.x);
        segment.CollisionCheck(ropeRadious,collisionLayer, groundFriction);
        if(z == maximumIterations/4-1)
            segment.ApplyForces(collisionLayer);
        if(i==segmentsCount-1)
            segment.endingPoint.ApplyForcesToRigidbodies(collisionLayer);
        if(z == VisualizeIteration)
        {
            Debug.DrawLine(segment.startingPoint.getPosition(), segment.endingPoint.getPosition(), Color.yellow);
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
