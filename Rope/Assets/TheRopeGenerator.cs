using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheRopeGenerator : MonoBehaviour
{
    [Header("The Start And The End")]
    public bool fixedPointA;
    public ThePoint fixedPositionA;
    public Transform fixedObjectA;

    public bool fixedPointB;
    public ThePoint fixedPositionB;
    public Transform fixedObjectB;

    public float Lenght;
    public float realLenght;
    public float extra;

    [Header("The Points")]
    public List<ThePoint> points;
    public List<Vector3> constrainedPoint;

    [Header("RopeConfiguration")]
    public float ropeRadious;
    [Range(0,1)]public float groundFriction;
    public LayerMask ropeLayers;
    [Range(0,1)] public float strechiness;
    public float distanceBetweenPoints;
    public float gravity;
    public int maxPointsLimit = 100;
    public bool canFlop;

    [Header("Visualizer")]
    LineRenderer lineRenderer;
    public Material material;
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
        float extraDistance = GenerateInBetweenPoints();
        AddPhysics();
        AddConstraints(extraDistance);
        CollisionSolver();
        VisualizePoints();
    }
    float GenerateInBetweenPoints()
    {
        if(fixedPointA && fixedObjectA != null)
        {
            fixedPositionA.currentPoint = fixedObjectA.transform.position;
        }
        if(fixedPointB && fixedObjectB != null)
        {
            fixedPositionB.currentPoint= fixedObjectB.transform.position;
        }

        int amountOfPoints = Mathf.FloorToInt(Lenght/distanceBetweenPoints);
        amountOfPoints = Mathf.Clamp(amountOfPoints, 0, maxPointsLimit);
        extra = Lenght-distanceBetweenPoints*amountOfPoints;
        if(extra <= 0)
            amountOfPoints -= 1;
        else
            amountOfPoints += 1;
        if(points.Count < amountOfPoints)
        {
            DoPoints(amountOfPoints);
        }
        else
            RemovePoints(amountOfPoints);
        
        return extra;
    }
    void RemovePoints(int amountOfPoints)
    {
        points.RemoveRange(amountOfPoints, points.Count-amountOfPoints);
    }
    void DoPoints(int amountOfPoints)
    {
        Vector3 fromAPoint = fixedPositionA.currentPoint;
        Vector3 fromBPoint = fixedPositionB.currentPoint;
        ThePoint pointA = new ThePoint(fromAPoint, fromAPoint);
        ThePoint pointB = new ThePoint(fromBPoint, fromBPoint);
        if(fixedPointA && points.Count < amountOfPoints)
        {
            points.Insert(0, pointA);
        }
        if(fixedPointB && points.Count < amountOfPoints)
        {
            points.Add(pointB);
        }       
    }
    void AddPhysics()
    {
        if(!fixedPointA)
        {
            DoPhysics(fixedPositionA);
        }
        else
            fixedPositionA.lastPoint = fixedPositionA.currentPoint;
        if(!fixedPointB)
        {
            DoPhysics(fixedPositionB);
        }
        else
            fixedPositionB.lastPoint = fixedPositionB.currentPoint;
        
        for(int i = 0; i < points.Count; i++)
        {
            DoPhysics(points[i]);
        }
    }
    void DoPhysics(ThePoint point)
    {
        Vector3 current = point.currentPoint;
        Vector3 last = point.lastPoint;
        Vector3 movedPoint = current + gravity*Vector3.down*Time.fixedDeltaTime + current-last;
        point.currentPoint = movedPoint;
        point.lastPoint = current;
    }
    void AddConstraints(float extraDistance)
    {
        List<ThePoint> temporalCopy = new List<ThePoint>(points);
        temporalCopy.Insert(0, fixedPositionA);
        temporalCopy.Add(fixedPositionB);      
        int load = Mathf.FloorToInt(Mathf.Lerp(1,10/distanceBetweenPoints, strechiness));
        int lastPoint = temporalCopy.Count;
        float distanceInHalf = extraDistance/2;
        for(int x = 0; x < load; x++)
        {
            for(int i = 1; i < lastPoint; i++)
            {
                ThePoint currentI1 = temporalCopy[i];
                ThePoint currentI2 = temporalCopy[i-1];
                Vector3 trans1 = currentI1.currentPoint;
                Vector3 trans2 = currentI2.currentPoint; 
                Vector3 directionForward = (trans2-trans1).normalized;
                float distance = Vector3.Distance(trans2,trans1);
                if((i == 1 || i == lastPoint-1) && extraDistance > 0)
                {
                    if((distance > distanceInHalf && canFlop) || !canFlop)
                    {
                        if(i == 1)
                            trans1 += (distance-distanceInHalf)*(directionForward).normalized;  
                        else
                            trans2 -= (distance-distanceInHalf)*(directionForward).normalized; 
                    }
                }
                else
                if((distance > distanceBetweenPoints && canFlop) || !canFlop)
                {
                    trans1 += (distance-distanceBetweenPoints)*(directionForward).normalized/2;  
                    trans2 -= (distance-distanceBetweenPoints)*(directionForward).normalized/2; 
                }

                if((i == lastPoint-1 && !fixedPointB) || i != lastPoint-1)   
                {
                    currentI1.currentPoint = trans1;
                    temporalCopy[i] = currentI1;     
                }

                if((i == 1 && !fixedPointA) || i != 1)   
                {
                    currentI2.currentPoint = trans2;
                    temporalCopy[i-1] = currentI2;
                }
            }
        }        
        temporalCopy.Remove(fixedPositionA);
        temporalCopy.Remove(fixedPositionB);
        points = temporalCopy;
    }
    void CollisionSolver()
    {
        for(int i = 0; i < points.Count; i++)
        {
            ChangePositionCollision(points[i]);
        }
    }
    void ChangePositionCollision(ThePoint point)
    {
        Vector3 startingPoint = point.lastPoint;
        Vector3 finalPoint = point.currentPoint;
        Vector3 direction = finalPoint-startingPoint;
        Vector3 original = direction;
        Vector3 points = Vector3.zero;
        RaycastHit hit;
        if(Physics.SphereCast(startingPoint, ropeRadious*0.9f, direction, out hit, direction.magnitude, ropeLayers))        
            finalPoint = hit.point + hit.normal*ropeRadious; 
        for(int i = 0; i < 20; i++)
        {
            if(Physics.SphereCast(startingPoint, ropeRadious*0.9f, direction, out hit, direction.magnitude, ropeLayers))  
            {      
                direction  -= Vector3.Project(direction, hit.normal);
                if(i == 0)
                    points = (hit.point + hit.normal*ropeRadious);
                else
                    points += (hit.point + hit.normal*ropeRadious);
            }       
            else
            {
                if(i != 0)
                    finalPoint = points/i + direction;
                Debug.DrawRay(startingPoint, direction, Color.magenta);
                break;
            }
        }

        Collider[] colliders = Physics.OverlapSphere(finalPoint, ropeRadious, ropeLayers);
        Vector3 normals = Vector3.zero;
        if(colliders.Length > 0)
        {
            for(int x = 0; x < colliders.Length; x++)
            {
                Vector3 collisionPoint = colliders[x].ClosestPoint(finalPoint);
                Vector3 directionColl = (finalPoint-collisionPoint);
                finalPoint += directionColl.normalized*(ropeRadious-directionColl.magnitude);
            }
        }

        if(finalPoint != point.currentPoint)
        {
            Vector3 newDirection = startingPoint-finalPoint;
            point.currentPoint = finalPoint;
            point.lastPoint = finalPoint + (startingPoint-finalPoint)*groundFriction;
        }
    }
    void VisualizePoints()
    {
        List<ThePoint> temporalPoints = new List<ThePoint>(points);
        temporalPoints.Insert(0,fixedPositionA);
        temporalPoints.Add(fixedPositionB);
        realLenght = 0;
        List<Vector3> currentPoints = new List<Vector3>();
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
    public Vector3 ClampMagnitude(Vector3 v, float min, float max)
    {
        double sm = v.sqrMagnitude;
        if(sm > (double)max * (double)max) return v.normalized * max;
        else if(sm < (double)min * (double)min) return v.normalized * min;
        return v;
    }
}
[System.Serializable]
public class ThePoint
{
    public Vector3 currentPoint;
    public Vector3 lastPoint;
    public ThePoint(Vector3 currentPoint, Vector3 lastPoint)
    {
        this.currentPoint = currentPoint;
        this.lastPoint = lastPoint;
    }
}

