﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TheRopeGenerator : MonoBehaviour
{
    [Header("Performance Settings")]
    public bool RUN = true;
    [Range(1,200)] public int maxPointsLimit = 100;
    [Range(1,100)] public int resolution = 10;
    [Range(0,1)] public float strechiness;

    [Header("The Points")]
    public List<DynamicPoint> dynamicPoints;
    
    [Header("RopeConfiguration")]
    public float Lenght;
    public float realLenght;
    public float ropeRadious;
    [Range(0,1)]public float groundFriction;
    public LayerMask ropeLayers;
    public float distanceBetweenPoints;
    public float gravity;
    public bool canFlop;

    [Header("Visualizer")]
    public Material material;
    LineRenderer lineRenderer;
    public bool DrawRopeLines;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if(lineRenderer == null)           
            lineRenderer = gameObject.AddComponent<LineRenderer>();        
        lineRenderer.widthMultiplier = ropeRadious*2;
        ropeLayers = LayerMask.GetMask("Default");
        lineRenderer.material = material;
        GenerateInBetweenPoints();
        AddConstraints(0);
    }

    void FixedUpdate()
    {
        if(!RUN)
            return;
        float extraDistance = GenerateInBetweenPoints();
        AddPhysics();
        AddConstraints(extraDistance);
        CollisionSolver(dynamicPoints);
        VisualizePoints();
    }
    float GenerateInBetweenPoints()
    {
        int amountOfPoints = Mathf.FloorToInt(Lenght/distanceBetweenPoints);
        float extra = Lenght-distanceBetweenPoints*amountOfPoints;
        if(extra <= 0)
            amountOfPoints -= 1;
        else
            amountOfPoints += 1;
        
        amountOfPoints = Mathf.Clamp(amountOfPoints, 0, maxPointsLimit);
        if(dynamicPoints.Count-2 < amountOfPoints)
        {
            DoPoints(amountOfPoints);
        }
        else
            RemovePoints(amountOfPoints);
        
        return extra;
    }
    void RemovePoints(int amountOfPoints)
    {
        dynamicPoints.RemoveRange(amountOfPoints+1, dynamicPoints.Count-2-amountOfPoints);
    }
    void DoPoints(int amountOfPoints)
    {
        DynamicPoint startA = dynamicPoints[0];
        DynamicPoint startB = dynamicPoints[dynamicPoints.Count-1];
        DynamicPoint pointA = new DynamicPoint(dynamicPoints[0]);
        DynamicPoint pointB = new DynamicPoint(dynamicPoints[dynamicPoints.Count-1]);
        amountOfPoints += 2;
        if(startA.isFixed && dynamicPoints.Count < amountOfPoints)
        {
            dynamicPoints.Insert(1, pointA);
        }
        if(startB.isFixed && dynamicPoints.Count < amountOfPoints)
        {
            dynamicPoints.Insert(dynamicPoints.Count-1,pointB);
        }       
    }
    void AddPhysics()
    {
        for(int i = 0; i < dynamicPoints.Count; i++)
        {   
            DoPhysics(dynamicPoints[i]);
        }
    }
    void DoPhysics(DynamicPoint point)
    {
        Vector3 current = point.currentPoint;
        Vector3 last = point.lastPoint;
        Vector3 movedPoint = current + gravity*Vector3.down*Time.fixedDeltaTime + current-last;
        point.currentPoint = movedPoint;
        point.lastPoint = current;
    }
    void AddConstraints(float extraDistance)
    {
        List<DynamicPoint> temporalCopy = new List<DynamicPoint>(dynamicPoints);
        int load = Mathf.FloorToInt(Mathf.Lerp(1, resolution/distanceBetweenPoints, strechiness));
        for(int x = 0; x < load; x++)
        {
            DoConstraint(extraDistance, temporalCopy);
        }        
        dynamicPoints = temporalCopy;
    }
    void DoConstraint(float extraDistance, List<DynamicPoint> temporalCopy)
    {
        int lastPoint = temporalCopy.Count;
        float distanceInHalf = extraDistance/2;
        for(int i = 1; i < lastPoint; i++)
        {
            DynamicPoint currentI1 = temporalCopy[i];
            DynamicPoint currentI2 = temporalCopy[i-1];
            Vector3 trans1 = currentI1.currentPoint;
            Vector3 trans2 = currentI2.currentPoint; 
            
            Vector3 directionForward = (trans2-trans1).normalized;
            float distance = Vector3.Distance(trans2,trans1);
            
            Vector3 directionAddition;
            float tempDistance;
            /*if((i == 1 || i == lastPoint-1) && extraDistance > 0)
            {
                tempDistance = distanceInHalf*(currentI1.percent+ (1-currentI2.percent));
                directionAddition = (distance-tempDistance)*(directionForward).normalized;
            }
            else
            {*/
                tempDistance = distanceBetweenPoints*(currentI1.percent+ (1-currentI2.percent));
                directionAddition = (distance-tempDistance)*(directionForward).normalized/2;
            //}

            if(!canFlop || (canFlop && distance > tempDistance))
            {
                trans1 += directionAddition;  
                trans2 -= directionAddition; 
            }   

            currentI1.currentPoint = trans1;
            currentI2.currentPoint = trans2;
        }
    }
    void CollisionSolver(List<DynamicPoint> pointsToEdit)
    {
        for(int i = 0; i < pointsToEdit.Count; i++)
        {
            ChangeFromRigidbodies(pointsToEdit[i]);
            ChangePositionCollision(pointsToEdit[i]);
            //AddForcesToRigidbodies(points[i], additive);
        }
    }
    void AddForcesToRigidbodies(DynamicPoint point, RaycastHit hit)
    {
        if(hit.collider)
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if(rb != null)
            {
                Vector3 force = point.currentPoint- point.lastPoint;
                Vector3 position = hit.collider.ClosestPoint(point.currentPoint);
                rb.AddForceAtPosition(force, position);
            }     
        }   
    }
    void ChangeFromRigidbodies(DynamicPoint point)
    {
        Vector3 lastPoint = point.lastPoint;
        Collider[] collidersInside = Physics.OverlapSphere(lastPoint, ropeRadious, ropeLayers);
        if(collidersInside.Length> 0)
        {
            for(int i = 0; i < collidersInside.Length; i++)
            {
                Rigidbody rb = collidersInside[i].GetComponent<Rigidbody>();
                if(rb)                    
                    lastPoint += rb.velocity*Time.deltaTime;                
            }
        }
        point.lastPoint = lastPoint;
    }
    void ChangePositionCollision(DynamicPoint point)
    {
        Vector3 startingPoint = point.lastPoint;
        Vector3 finalPoint = point.currentPoint;
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
        AddForcesToRigidbodies(point, hit);
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

        if(finalPoint != point.currentPoint)
        {
            Vector3 newDirection = startingPoint-finalPoint;
            point.currentPoint = finalPoint;
            point.lastPoint = finalPoint + (startingPoint-finalPoint)*(1-groundFriction);
        }
    }
    void VisualizePoints()
    {
        List<DynamicPoint> temporalPoints = new List<DynamicPoint>(dynamicPoints);
        realLenght = 0;
        List<Vector3> currentPoints = new List<Vector3>();
        lineRenderer.widthMultiplier = ropeRadious*2;
        for(int x = 0; x < temporalPoints.Count; x++)
        {
            currentPoints.Add(temporalPoints[x].currentPoint);
            if(x > 0)
                realLenght += Vector3.Distance(temporalPoints[x].currentPoint,temporalPoints[x-1].currentPoint);
            if(DrawRopeLines)
            {
                Debug.DrawRay(temporalPoints[x].currentPoint, Vector3.up, Color.red);
                if(x>0)
                    Debug.DrawLine(temporalPoints[x].currentPoint,temporalPoints[x-1].currentPoint, Color.blue);
            }
        } 
        lineRenderer.positionCount = currentPoints.Count;
        lineRenderer.SetPositions(currentPoints.ToArray());
    }
}
[System.Serializable]
public class DynamicPoint
{
    private Vector3 current;
    private Vector3 last;
    [SerializeField] private bool _fixed;
    [Range(0,2)] public float percent = 1;
    public Vector3 offsetOfForce;
    public Transform realObject;
    public Rigidbody realObjectRB;
    private DestroyItselfPoint currentDestroyTemporal;
    public Vector3 currentPoint{
        get{return current;}
        set{
            if(_fixed)
            {
                if(realObject != null)
                {
                    if(realObjectRB == null || realObjectRB.isKinematic)
                        current = realObject.position+realObject.TransformDirection(offsetOfForce);
                    else
                    {
                        realObjectRB.AddForceAtPosition((value-current), realObject.position+realObject.TransformDirection(offsetOfForce), ForceMode.Impulse);
                        current = realObject.position+realObject.TransformDirection(offsetOfForce);
                    }
                }
                else
                {
                    GameObject currentFake = new GameObject("FakeRealWorld Point");
                    currentDestroyTemporal = currentFake.AddComponent<DestroyItselfPoint>();
                    currentFake.transform.position = last;
                    realObject = currentFake.transform;
                    current = last;
                }
                last = current;
            }
            else
            {
                if(realObject && currentDestroyTemporal)
                   currentDestroyTemporal.Destroy();
                current = value;
            }
        }
    }
    public Vector3 lastPoint{
        get{return last;}
        set{last = value;}
    }
    public bool isFixed{
        get{ return _fixed;}
        set{}
    }
    public void AssignObject(GameObject referent)
    {
        realObject = referent.transform;
        realObjectRB = referent.GetComponent<Rigidbody>();
    }
    public DynamicPoint(Vector3 currentPoint, Vector3 lastPoint)
    {
        this.currentPoint = currentPoint;
        this.lastPoint = lastPoint;
    }
    public DynamicPoint(DynamicPoint point)
    {
        this.currentPoint = point.currentPoint;
        this.lastPoint = point.lastPoint;
    }
}
public class DestroyItselfPoint : MonoBehaviour
{
    public void Destroy()
    {
        Destroy(this.gameObject);
    }
}