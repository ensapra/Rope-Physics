using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SegmentSim
{
    [SerializeReference] public PointSim startingPoint;
    [SerializeReference] public PointSim endingPoint;
    public float segmentTension;
    public float currentAcomulatedTension;
    public List<PointSim> inbetweenPoints = new List<PointSim>();
    public SegmentSim(PointSim startingPoint, PointSim endingPoint)
    {
        this.startingPoint = startingPoint;
        this.endingPoint = endingPoint;
    }
    public void AddPhysics(Vector3 gravity, float airFriction)
    {
        startingPoint.UpdatePhysics(gravity, airFriction);
    }
    public void ApplyForces(LayerMask ropeLayers)
    {
        startingPoint.ApplyForcesToRigidbodies(ropeLayers);
    }
    public bool breakRope(float maxTension, float maxAcomulatedTension)
    {
        if(segmentTension > maxTension)
            currentAcomulatedTension += segmentTension*0.1f;
        else
            currentAcomulatedTension = 0;

        if(currentAcomulatedTension > maxAcomulatedTension)
            return true;
        else
            return false;
    }
    public void ConstrainRope(float maximumDistance, float minimumDistance)
    {
        Vector3 starting = startingPoint.getPosition();
        Vector3 ending = endingPoint.getPosition();
        Vector3 forwardDirection = ending-starting;
        float distance = Vector3.Distance(starting,ending);

        if(distance > maximumDistance)
        {
            startingPoint.ConstrainPosition((distance-maximumDistance)*(forwardDirection).normalized);  
            endingPoint.ConstrainPosition(-(distance-maximumDistance)*(forwardDirection).normalized); 
        }        
        if(distance < minimumDistance)
        {
            startingPoint.ConstrainPosition((distance-minimumDistance)*(forwardDirection).normalized);  
            endingPoint.ConstrainPosition(-(distance-minimumDistance)*(forwardDirection).normalized); 
        }    
    }
    public void ConstrainRopeFuture(float maximumDistance, float minimumDistance)
    {
        Vector3 starting = startingPoint.getFuturePosition();
        Vector3 ending = endingPoint.getFuturePosition();
        Vector3 forwardDirection = ending-starting;
        float distance = Vector3.Distance(starting,ending);

        if(distance > maximumDistance)
        {
            startingPoint.ConstrainFuture((distance-maximumDistance)*(forwardDirection).normalized);  
            endingPoint.ConstrainFuture(-(distance-maximumDistance)*(forwardDirection).normalized); 
        }        
        if(distance < minimumDistance)
        {
            startingPoint.ConstrainFuture((distance-minimumDistance)*(forwardDirection).normalized);  
            endingPoint.ConstrainFuture(-(distance-minimumDistance)*(forwardDirection).normalized); 
        }    
    }
    public void CollisionCheck(float ropeRadious, LayerMask ropeLayers, float groundFriction)
    {
        startingPoint.CollisionCheck(ropeRadious, ropeLayers, groundFriction);
        endingPoint.CollisionCheck(ropeRadious, ropeLayers, groundFriction);
    }
    public float getCurrentLenght(float maxDistance)
    {
        float distance = (startingPoint.getFuturePosition()-endingPoint.getFuturePosition()).magnitude;
        segmentTension = (distance/maxDistance)-1;
        return distance;
    }
    public List<Vector3> getPoints()
    {
        List<Vector3> current = new List<Vector3>(){startingPoint.getPosition()};
        foreach(PointSim pointSim in inbetweenPoints)
        {
            current.Add(pointSim.getPosition());
        }
        return current;
    }
    public void Visualize()
    {
        Debug.DrawLine(startingPoint.getPosition(), endingPoint.getPosition(), Color.red);
        Debug.DrawRay(startingPoint.getPosition(), Vector3.up, Color.green);
    }
    public void VisualizeFuture()
    {
        Debug.DrawLine(startingPoint.getFuturePosition(), endingPoint.getFuturePosition(), Color.blue);
        Debug.DrawRay(startingPoint.getFuturePosition(), Vector3.up, Color.green);
    }
}
