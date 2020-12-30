using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeGenerator : MonoBehaviour
{
    public Transform ropesHolder;
    public Transform fakePointPrefab;
    public List<Transform> points = new List<Transform>();
    public List<Vector3> positions = new List<Vector3>();
    public float distanceBetween;
    public float maxDistance;
    public float ropeGravity;
    public float pointLerping;
    public float ropeRadious;
    public bool canStrech;    
    public LayerMask ropeLayers;
    private RopeMove currentFinalPoint;
    private LineRenderer lineRenderer;
    public List<Vector3> middlePoints = new List<Vector3>();
    public List<Vector3> currentMiddlePoints = new List<Vector3>();
    public int FromLastStaticPoint;
    public float currentLength;
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
    public void DestroyRope()
    {
        foreach(Transform trans in points)
        {
            Destroy(trans.gameObject);
        }
        points.Clear();
        positions.Clear();
    }
    public void UpdateRope(Rigidbody finalPointRB, RopeMove ropeMove)
    {
        if(currentFinalPoint == null)
            currentFinalPoint = ropeMove;
        positions.Clear();
        for(int i = 0; i < points.Count-1; i++)
        {
            positions.Add(points[i].position);
        }
        Vector3 finalPosition = ropeMove.transform.position;
        if(positions.Count>0)
            finalPosition = GetFixedPoints(middlePoints, ropeMove.transform.position, positions[positions.Count-1]);    
        
        positions.Add(finalPosition);
        positions.Insert(0,transform.position+Vector3.up*1);   
        currentLength = GetLength(positions);             
        if(currentFinalPoint.ObjectClinged)        
            GrabedRope(positions);        
        else
            AirRope(positions);  
        positions.RemoveAt(0); 
        lineRenderer.positionCount = positions.Count+currentMiddlePoints.Count+1;   

        for(int i= 0; i< positions.Count-1; i++)
        {
            points[i].position = Vector3.Lerp(points[i].position,positions[i], Time.deltaTime*pointLerping);
            lineRenderer.SetPosition(i+1, points[i].position);
        }   
        for(int i= 0; i< currentMiddlePoints.Count-1; i++)
        {
            lineRenderer.SetPosition(positions.Count+i, currentMiddlePoints[i]);
        }   

        lineRenderer.SetPosition(0, transform.position+Vector3.up);
        lineRenderer.SetPosition(positions.Count+currentMiddlePoints.Count, ropeMove.transform.position);
        if(Vector3.Distance(transform.position, currentFinalPoint.transform.position) > maxDistance)
        {
            Vector3 gotIt = (points[points.Count-1].position-transform.position);
            Vector3 current = Vector3.Project(finalPointRB.velocity, gotIt);
            finalPointRB.velocity -= current;
        }
    }
    public void GenerateRope(RopeMove finalRope)
    {
        currentFinalPoint = finalRope;
        points.Clear();
        points.Add(finalRope.transform);
    }
    void AirRope(List<Vector3> enterPositions)
    {
        ApplyGravity(enterPositions);
        if(currentLength < maxDistance)
        {
            float initialDis = Vector3.Distance(enterPositions[0], enterPositions[1]);
            if(initialDis > distanceBetween)
            {
                Transform tempTr = Instantiate(fakePointPrefab, enterPositions[0], Quaternion.identity, ropesHolder);
                points.Insert(0,tempTr);
                enterPositions.Insert(1, tempTr.position);
            }  
            LastToFirst(enterPositions.Count-1,enterPositions);   
        }
        else
        {
            LastToFirst(0, enterPositions);
            FirstToLast(0, enterPositions);
        }
    }
    Vector3 GetFixedPoints(List<Vector3> oldMiddlePoints, Vector3 ropeMovePosition, Vector3 lastPoint)
    {
        oldMiddlePoints.Reverse();
        List<Vector3> currentPoints = new List<Vector3>();
        if(oldMiddlePoints.Count> 0)
        {
            int amount = oldMiddlePoints.Count;
            for(int i = 0; i<oldMiddlePoints.Count; i++)
            {
                Vector3 currentPosition = oldMiddlePoints[i];
                Vector3 nextPosition;
                if(i+1 >= amount)
                    nextPosition = ropeMovePosition;
                else
                    nextPosition = oldMiddlePoints[i+1];
                    
                Vector3 beforePosition;
                if(i-1 < 0)
                    beforePosition = lastPoint;
                else
                    beforePosition = oldMiddlePoints[i-1];

                Vector3 direction1 = beforePosition-nextPosition;
                RaycastHit hit1;
                RaycastHit hit2;
                if(Physics.Raycast(nextPosition, direction1, out hit1, direction1.magnitude, ropeLayers) && Physics.Raycast(beforePosition, -direction1, out hit2, direction1.magnitude, ropeLayers))
                {
                    Plane pointPlane1 = new Plane(hit1.normal,hit1.point+hit1.normal*ropeRadious);
                    Plane pointPlane2 = new Plane(hit2.normal,hit2.point+hit2.normal*ropeRadious);
                    currentPosition = pointPlane1.ClosestPointOnPlane(currentPosition);
                    currentPosition = pointPlane2.ClosestPointOnPlane(currentPosition);
                    currentMiddlePoints.Add(currentPosition);
                }
                else
                    currentMiddlePoints.Add(currentPosition);
            }
            currentMiddlePoints = currentPoints;
            if(currentMiddlePoints.Count> 0)            
                return currentMiddlePoints[currentMiddlePoints.Count-1];            
            else
            {
                return ropeMovePosition;
            }
        }
        else
            return ropeMovePosition;
        /*
        List<Edge> currentPoints = new List<Edge>();
        if(points.Count< 1)
        {
            points.Add(new Edge(transform.position+Vector3.up, Vector3.up, Vector3.up));
            points.Add(new Edge(thrownO.transform.position, Vector3.up, Vector3.up));
        }
        points[0].point = transform.position+Vector3.up;
        points[points.Count-1].point = thrownO.transform.position;
        for(int i= 0; i< points.Count-1;i++)
        {
            if(points[i] == null)
                continue;
            Vector3 direction = points[i+1].point-points[i].point;
            Vector3 point = points[i].point;
            RaycastHit hit;
            RaycastHit hit2;
            Edge currentEd = points[i];
            currentPoints.Add(currentEd);
            if(Physics.Raycast(points[i].point, direction,out hit, direction.magnitude) && Physics.Raycast(points[i+1].point, -direction,out hit2, direction.magnitude))
            {
                Vector3 finalPoint;              
                Vector3 eix = Vector3.Cross(hit.normal,hit2.normal); 
                Plane pointPlane1 = new Plane(hit.normal,hit.point);
                finalPoint = pointPlane1.ClosestPointOnPlane(hit2.point);
                finalPoint += ((hit.normal+hit2.normal)/2)/10;
                if((hit.point-hit2.point).magnitude > 0.2f)
                {
                    currentPoints.Add(new Edge(finalPoint, hit.normal, hit2.normal));
                }
            }
            else
            {
                if(points.Count > i+2)
                {
                    Vector3 normal1 = points[i+1].normal1;
                    Vector3 normal2 = points[i+1].normal2;
                    Vector3 point1 = points[i+1].point;
                    Vector3 point2 = points[i+1].point;
                    Vector3 middle = (points[i+2].point+points[i].point)/2;
                    direction = points[i+2].point-points[i].point;
                    if(!Physics.Raycast(points[i].point, direction,out hit, direction.magnitude))
                    {
                        currentPoints.Remove(currentEd);
                        continue;
                    }
                    else
                    {
                        normal1 = hit.normal;
                        middle = hit.point+normal1/2;
                        //point1 = hit.point+normal1/2;
                        //point2 = hit2.point+normal2/2;
                        Debug.DrawRay(points[i+2].point, direction, Color.red);
                    }
                    Plane pointPlane1 = new Plane(normal1,point1);
                    Plane pointPlane2 = new Plane(normal2,point2);
                    Vector3 projected = pointPlane1.ClosestPointOnPlane(middle);
                    projected = pointPlane2.ClosestPointOnPlane(projected);
                    points[i+1].point = Vector3.Lerp(points[i+1].point,projected,Time.deltaTime*5);
                }
            }            
        }
        currentPoints.Add(new Edge(thrownO.transform.position, Vector3.up,Vector3.up));
        renderer.positionCount = currentPoints.Count;
        Vector3[] pointAr = new Vector3[currentPoints.Count];
        for(int i = 0; i< pointAr.Length; i++)
        {
            pointAr[i] = currentPoints[i].point;
        }
        renderer.SetPositions(pointAr);
        points = currentPoints;
        float currentDistance = 0;*/
    }
    void GrabedRope(List<Vector3> enterPositions)
    {
        ApplyGravity(enterPositions);
        FirstToLast(enterPositions.Count-1, enterPositions);
        LastToFirst(enterPositions.Count-1, enterPositions);
    }
    void ApplyGravity(List<Vector3> pointsTemp)
    {
        for(int i = 0; i< pointsTemp.Count-1; i++)
        {
            Vector3 trans1 = pointsTemp[i];
            Vector3 gravity = Vector3.down*ropeGravity*Time.deltaTime;
            Collider[] collisions = Physics.OverlapSphere(trans1, 0.21f, ropeLayers);
            if(!Physics.Raycast(trans1, Vector3.down, gravity.magnitude*Time.deltaTime,ropeLayers) && collisions.Length <= 0)
            {
                trans1 += gravity;
                pointsTemp[i] = trans1;
            }
        }
    }
    void FirstToLast(int fixedPoint,List<Vector3> positionsTemp)
    {
        for(int i = 1; i<positionsTemp.Count; i++)
        {
            if(i == fixedPoint)
                continue;
            else
            {
                Vector3 trans1 = positionsTemp[i];
                Vector3 trans2 = positionsTemp[i-1];
                float distanceBetweenPoints = Vector3.Distance(trans2,trans1);
                Vector3 directionForward = trans2-trans1;                
                if(canStrech)
                {
                    if(distanceBetweenPoints>distanceBetween)
                        trans1 += (distanceBetweenPoints-distanceBetween)*(directionForward).normalized;  
                }
                else
                    trans1 += (distanceBetweenPoints-distanceBetween)*(directionForward).normalized;  
                positionsTemp[i] = trans1;                 positionsTemp[i] = trans1;  
                Debug.DrawLine(trans1,trans2, Color.black);
            }
        }
    }
    void LastToFirst(int fixedPoint, List<Vector3> positionsTemp)
    {
        int tempFix = (positionsTemp.Count-1)-fixedPoint;
        positionsTemp.Reverse();
        for(int i =1; i<positionsTemp.Count; i++)
        {
            if(i == tempFix)
                continue;
            else
            {
                Vector3 trans1 = positionsTemp[i];
                Vector3 trans2 = positionsTemp[i-1];           
                Vector3 trans3 = Vector3.zero;
                if(i-2 > 0)
                    trans3 = positionsTemp[i-2];
                CollisionDetectionBetweenPoints(ref trans1, trans2, trans3);
                float distanceBetweenPoints = Vector3.Distance(trans2,trans1);
                Vector3 directionForward = trans2-trans1;          
                
                if(canStrech)
                {
                    if(Vector3.Distance(trans2,trans1)>distanceBetween)
                        trans1 += (Vector3.Distance(trans2,trans1)-distanceBetween)*(directionForward).normalized;  
                }
                else
                    trans1 += (Vector3.Distance(trans2,trans1)-distanceBetween)*(directionForward).normalized;  
                
                Debug.DrawLine(trans1,trans2, Color.magenta);
                positionsTemp[i] = trans1;  
            }
        }
        positionsTemp.Reverse();
    }
    void CollisionDetectionBetweenPoints(ref Vector3 forwardPoint, Vector3 backPoint, Vector3 backBackPoint)
    {
        RaycastHit hit1;
        RaycastHit hit2;
        Vector3 directionVector = forwardPoint-backPoint;
        if(Physics.Raycast(backPoint, directionVector, out hit1, directionVector.magnitude, ropeLayers))
        {
            //Corner detection
            if(Physics.Raycast(forwardPoint, -directionVector, out hit2, directionVector.magnitude, ropeLayers))
            {
                Debug.DrawRay(backPoint,Vector3.up, Color.blue);
                Debug.DrawRay(forwardPoint,Vector3.up, Color.gray);
                Plane plane1 = new Plane(hit1.normal, hit1.point);
                Plane plane2 = new Plane(hit2.normal, hit2.point);
                Vector3 middle = (hit1.point+hit2.point)/2;
                middle = plane1.ClosestPointOnPlane(middle);
                middle = plane2.ClosestPointOnPlane(middle);
                Debug.DrawRay(middle,hit1.normal+hit2.normal,Color.red);  
                if(Vector3.Distance(backPoint, middle)< distanceBetween+0.1f)
                    middlePoints.Add(middle);              
            }
        }
    }
    float GetLength(List<Vector3> positions)
    {
        float current = 0;
        for(int i = 0; i< positions.Count-1; i++)
        {
            current += Vector3.Distance(positions[i],positions[i+1]);
        }
        return current;
    }    
    
}

