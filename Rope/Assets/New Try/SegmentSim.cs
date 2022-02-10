using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentSim
{
    public PointSim startingPoint;
    public PointSim endingPoint;
    public List<PointSim> inbetweenPoints = new List<PointSim>();
    public SegmentSim(Vector3 startingPoint, Vector3 endingPoint)
    {

    }
    public void SimulateSegment(float maximumDistance)
    {
        AddPhysics();

    }
    private void AddPhysics()
    {

    }
    private void ConstrainRope()
    {

    }
    public void ResetToPreviousState()
    {

    }
    public float getCurrentLenght()
    {
        return 1;
    }
}
