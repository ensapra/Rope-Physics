using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PointSim
{
    [SerializeField] Vector3 position;
    Vector3 previousPosition;
    Vector3 futurePosition;

    bool staticPoint{set{storedValue = value;} get{return rb != null? rb.isKinematic:storedValue;;}}
    bool storedValue = true;
    [SerializeField] Transform transform;
    [SerializeField] Rigidbody rb;

    bool insideObject;

    //Simplificar les creacions de punts
    public PointSim(Vector3 position, bool staticPoint)
    {
        this.position = position;
        this.staticPoint = staticPoint;
        this.previousPosition = position;
        this.futurePosition = position;
        this.transform = null;
        this.rb = null;
    }
    public PointSim(Transform transform)
    {
        RepurposeObject(transform);
    }
    public PointSim(PointSim originalPoint)
    {
        RepurposeObject(originalPoint);
    }
    public void RepurposeObject(PointSim copiedPoint)
    {
        this.position = copiedPoint.position;
        this.previousPosition = copiedPoint.previousPosition;
        this.storedValue = copiedPoint.storedValue;
        this.futurePosition = copiedPoint.futurePosition;
        this.transform = copiedPoint.transform;
        this.rb = copiedPoint.rb;
    }
    public void RepurposeObject(Transform transform)
    {
        this.position = transform.position;
        this.previousPosition = position;
        this.transform = transform;
        this.rb = transform.GetComponent<Rigidbody>();
        this.futurePosition = position;
    }
    public Vector3 getPosition(){return position;}
    public Vector3 getFuturePosition(){return futurePosition;}
    public void CollisionCheck(float ropeRadious, LayerMask ropeLayers, float groundFriction)
    {
        insideObject = false;
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
                else
                    insideObject = true;                
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

    public void ApplyForcesToRigidbodies(LayerMask ropeLayers)
    {
        if(insideObject)
            return;
        RaycastHit hit;
        Vector3 direction = futurePosition-position;
        if(Physics.Raycast(position, direction, out hit, direction.magnitude, ropeLayers))
        {
            if(hit.collider)
            {
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                if(rb != null)
                {
                    Vector3 finalPosition = hit.collider.ClosestPoint(position);
                    rb.AddForceAtPosition(direction, finalPosition);
                }   
            }
        }
    }
    public void UpdatePhysics(Vector3 gravity, float airFriction)
    {
        futurePosition = position;
        Vector3 existingPrevious = previousPosition;
        previousPosition = position;
        if(staticPoint)
            return;
        Vector3 movedPoint = position + gravity*Time.fixedDeltaTime + (position-existingPrevious)*airFriction;
        position = movedPoint;
        futurePosition = movedPoint;
    }
    public void UpdatePosition()
    {
        if(transform != null)
        {
            this.position = transform.position;
            this.futurePosition = transform.position;
        }
    }
    public void ConstrainFuture(Vector3 addition)
    {
        if(!staticPoint)
            this.futurePosition += addition/2;
    }
    public void ConstrainPosition(Vector3 addition)
    {
        if(!staticPoint)
        {
            //Falta eliminar si la distancia es masa gran
            if(rb != null)
            {
                rb.AddForceAtPosition(addition/2, position, ForceMode.Impulse);
            }
            this.position += addition/2;
        }
    }
}
