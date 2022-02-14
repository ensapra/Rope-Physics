using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PointSim
{
    [SerializeField] Vector3 position;
    Vector3 previousPosition;

    [SerializeField] bool staticPoint;
    public PointSim(Vector3 position, bool staticPoint)
    {
        this.position = position;
        this.staticPoint = staticPoint;
        this.previousPosition = position;
    }
    public void setStatic(bool staticPoint)
    {this.staticPoint = staticPoint;}
    public bool isStatic(){return staticPoint;}
    public PointSim(PointSim originalPoint)
    {
        this.position = originalPoint.getPosition();
        this.staticPoint = originalPoint.isStatic();
    }
    public void CollisionCheck(float ropeRadious, LayerMask ropeLayers, float groundFriction)
    {
        Vector3 startingPoint = previousPosition;
        Vector3 finalPoint = position;
        Vector3 direction = finalPoint-startingPoint;
        RaycastHit hit;
        if(Physics.SphereCast(startingPoint, ropeRadious*0.9f, direction, out hit, direction.magnitude, ropeLayers))
        {
            Plane temporalPlane = new Plane(hit.normal, hit.point+hit.normal*ropeRadious);
            finalPoint = temporalPlane.ClosestPointOnPlane(finalPoint);
        }
        direction = finalPoint-startingPoint;
        RaycastHit nonAll;
        if(Physics.SphereCast(startingPoint, ropeRadious*0.8f, direction, out nonAll, direction.magnitude, ropeLayers))
            finalPoint = hit.point+hit.normal*ropeRadious;
        Collider[] colliders = Physics.OverlapSphere(finalPoint, ropeRadious, ropeLayers);
        if(colliders.Length > 0)
        {
            for(int i = 0; i < colliders.Length; i++)
            {
                Vector3 collisionPoint = colliders[i].ClosestPoint(finalPoint);
                Vector3 directionColl = (finalPoint-collisionPoint);
                finalPoint += directionColl.normalized*(ropeRadious-directionColl.magnitude);
            }
        }

        if(finalPoint != position)
        {
            Vector3 newDirection = startingPoint-finalPoint;
            position = finalPoint;
            previousPosition = finalPoint + (startingPoint-finalPoint)*(1-groundFriction);
        }
    }
    private Vector3 CollisionCheck(Vector3 startingPoint, Vector3 endingPoint)
    {
        RaycastHit hit;
        Vector3 direction = endingPoint-startingPoint;
        if(Physics.SphereCast(startingPoint, 0.9f, direction, out hit, direction.magnitude+0.2f))
        {
            return hit.point-direction.normalized*0.19f;
        }
        return endingPoint;
    }
    public void UpdatePhysics(Vector3 gravity, float airFriction)
    {
        if(staticPoint)
            return;
        Vector3 existingPrevious = previousPosition;
        previousPosition = position;
        Vector3 movedPoint = position + gravity*Time.fixedDeltaTime + (position-existingPrevious)*airFriction;
        position = movedPoint;
    }
    public void UpdatePosition(Vector3 position)
    {
        this.position = position;
    }
    public void ConstrainPosition(Vector3 addition)
    {
        if(!staticPoint)
            this.position += addition/2;
    }
    public Vector3 getPosition()
    {
        return position;
    }

}
