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
        this.previousPosition = originalPoint.previousPosition;
        this.staticPoint = originalPoint.isStatic();
    }
    public void CollisionCheck(float ropeRadious, LayerMask ropeLayers, float groundFriction)
    {
        InsideCollisionCheck(ropeRadious, ropeLayers);
        Vector3 finalPoint = position;
        finalPoint = FutureCollisionCheck(finalPoint, ropeRadious, ropeLayers);
        finalPoint = CloseCollisionCheck(finalPoint, ropeRadious, ropeLayers);
        
        if(finalPoint != position)
        {
            position = finalPoint;
            Vector3 direction = finalPoint-previousPosition;
            previousPosition = finalPoint + direction*(1-groundFriction);
        }
    }
    private void InsideCollisionCheck(float ropeRadious, LayerMask ropeLayers)
    {
        Collider[] collidersInside = Physics.OverlapSphere(previousPosition, ropeRadious, ropeLayers);
        if(collidersInside.Length> 0)
        {
            for(int i = 0; i < collidersInside.Length; i++)
            {
                Rigidbody rb = collidersInside[i].GetComponent<Rigidbody>();
                if(rb)                    
                    previousPosition += rb.velocity*Time.deltaTime;                
            }
        }
    }
    private Vector3 CloseCollisionCheck(Vector3 finalPoint, float ropeRadious, LayerMask ropeLayers)
    {
        //Handles half Inside an object
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
        return finalPoint;
    }
    private Vector3 FutureCollisionCheck(Vector3 finalPoint, float ropeRadious, LayerMask ropeLayers)
    {        
        //Handles outside an object but will be
        RaycastHit hit;
        Vector3 direction = finalPoint-previousPosition;
        bool foundCollision;
        int tries = 10;
        do{
            direction = finalPoint-previousPosition;
            foundCollision = Physics.SphereCast(previousPosition, ropeRadious*0.9f, direction, out hit, direction.magnitude, ropeLayers);
            if(foundCollision)
            {
                Plane temporalPlane = new Plane(hit.normal, hit.point+hit.normal*ropeRadious);
                if(tries < 10)
                    finalPoint = hit.point+hit.normal*ropeRadious*0.5f;
                else
                    finalPoint = temporalPlane.ClosestPointOnPlane(finalPoint);

            }
            tries--;
        }
        while(foundCollision && tries > 0);
        return finalPoint;
    }
    public void UpdatePhysics(Vector3 gravity, float airFriction)
    {
        Vector3 existingPrevious = previousPosition;
        previousPosition = position;
        if(staticPoint)
            return;
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
