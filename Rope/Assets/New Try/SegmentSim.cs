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
    public void CreateCorners(float ropeRadious, LayerMask collisionLayer)
    {
        FindCorners(ropeRadious, collisionLayer);
    }
    public void ConstrainRope(float maximumDistance, float minimumDistance)
    {
        Vector3 starting = startingPoint.getPosition();
        Vector3 startEnd = inbetweenPoints.Count > 0 ? inbetweenPoints[0].getPosition() : endingPoint.getPosition();

        Vector3 endStart = inbetweenPoints.Count > 0 ? inbetweenPoints[inbetweenPoints.Count-1].getPosition() : startingPoint.getPosition();
        Vector3 ending = endingPoint.getPosition();
        
        Vector3 startDirection = startEnd-starting;
        Vector3 endingDirection = endStart-ending;

        float distance = Vector3.Distance(starting,startEnd);
        for(int i = 0; i < inbetweenPoints.Count; i++)
        {
            starting = inbetweenPoints[i].getPosition();
            startEnd = i+1 < inbetweenPoints.Count ? inbetweenPoints[i+1].getPosition() : endingPoint.getPosition();
            distance += Vector3.Distance(starting,startEnd);
        }
        if(distance > maximumDistance)
        {
            startingPoint.ConstrainPosition((distance-maximumDistance)*(startDirection).normalized);  
            endingPoint.ConstrainPosition((distance-maximumDistance)*(endingDirection).normalized); 
        }        
        if(distance < minimumDistance)
        {
            startingPoint.ConstrainPosition((distance-minimumDistance)*(startDirection).normalized);  
            endingPoint.ConstrainPosition((distance-minimumDistance)*(endingDirection).normalized); 
        }    
    }
    private void FindCorners(float ropeRadious, LayerMask collisionLayer)
    {
        PointSim startCheck = startingPoint;
        PointSim nextCheck = inbetweenPoints.Count != 0 ? inbetweenPoints[0] : endingPoint;
        PointSim initialPoint = CheckCorner(startCheck, nextCheck, ropeRadious, collisionLayer);
        if(initialPoint != null)
        {
            if(inbetweenPoints.Count > 30)
                return;
            inbetweenPoints.Add(initialPoint);
            return;
        }
        else 
        {           
            if(Vector3.Distance(startCheck.getPosition(), nextCheck.getPosition()) < 0.1f && inbetweenPoints.Count >0)
            {
                inbetweenPoints.RemoveAt(0);
                return;
            }
        }
        for(int i = 0; i<inbetweenPoints.Count; i++)
        {
            startCheck = inbetweenPoints[i];
            nextCheck = i+1 < inbetweenPoints.Count ? inbetweenPoints[i+1] : endingPoint;
            PointSim foundCorner = CheckCorner(startCheck, nextCheck, ropeRadious, collisionLayer);
            if(foundCorner != null)
            {
                if(inbetweenPoints.Count > 30)
                    return;
                inbetweenPoints.Add(foundCorner);
                break;
            }
            else
            {
                if(Vector3.Distance(startCheck.getPosition(), nextCheck.getPosition()) < 0.1f)
                {
                    inbetweenPoints.RemoveAt(i);
                    break;
                }
                PointSim previousPoint = i == 0 ? startingPoint : inbetweenPoints[i-1];
                PointSim cornerExists = CheckCorner(previousPoint, nextCheck, ropeRadious, collisionLayer);
                if(cornerExists != null)
                    inbetweenPoints[i].ChangePosition(cornerExists.getPosition());
                else
                {
                    inbetweenPoints.RemoveAt(i);
                    break;
                }
            }
        }
    }
    private PointSim CheckCorner(PointSim startingPoint, PointSim finalPoint, float ropeRadious, LayerMask collisionLayer)
    {
        RaycastHit startHit;
        RaycastHit endHit;
        Vector3 startPosition = startingPoint.getPosition();
        Vector3 endingPosition = finalPoint.getPosition();
        Debug.DrawLine(startPosition, endingPosition);
        Vector3 direction = (endingPosition-startPosition);
        if(direction.magnitude < 0.1f)
            return null;
        Vector3 newCorner;
        Vector3 finalNormal = Vector3.zero;
        if(Physics.SphereCast(startPosition,ropeRadious, direction, out startHit, direction.magnitude, collisionLayer))
        {
            Plane startPlane = new Plane(startHit.normal, startHit.point+startHit.normal*ropeRadious);
            finalNormal += startHit.normal;
            if(Physics.SphereCast(endingPosition,ropeRadious, -direction, out endHit, direction.magnitude, collisionLayer))
            {
                if(endHit.normal == startHit.normal)
                    return null;
                finalNormal += endHit.normal;
                finalNormal /= 2;
                newCorner = startPlane.ClosestPointOnPlane(endHit.point+endHit.normal*ropeRadious);
                return new PointSim(newCorner, false);
            }
        }
        return null;
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
