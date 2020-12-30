using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecuredCopy3 : MonoBehaviour
{
    /*
    public List<Segment> segments;
    public List<Edge> corners;
    public LineRenderer lineRenderer;
    public float ropeRadious = 0.2f;
    public float targetLength = 10;
    public float distanceBetweenPoints;
    public float treshold = 0.5f;
    public float currentLenghtOfRope;
    public float smoothness;
    public bool canStrech;
    public LayerMask ropeLayers;
    public float ropeGravity;
    private List<Vector3> allPoints = new List<Vector3>();
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
    public void GenerateRope(Vector3 startingPosition, Vector3 endingPosition)
    {
        corners = FindCorners(startingPosition, endingPosition);
        GetSegments();
        MoveCorners();
        GenerateBetweenPoints(segments);
        VisualizeSegments(segments);
    }
    /*
    void MoveCorners()
    {
        for(int i = 2; i < corners.Count; i++)
        {
            Vector3 starting;
            if(i <= corners.Count-1)
                starting = corners[i].point-corners[i].normal2;
            else
                starting = corners[i].point;
            Vector3 ending;
            if(i-2 >= 0) 
                ending = corners[i-2].point-corners[i-2].normal1;
            else
                ending = corners[i-2].point;
            Debug.DrawRay(starting,Vector3.up*5, Color.red);
            Vector3 betweenDirection = ending-starting;
            Vector3 middle = (ending+starting)/2;
            RaycastHit hit1;
            RaycastHit hit2;
            Vector3 normal1 = corners[i-1].normal1;
            Vector3 normal2 = corners[i-1].normal2;
            Vector3 pointBefore = corners[i-1].point;
            betweenDirection = ending-starting;
            if(Physics.Raycast(starting, betweenDirection,out hit1, betweenDirection.magnitude) && Physics.Raycast(ending, -betweenDirection,out hit2, betweenDirection.magnitude))
            {
                Plane pointplane1 = new Plane(normal1,  pointBefore);
                Plane pointplane2 = new Plane(normal2,  pointBefore);
                middle = pointplane1.ClosestPointOnPlane(middle);
                middle = pointplane2.ClosestPointOnPlane(middle);
                corners[i-1].point = Vector3.Lerp(corners[i-1].point, middle, Time.deltaTime*smoothness/10);
            }
        }
    }
    void MoveCorners()
    {
        for(int i = 1; i < segments.Count; i++)
        {
            Vector3 starting;
            if(segments[i].inBetween.Count>0)
                starting = segments[i].inBetween[0];
            else
            {
                if(i == segments.Count-1)
                    starting = segments[i].endingEdge.point;
                else
                    starting = segments[i].endingEdge.point+segments[i].endingEdge.normal2;

            }            
            Vector3 ending;
            if(segments[i-1].inBetween.Count>0)
                ending = segments[i-1].inBetween[segments[i-1].inBetween.Count-1];
            else
            {
                if(i == 1)
                    ending = segments[i-1].startingEdge.point;
                else
                    ending = segments[i-1].startingEdge.point+segments[i-1].endingEdge.normal1;
            }    
            Debug.DrawRay(starting,Vector3.up*5, Color.red);
            Debug.DrawRay(ending,Vector3.up*5, Color.green);
            Debug.DrawLine(starting, ending, Color.grey);
            Vector3 betweenDirection = ending-starting;
            Vector3 middle = (ending+starting)/2;
            RaycastHit hit1;
            RaycastHit hit2;
            Vector3 normal1 = segments[i].startingEdge.normal1;
            Vector3 normal2 = segments[i].startingEdge.normal2;
            Vector3 pointBefore = segments[i].startingEdge.point;
            betweenDirection = ending-starting;
            if(Physics.Raycast(starting, betweenDirection,out hit1, betweenDirection.magnitude) && Physics.Raycast(ending, -betweenDirection,out hit2, betweenDirection.magnitude))
            {
                Plane pointplane1 = new Plane(hit1.normal,  pointBefore);
                Plane pointplane2 = new Plane(hit2.normal,  pointBefore);
                middle = pointplane1.ClosestPointOnPlane(middle);
                middle = pointplane2.ClosestPointOnPlane(middle);
                corners[i].normal1 =hit1.normal;
                corners[i].normal2 = hit2.normal;
            }
            else
            {
                Plane pointplane1 = new Plane(normal1,  pointBefore);
                Plane pointplane2 = new Plane(normal2,  pointBefore);
                middle = pointplane1.ClosestPointOnPlane(middle);
                middle = pointplane2.ClosestPointOnPlane(middle);
            }
            corners[i].point = Vector3.Lerp(corners[i].point, middle, Time.deltaTime*smoothness);
        }
    }
    void VisualizeSegments(List<Segment> segments)
    {
        int currentAmount = 1;
        if(currentLenghtOfRope < targetLength+treshold)
        {
            lineRenderer.positionCount = 1;
            lineRenderer.SetPosition(0, segments[0].startingEdge.point);
        }
        else
            currentAmount = 0;
            
        foreach(Segment segment in segments)
        {
            lineRenderer.positionCount = currentAmount+segment.inBetween.Count+segment.middlePoints.Count+1;
            int amountMiddles = 0;
            for(int i= 0; i< segment.inBetween.Count; i++)
            {
                lineRenderer.SetPosition(currentAmount, segment.inBetween[i]);
                currentAmount += 1;
                if(amountMiddles < segment.middlePoints.Count && segment.middlePoints[amountMiddles].i == i+1)
                {
                    Debug.Log("added");
                    lineRenderer.SetPosition(currentAmount, segment.middlePoints[amountMiddles].point);
                    currentAmount += 1;
                    amountMiddles += 1;
                }  
            }
            lineRenderer.SetPosition(currentAmount, segment.endingEdge.point);
            currentAmount += 1;
        }
    }
    void GetSegments()
    {
        for(int i = 0; i< segments.Count; i++)
        {
            if(i+1 < corners.Count)
            {
                segments[i].startingEdge = corners[i];
                segments[i].endingEdge = corners[i+1];
            }
            else
                break;
        }
        if(segments.Count+1< corners.Count)
        {
            for(int i = segments.Count; i+1 < corners.Count; i++)
            {
                Segment segment = new Segment();
                segment.startingEdge = corners[i];
                segment.endingEdge = corners[i+1];
                segments.Add(segment);                
            }
        }        
        else if(segments.Count >= corners.Count)
        {
            segments.RemoveRange(corners.Count-1, segments.Count-corners.Count+1);
        }
    }
    List<Edge> FindCorners(Vector3 startingPosition, Vector3 endingPosition)
    {   
        List<Edge> foundEdges = new List<Edge>();
        if(corners.Count< 1)
        {
            corners.Add(new Edge(startingPosition, Vector3.up, Vector3.up));
            corners.Add(new Edge(endingPosition, Vector3.up, Vector3.up));
        }
        corners[0].point = startingPosition;
        corners[corners.Count-1].point = endingPosition;
        corners.Reverse();
        foundEdges.Add(corners[0]);
        float distanceTravelled = 0;
        for(int i= 1; i< corners.Count;i++)
        {
            if(corners[i] == null)
                continue;
            Vector3 starting = corners[i].point;
            Vector3 ending = corners[i-1].point;
            Vector3 betweenDirection = ending-starting;
            float preDist = (starting-ending).magnitude;
            //if(distanceTravelled + preDist > targetLength)
                //starting += betweenDirection.normalized * (distanceTravelled+preDist-targetLength);
            RaycastHit hit1;
            RaycastHit hit2;
            Debug.DrawRay(starting, Vector3.up*(i+1), Color.black);
            foundEdges.Add(corners[i]);
            if(Physics.Raycast(starting, betweenDirection, out hit1, betweenDirection.magnitude) && Physics.Raycast(ending, -betweenDirection,out hit2, betweenDirection.magnitude))
            {
                Vector3 finalPoint;              
                Plane pointPlane1 = new Plane(hit1.normal,hit1.point);
                finalPoint = pointPlane1.ClosestPointOnPlane(hit2.point);
                finalPoint += ((hit1.normal+hit2.normal).normalized)*ropeRadious*2;
                Debug.DrawRay(finalPoint, Vector3.up, Color.red);
                if((hit1.point-hit2.point).magnitude > 0.2f)
                {
                    foundEdges.Insert(i,new Edge(finalPoint, hit1.normal, hit2.normal));
                    distanceTravelled += (starting-finalPoint).magnitude +(finalPoint-ending).magnitude;
                    continue;
                }
            }
            else
            {
                if(i-2 >= 0)
                {
                    Vector3 normal1 = corners[i-1].normal1;
                    Vector3 normal2 = corners[i-1].normal2;
                    Vector3 ending2 = corners[i-2].point;
                    Vector3 middle = (ending2+starting)/2;
                    betweenDirection = ending2-starting;
                    if(!Physics.Raycast(starting, betweenDirection,out hit1, betweenDirection.magnitude))
                    {
                        foundEdges.RemoveAt(i-1);
                        distanceTravelled -= (ending-ending2).magnitude; 
                        distanceTravelled += (starting-ending2).magnitude;
                        continue;
                    }
                }
            }      
            distanceTravelled += preDist;
        }
        currentLenghtOfRope = distanceTravelled;
        corners.Reverse();
        foundEdges.Reverse();
        return foundEdges; 
    }
    void GenerateBetweenPoints(List<Segment> currentSegments)
    {
        float totalLength = GetLength(allPoints);
        allPoints.Clear();
        foreach(Segment segment in currentSegments)
        {
            GeneratePoint(segment, currentLenghtOfRope);
            allPoints.AddRange(segment.inBetween);
        }
    }
    float GetLength(List<Vector3> currentSegments)
    {
        float currentLenghtAmount = 0;
        for(int i = 1; i< currentSegments.Count; i++)
        {
            currentLenghtAmount += (currentSegments[i]-currentSegments[i-1]).magnitude;
        }
        return currentLenghtAmount;
    }
    void GeneratePoint(Segment segment, float currentLenghtAmount)
    {
        Vector3 startingPoint = segment.startingEdge.point;
        Vector3 endingPoint = segment.endingEdge.point;
        //Debug.DrawRay(endingPoint, Vector3.up, Color.cyan); 
        int pointAmount = Mathf.FloorToInt(((targetLength*(startingPoint-endingPoint).magnitude)/currentLenghtAmount)/distanceBetweenPoints)-1;
        pointAmount = Mathf.Clamp(pointAmount, 0, 100);
        if(segment.inBetween.Count < pointAmount)
        {
            Vector3 startingTemp;
            if(segment.inBetween.Count <= 0)
                startingTemp = startingPoint;
            else
                startingTemp = segment.inBetween[segment.inBetween.Count-1];
            List<Vector3> empty = new List<Vector3>();
            for(int i= 0; i< pointAmount-segment.inBetween.Count;i++)
            {
                Vector3 position = startingTemp + (endingPoint-startingTemp).normalized*distanceBetweenPoints;//*i*distanceBetweenPoints;
                empty.Add(position);
                startingTemp = position;
            }
            segment.inBetween.AddRange(empty);
        }
        /*
        else if(segment.inBetween.Count> pointAmount)
        {
            segment.inBetween.RemoveRange(pointAmount, segment.inBetween.Count-pointAmount);
        }
        List<Vector3> temporalPoints = new List<Vector3>(segment.inBetween);
        temporalPoints.Insert(0, startingPoint);
        temporalPoints.Add(endingPoint);
        float gravityM = Mathf.Lerp(0,1, Mathf.Clamp(currentLenghtAmount/targetLength,0.9f,1)*10-9);
        ApplyGravity(temporalPoints,1-gravityM);
        FirstToLast(temporalPoints.Count-1,temporalPoints);
        segment.middlePoints = LastToFirst(temporalPoints.Count-1,temporalPoints);
        temporalPoints.RemoveAt(0);
        temporalPoints.RemoveAt(temporalPoints.Count-1);
        for(int i = 0; i< temporalPoints.Count;i++)
        {
            segment.inBetween[i] = Vector3.Lerp(segment.inBetween[i], temporalPoints[i], Time.deltaTime*smoothness);
        }
    }
    void ApplyGravity(List<Vector3> pointsTemp, float gravityMult)
    {
        for(int i = 0; i< pointsTemp.Count-1; i++)
        {
            Vector3 trans1 = pointsTemp[i];
            Vector3 gravity = Vector3.down*ropeGravity*Time.deltaTime*gravityMult;
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
                float between = Vector3.Distance(trans2,trans1);
                Vector3 directionForward = trans2-trans1;                
                if(canStrech)
                {
                    if(between>distanceBetweenPoints)
                        trans1 += (between-distanceBetweenPoints)*(directionForward).normalized;  
                }
                else
                    trans1 += (between-distanceBetweenPoints)*(directionForward).normalized;  
                Debug.DrawLine(trans1,trans2, Color.black);
                positionsTemp[i] = trans1;
            }
        }
    }
    List<MiddlePoint> LastToFirst(int fixedPoint, List<Vector3> positionsTemp)
    {        
        int tempFix = (positionsTemp.Count-1)-fixedPoint;
        positionsTemp.Reverse();
        List<MiddlePoint> middles = new List<MiddlePoint>();
        for(int i = 1; i<positionsTemp.Count; i++)
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
                Debug.DrawRay(trans1,Vector3.up, Color.cyan);
                Debug.DrawRay(trans2,Vector3.up, Color.green);

                Vector3 middlePoint = CollisionDetectionBetweenPoints(ref trans1, trans2, trans3);
                float distanceBetween = Vector3.Distance(trans2,trans1);
                float minDistance = distanceBetweenPoints;
                Vector3 directionForward = trans2-trans1;
                if(middlePoint != Vector3.zero)
                {
                    middles.Add(new MiddlePoint(middlePoint, positionsTemp.Count-1-i));
                    Debug.Log(positionsTemp.Count);
                    distanceBetween = Vector3.Distance(trans1,middlePoint);
                    minDistance -= Vector3.Distance(middlePoint,trans2);
                    
                    if(canStrech)
                    {
                        if(distanceBetween>minDistance)
                            trans1 += (distanceBetween-minDistance)*(middlePoint-trans1).normalized;  
                    }
                    else
                        trans1 += (distanceBetween-minDistance)*(middlePoint-trans1).normalized;  
                }
                
                if(canStrech)
                {
                    if(Vector3.Distance(trans2,trans1)>distanceBetweenPoints)
                        trans1 += (Vector3.Distance(trans2,trans1)-distanceBetweenPoints)*(directionForward).normalized;  
                }
                else
                    trans1 += (Vector3.Distance(trans2,trans1)-distanceBetweenPoints)*(directionForward).normalized;  
                
                Debug.DrawLine(trans1,trans2, Color.magenta);
                positionsTemp[i] = trans1;  
            }
        }
        positionsTemp.Reverse();
        middles.Reverse();
        return middles;
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
                middle += (hit1.normal+hit2.normal).normalized*ropeRadious*2;
                Debug.DrawRay(middle,hit1.normal+hit2.normal,Color.red);  
                if(Vector3.Distance(backPoint, middle)< distanceBetweenPoints+0.1f)
                    return middle;                    
            }
        }
        Plane plane = new Plane(hit1.normal, hit1.point+hit1.normal*ropeRadious);
        forwardPoint = plane.ClosestPointOnPlane(forwardPoint);
        Debug.DrawRay(hit1.point, Vector3.up*2, Color.yellow);
        return Vector3.zero;
    }*/
}
