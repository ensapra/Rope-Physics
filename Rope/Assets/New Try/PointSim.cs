using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PointSim
{
    Vector3 position;
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
    public void UpdatePhysics(Vector3 gravity, float airFriction)
    {
        if(staticPoint)
            return;
        Vector3 existingPrevious = previousPosition;
        previousPosition = position;
        position += (position-existingPrevious)*airFriction;
        position += gravity*Time.deltaTime;
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
