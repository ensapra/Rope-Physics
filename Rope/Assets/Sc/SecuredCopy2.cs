using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecuredCopy2 : MonoBehaviour
{
    /*
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
    public List<MiddlePoint> middles = new List<MiddlePoint>();
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
        for(int i = 0; i < points.Count; i++)
        {
            positions.Add(points[i].position);
        }
        positions.Insert(0,transform.position+Vector3.up*1);   
        currentLength = GetLength(positions);     
        if(currentFinalPoint.ObjectClinged)        
            GrabedRope(positions);        
        else
            AirRope(positions);  
        GetFixedPoints(positions);    
        positions.RemoveAt(0); 
        lineRenderer.positionCount = positions.Count+1;   
        for(int i= 0; i< positions.Count-1; i++)
        {
            points[i].position = Vector3.Lerp(points[i].position,positions[i], Time.deltaTime*pointLerping);
            lineRenderer.SetPosition(i+1, points[i].position);
        }    
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(positions.Count, ropeMove.transform.position);
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
    void GetFixedPoints(List<Vector3> currentPoints)
    {
        /*
        if(middles.Count <= 0)
        {
            FromLastStaticPoint = 0;
            return;
        }
        currentPoints.Reverse();
        FromLastStaticPoint  = middles[middles.Count-1].i;
        int imiddle = 0;
        for(int i = 1;i < FromLastStaticPoint; i++)
        {
            if(i > middles[imiddle].i)
                i+= 1;            
            Vector3 trans1 = currentPoints[i];
            Vector3 trans2 = currentPoints[i-1];
            Vector3 dir = middles[imiddle].point;
            Vector3 directionForward = dir-trans1;
            trans1 = trans2 + (distanceBetween)*(directionForward).normalized;  
        }
        currentPoints.Reverse();*/
        /*
            if(trans1.y+0.2f < trans2.y && currentLength > maxDistance)
            {
                Vector3 trans3;       
                if(i+1 < positionsTemp.Count-1)
                {
                    trans3 = positionsTemp[i+1];
                    if(trans3.y > trans1.y)
                        final = i;
                }
            }
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
        int nextI = -1;
        middles.Reverse();
        if(middles.Count>0)
            nextI = middles[0].i;
        int amount = 0;
        for(int i = 1; i<positionsTemp.Count; i++)
        {
            if(i == fixedPoint)
                continue;
            else
            {
                Vector3 trans1 = positionsTemp[i];
                Vector3 trans2 = positionsTemp[i-1];
                if(i == nextI && amount < middles.Count)
                {
                    trans2 = middles[amount].point;
                    if(amount+1 < middles.Count)
                        nextI =  middles[amount+1].i;
                    else
                        nextI = -1;
                                       
                    trans2 = middles[amount].point;
                    if(i-2 >= 0)
                    {
                        Vector3 trans3 = positionsTemp[i-2];
                        if(Physics.Raycast(trans1, trans1-trans3, (trans1-trans3).magnitude, ropeLayers))
                            trans2 = middles[amount].point;
                    }
                    amount += 1;

                }
                float distanceBetweenPoints = Vector3.Distance(trans2,trans1);
                Vector3 directionForward = trans2-trans1;                
                trans1 += (distanceBetweenPoints-distanceBetween)*(directionForward).normalized;  
                positionsTemp[i] = trans1;  
                Debug.DrawLine(trans1,trans2, Color.black);
            }
        }
    }
    void LastToFirst(int fixedPoint, List<Vector3> positionsTemp)
    {
        int tempFix = (positionsTemp.Count-1)-fixedPoint;
        positionsTemp.Reverse();
        int currentAmount = 0;
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
                Vector3 middlePoint = CollisionDetectionBetweenPoints(ref trans1, trans2, trans3);
                float distanceBetweenPoints = Vector3.Distance(trans2,trans1);
                float minDistance = distanceBetween;
                Vector3 directionForward = trans2-trans1;
                if(middlePoint != Vector3.zero)
                {
                    if(currentAmount< middles.Count)
                        middles[currentAmount] = new MiddlePoint(middlePoint, positionsTemp.Count-1-i);
                    else
                        middles.Add(new MiddlePoint(middlePoint, positionsTemp.Count-1-i));
                    currentAmount += 1;
                    distanceBetweenPoints = Vector3.Distance(trans1,middlePoint);
                    minDistance -= Vector3.Distance(middlePoint,trans2);
                    if(canStrech)
                    {
                        if(distanceBetweenPoints>minDistance)
                            trans1 += (distanceBetweenPoints-minDistance)*(middlePoint-trans1).normalized;  
                    }
                    else
                        trans1 += (distanceBetweenPoints-minDistance)*(middlePoint-trans1).normalized;  
                }
                
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
        if(currentAmount < middles.Count)
            middles.RemoveRange(currentAmount, middles.Count-currentAmount);
        positionsTemp.Reverse();
    }
    Vector3 CollisionDetectionBetweenPoints(ref Vector3 forwardPoint, Vector3 backPoint, Vector3 backBackPoint)
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
                    return middle;                    
            }
        }
        Plane plane = new Plane(hit1.normal, hit1.point+hit1.normal*ropeRadious);
        forwardPoint = plane.ClosestPointOnPlane(forwardPoint);
        Debug.DrawRay(hit1.point, Vector3.up*2, Color.yellow);
        return Vector3.zero;
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
    */
}

