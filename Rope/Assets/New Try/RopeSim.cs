using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeSim : MonoBehaviour
{
    public List<SegmentSim> currentSegments = new List<SegmentSim>();
    public Transform startingPoint; 
    public Transform endingPoint;

    public float maximumDistance = 1;
    public float ropeLength = 5;
    public float currentLenght;
    // Start is called before the first frame update
    void Start()
    {
        SegmentSim baseSegment = new SegmentSim(startingPoint.position, endingPoint.position);
    }

    // Update is called once per frame
    void FixedUpdate()
    {   
        CreateNewSegments();
        SimulateSegments();
    }
    private void CreateNewSegments()
    {
        float amountOfSegments = (ropeLength/maximumDistance);
    }
    private void SimulateSegments()
    {
        ResetToPreviousState();
        foreach(SegmentSim segmentSim in currentSegments)
        {
            segmentSim.SimulateSegment(maximumDistance);
        }
    }
    private void ResetToPreviousState()
    {
        foreach(SegmentSim segmentSim in currentSegments)
        {
            segmentSim.ResetToPreviousState();
        }
    }
}
