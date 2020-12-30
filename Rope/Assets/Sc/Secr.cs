using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public class Secr : MonoBehaviour
{
    public List<Segment> segments;
    public LineRenderer lineRenderer;
    public int maxPointsInSegment = 100;
    public float ropeRadious = 0.2f;
    public float targetLength = 10;
    public float distanceBetweenPoints;
    public float currentLenghtOfRope;
    public float lineRendererSmoothness = 10;
    public float inBetweenDistanceTreshold;
    public float cornerAngleTrehsold;
    public float cornerDistanceTrehsold = 0.3f;
    public bool canStrech;
    public LayerMask ropeLayers;
    public float ropeGravity = 15;
    public List<Vector3> allPoints = new List<Vector3>();

    [Header("DEBUG OPTIONS")]
    public bool showRopeKinematicLines;
    public bool showCornerVectors;
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
    public void GenerateRope(Vector3 startingPosition, Vector3 endingPosition, bool pinged)
    {
        if(ShouldCalculate(startingPosition, endingPosition))
        {
            if(!pinged)
                targetLength = currentLenghtOfRope+2;
            
            
            GetSegments(startingPosition, endingPosition);
            MoveCorners();
            GenerateBetweenPoints(segments);
            VisualizeSegments(segments, endingPosition);
        }
    }
    void GetSegments(Vector3 startingPosition, Vector3 endingPosition)
    {
        if(segments.Count <= 0)
        {
            Edge StartingEdge = new Edge(startingPosition, Vector3.up, Vector3.up, null);
            Edge EndingEdge = new Edge(endingPosition, Vector3.up, Vector3.up, null);
            CreateSegment(StartingEdge, EndingEdge, new List<Vector3>(), 0);
        }
        else
        {
            segments[0].startingEdge.point = startingPosition;
            segments[segments.Count-1].endingEdge.point = endingPosition;
            List<Segment> temporalSegments = new List<Segment>();
            for(int i = 0; i < segments.Count; i++)
            {
                List<MiddlePoint> middlesPoints = segments[i].middlePoints;
                List<Vector3> inbetweens = segments[i].inBetween;
                Edge startingPoint = segments[i].startingEdge;
                Edge endingPoint = segments[i].endingEdge;
                int lastPoint = 0;
                if(middlesPoints.Count > 0)
                {
                    for(int x = 0; x <= middlesPoints.Count; x++)
                    {
                        int currentPoint;
                        Segment middleSegment = new Segment();
                        if(x < middlesPoints.Count)
                            currentPoint = middlesPoints[x].i;
                        else
                            currentPoint = inbetweens.Count-1;

                        if(x <= 0)
                            middleSegment.startingEdge = startingPoint;
                        else
                            middleSegment.startingEdge = middlesPoints[x-1].edge;
                        
                        if(x == middlesPoints.Count)
                            middleSegment.endingEdge = endingPoint;
                        else
                            middleSegment.endingEdge =  middlesPoints[x].edge;
                        
                        if(currentPoint-lastPoint > 0)
                            middleSegment.inBetween.AddRange(inbetweens.GetRange(lastPoint, currentPoint-lastPoint));
                        lastPoint = currentPoint;
                        if(Vector3.Distance(middleSegment.startingEdge.point, middleSegment.endingEdge.point) > 0.1f)
                            temporalSegments.Add(middleSegment);
                    }
                }
                else
                    temporalSegments.Add(segments[i]);
            }
            segments = temporalSegments;
        }
    }
    bool ShouldCalculate(Vector3 startingPosition, Vector3 endingPosition)
    {
        /*
        if(allPoints.Count <= 0)
            return true;
        else
        {
            if(startingPosition != allPoints[allPoints.Count-1] || endingPosition != allPoints[1])
                return true;
            else
                if(!stableRope)
                    return true;
                else
                    return false;
        }
        return true;
    }
    void MoveCorners()
    {
        for(int i = 1; i<segments.Count; i++)
        {
            Vector3 starting;
            Vector3 ending;
            Segment currrentSegment = segments[i];
            Segment beforeSegment = segments[i-1];
            Edge startingEdge = currrentSegment.startingEdge;
            Edge endingEdge = currrentSegment.endingEdge;
            Vector3 pointBefore = currrentSegment.startingEdge.point;
            if(currrentSegment.inBetween.Count>0)
            {
                starting = currrentSegment.inBetween[0];
                if(Vector3.Distance(starting, startingEdge.point) < distanceBetweenPoints/2 && currrentSegment.inBetween.Count > 1)
                    starting = currrentSegment.inBetween[1];
            }           
            else
            {
                starting = currrentSegment.endingEdge.point;
            }   
            
            if(beforeSegment.inBetween.Count>0)            
            {
                ending = beforeSegment.inBetween[beforeSegment.inBetween.Count-1];  
                if(Vector3.Distance(ending, startingEdge.point) < distanceBetweenPoints/2 && beforeSegment.inBetween.Count > 1)
                    ending = beforeSegment.inBetween[beforeSegment.inBetween.Count-2];   
            }       
            else
            {
                ending = beforeSegment.startingEdge.point;
            }
            RaycastHit hit1;
            RaycastHit hit2;
            Vector3 relocatedStart = pointBefore - startingEdge.normal2*ropeRadious*2 - startingEdge.normal1*ropeRadious*1.2f;
            Vector3 reloactedEnd = pointBefore - startingEdge.normal1*ropeRadious*2 - startingEdge.normal2*ropeRadious*1.2f;
            Vector3 betweenDirection = reloactedEnd-relocatedStart;
            Vector3 middle = (starting+ending)/2;
            Vector3 startingMiddle = pointBefore;

            if(showCornerVectors)
            {
                Debug.DrawRay(pointBefore, startingEdge.normal1, Color.yellow*2);
                Debug.DrawRay(pointBefore, startingEdge.normal2, Color.green*2);
                Debug.DrawLine(relocatedStart, reloactedEnd, Color.magenta*2);
                Debug.DrawLine(relocatedStart, pointBefore - startingEdge.normal2*ropeRadious*2, Color.black*2);
                Debug.DrawLine(reloactedEnd, pointBefore - startingEdge.normal1*ropeRadious*2, Color.black*2);
            }

            if(TwoWayRaycast(relocatedStart, betweenDirection, reloactedEnd, -betweenDirection, out hit1, out hit2, betweenDirection.magnitude))
            {
                if(hit1.normal != -hit2.normal)
                {
                    Plane pointplane1 = new Plane(hit1.normal,  pointBefore);
                    Plane pointplane2 = new Plane(hit2.normal,  pointBefore);
                    middle = pointplane1.ClosestPointOnPlane(middle);
                    middle = pointplane2.ClosestPointOnPlane(middle);
                    startingEdge.normal1 = hit1.normal;
                    startingEdge.normal2 = hit2.normal;
                }

                if(showCornerVectors)
                    Debug.DrawRay(middle, Vector3.Cross(hit2.normal,hit1.normal), Color.red);
            }
            
            if(Physics.Raycast(startingMiddle, (middle-startingMiddle), out hit1, (startingMiddle-middle).magnitude+0.1f, ropeLayers)) 
            {
                if(showCornerVectors)
                    Debug.DrawRay(startingMiddle,(middle-startingMiddle).normalized*ropeRadious*2, Color.white);           
                startingEdge.point = hit1.point - (middle-startingMiddle).normalized*ropeRadious*2; 
            } 
            else
                startingEdge.point = middle;

            Vector3 dir1 = pointBefore-starting;
            Vector3 dir2 = ending-pointBefore;
            Vector3 up = Vector3.Cross(startingEdge.normal1, startingEdge.normal2);
            dir1 = Vector3.ProjectOnPlane(dir1, up);
            dir2 = Vector3.ProjectOnPlane(dir2,up);
            float Angle = Vector3.SignedAngle(dir1, dir2, up);
            bool collides = OneCollides(starting, ending, startingEdge, pointBefore);
            
            if(Angle < cornerAngleTrehsold || startingEdge.normal1 == -startingEdge.normal2 || !collides)
            {
                beforeSegment.inBetween.AddRange(currrentSegment.inBetween);
                beforeSegment.endingEdge = currrentSegment.endingEdge;
                segments.RemoveAt(i);
                i-=1;
                continue;
            }
            float distance = (startingEdge.point - endingEdge.point).magnitude;
            if(distance < ropeRadious/2)
            {
                beforeSegment.inBetween.AddRange(currrentSegment.inBetween);
                beforeSegment.endingEdge = currrentSegment.endingEdge;
                segments.RemoveAt(i);
                i-=1;
                continue;
            }
            /*
            else
            {
                Vector3 dir1 = pointBefore-starting;
                Vector3 dir2 = ending-pointBefore;
                Vector3 up = Vector3.Cross(startingEdge.normal1, startingEdge.normal2);
                dir1 = Vector3.ProjectOnPlane(dir1, up);
                dir2 = Vector3.ProjectOnPlane(dir2,up);
                float Angle = Vector3.SignedAngle(dir1, dir2, up);
                bool collides = OneCollides(starting, ending, startingEdge);
                if(((Angle < cornerAngleTrehsold) && collides) || !collides || startingEdge.normal1 == -startingEdge.normal2)
                {            
                    //beforeSegment.inBetween.AddRange(currrentSegment.inBetween);
                    //beforeSegment.endingEdge = currrentSegment.endingEdge;
                    //segments.RemoveAt(i);
                    //i-=1;
                    continue;
                }
               
            }
            if(Physics.Raycast(startingMiddle, (middle-startingMiddle), out hit1, (startingMiddle-middle).magnitude+0.1f, ropeLayers)) 
            {
                if(showCornerVectors)
                    Debug.DrawRay(startingMiddle,(middle-startingMiddle).normalized*ropeRadious*2, Color.white);           
                startingEdge.point = hit1.point - (middle-startingMiddle).normalized*ropeRadious*2; 
            }           
            else
                startingEdge.point = middle;
            float distance = (startingEdge.point - endingEdge.point).magnitude;
            if(distance < distanceBetweenPoints/2)
            {
                beforeSegment.inBetween.AddRange(currrentSegment.inBetween);
                beforeSegment.endingEdge = currrentSegment.endingEdge;
                segments.RemoveAt(i);
                i-=1;
                continue;
            }
        }
    }
    /*
    void MoveCorners()
    {
        for(int i = 1; i<segments.Count; i++)
        {
            Vector3 starting;
            Vector3 ending;
            Segment currrentSegment = segments[i];
            Segment beforeSegment = segments[i-1];
            Edge startingEdge = currrentSegment.startingEdge;
            Edge endingEdge = currrentSegment.endingEdge;
            if(currrentSegment.inBetween.Count>0)
            {
                starting = currrentSegment.inBetween[0];
                if(Vector3.Distance(starting, startingEdge.point) < distanceBetweenPoints/2 && currrentSegment.inBetween.Count > 1)
                    starting = currrentSegment.inBetween[1];
            }           
            else
            {
                starting = currrentSegment.endingEdge.point;
            }   
            
            if(beforeSegment.inBetween.Count>0)            
            {
                ending = beforeSegment.inBetween[beforeSegment.inBetween.Count-1];  
                if(Vector3.Distance(ending, startingEdge.point) < distanceBetweenPoints/2 && beforeSegment.inBetween.Count > 1)
                    ending = beforeSegment.inBetween[beforeSegment.inBetween.Count-2];   
            }       
            else
            {
                ending = beforeSegment.startingEdge.point;
            }

            Vector3 pointBefore = currrentSegment.startingEdge.point;
            RaycastHit hit1;
            RaycastHit hit2;
            Vector3 relocatedStart = pointBefore + (starting-pointBefore).normalized*cornerDistanceTrehsold - startingEdge.normal1*ropeRadious;
            Vector3 reloactedEnd = pointBefore + (ending-pointBefore).normalized*cornerDistanceTrehsold - startingEdge.normal2*ropeRadious;
            Vector3 betweenDirection = reloactedEnd-relocatedStart;
            Vector3 middle = (starting+ending)/2;
            Vector3 startingMiddle = pointBefore;
            if(showCornerVectors)
            {
                Debug.DrawRay(pointBefore, startingEdge.normal1, Color.yellow*2);
                Debug.DrawRay(pointBefore, startingEdge.normal2, Color.green*2);
                Debug.DrawLine(relocatedStart, reloactedEnd, Color.magenta*2);
                Debug.DrawLine(relocatedStart, pointBefore + (starting-pointBefore).normalized*cornerDistanceTrehsold, Color.black*2);
                Debug.DrawLine(reloactedEnd, pointBefore + (ending-pointBefore).normalized*cornerDistanceTrehsold, Color.black*2);
            }
            if(TwoWayRaycast(relocatedStart, betweenDirection, reloactedEnd, -betweenDirection, out hit1, out hit2, betweenDirection.magnitude))
            {
                Plane pointplane1 = new Plane(hit1.normal,  pointBefore);
                Plane pointplane2 = new Plane(hit2.normal,  pointBefore);
                middle = pointplane1.ClosestPointOnPlane(middle);
                middle = pointplane2.ClosestPointOnPlane(middle);
                if(showCornerVectors)
                    Debug.DrawRay(middle, Vector3.Cross(hit2.normal,hit1.normal), Color.red);
                if(hit1.normal != -hit2.normal)
                {
                    startingEdge.normal1 = hit1.normal;
                    startingEdge.normal2 = hit2.normal;
                }
            }
            else
            {
                Vector3 dir1 = pointBefore-starting;
                Vector3 dir2 = ending-pointBefore;
                Vector3 up = Vector3.Cross(startingEdge.normal1, startingEdge.normal2);
                dir1 = Vector3.ProjectOnPlane(dir1, up);
                dir2 = Vector3.ProjectOnPlane(dir2,up);
                float Angle = Vector3.SignedAngle(dir1, dir2, up);
                bool collides = OneCollides(starting, ending, startingEdge);
                if(((Angle < cornerAngleTrehsold) && collides) || !collides || startingEdge.normal1 == -startingEdge.normal2)
                {            
                    //beforeSegment.inBetween.AddRange(currrentSegment.inBetween);
                    //beforeSegment.endingEdge = currrentSegment.endingEdge;
                    //segments.RemoveAt(i);
                    //i-=1;
                    continue;
                }
                /*
                else
                {
                    
                    Plane pointplane1 = new Plane(startingEdge.normal1,  pointBefore);
                    Plane pointplane2 = new Plane(startingEdge.normal2,  pointBefore);
                    middle = pointplane1.ClosestPointOnPlane(middle);
                   // middle = pointplane2.ClosestPointOnPlane(middle);
                }
            }
            if(Physics.Raycast(startingMiddle, (middle-startingMiddle), out hit1, (startingMiddle-middle).magnitude+0.1f, ropeLayers)) 
            {
                if(showCornerVectors)
                    Debug.DrawRay(startingMiddle,(middle-startingMiddle).normalized*ropeRadious*2, Color.white);           
                startingEdge.point = hit1.point - (middle-startingMiddle).normalized*ropeRadious*2; 
            }           
            else
                startingEdge.point = middle;
            float distance = (startingEdge.point - endingEdge.point).magnitude;
            if(distance < distanceBetweenPoints/2)
            {
                beforeSegment.inBetween.AddRange(currrentSegment.inBetween);
                beforeSegment.endingEdge = currrentSegment.endingEdge;
                segments.RemoveAt(i);
                i-=1;
                continue;
            }
        }
    }
    
    bool OneCollides(Vector3 starting, Vector3 ending, Edge edge, Vector3 pointBefore)
    {
        float angle1 = Vector3.Dot((starting-pointBefore).normalized, -edge.normal2);
        float angle2 = Vector3.Dot((ending-pointBefore).normalized, -edge.normal1);
        Vector3 projected1 = (starting-pointBefore).normalized*(ropeRadious*2/angle1);
        Vector3 projected2 = (ending-pointBefore).normalized*(ropeRadious*2/angle2);
        projected1 = Vector3.ProjectOnPlane(projected1, edge.normal1);
        projected2 = Vector3.ProjectOnPlane(projected2, edge.normal2);
        Vector3 relocatedStart = pointBefore + projected1;
        Vector3 relocatedEnd = pointBefore + projected2;
        if(showCornerVectors)
        {
            Debug.DrawRay(relocatedStart, -edge.normal1*ropeRadious*2, Color.cyan);
            Debug.DrawRay(relocatedEnd, -edge.normal2*ropeRadious*2, Color.blue);
        }

        if(Physics.Raycast(relocatedStart, -edge.normal1, ropeRadious*2, ropeLayers) || Physics.Raycast(relocatedEnd, -edge.normal2, ropeRadious*2, ropeLayers))
        {
            return true;
        }
        else
            return false;
    }
    
    void VisualizeSegments(List<Segment> segments, Vector3 endingPoint)
    {
        int currentAmount = 0;
        allPoints.Reverse();
        for(int x = 0; x < segments.Count; x++)
        {
            Segment segment = segments[x];
            if(x == 0)
                AddPoint(segment.startingEdge.point,ref currentAmount, true);
            else
                AddPoint(segment.startingEdge.point,ref currentAmount, false);
            for(int z = 0; z < segment.inBetween.Count; z++)
            {
                AddPoint(segment.inBetween[z], ref currentAmount, false);
            }
        }
        AddPoint(endingPoint, ref currentAmount, true);
        if(currentAmount < allPoints.Count)        
            allPoints.RemoveRange(currentAmount, allPoints.Count-currentAmount);     
        allPoints.Reverse();   
        lineRenderer.positionCount = allPoints.Count;
        lineRenderer.SetPositions(allPoints.ToArray());
    }
    void AddPoint(Vector3 point, ref int i, bool instant)
    {       
        if(i >= allPoints.Count)
        {
            allPoints.Add(point);
        }
        else
        {
            if(instant)
                allPoints[i] = point;
            else
                allPoints[i] = Vector3.Lerp(allPoints[i], point, Time.deltaTime*lineRendererSmoothness);
        }
        i += 1;
    }
    void CreateSegment(Edge startingEdge, Edge endingEdge, List<Vector3> inbetweens, int position)
    {
        Segment segment = new Segment();
        segment.startingEdge = startingEdge;
        segment.endingEdge = endingEdge;
        segment.inBetween = inbetweens;
        if(position < segments.Count)
            segments.Insert(position, segment);
        else
            segments.Add(segment);
    }
    bool TwoWayRaycast(Vector3 startPoint, Vector3 direction1, Vector3 endPoint, Vector3 direction2, out RaycastHit hit1, out RaycastHit hit2, float magnitude)
    {
        if(Physics.Raycast(startPoint, direction1, out hit1, magnitude, ropeLayers) && Physics.Raycast(endPoint, direction2, out hit2, magnitude, ropeLayers))  
            return true;
        else
        {
            hit1 = new RaycastHit();
            hit2 = hit1;
            return false;
        }
    }
    List<Vector3> EditSegment(Segment editable, Edge endingEdge, int lastInbetween)
    {
        editable.endingEdge = endingEdge;
        List<Vector3> sobres = editable.inBetween.GetRange(lastInbetween, editable.inBetween.Count-lastInbetween-1);
        editable.inBetween.RemoveRange(lastInbetween, editable.inBetween.Count-lastInbetween-1);
        return sobres;
    }
    void GenerateBetweenPoints(List<Segment> currentSegments)
    {
        currentLenghtOfRope = GetLength();
        currentSegments.Reverse();
        float lengthFound = 0;
        foreach(Segment segment in currentSegments)
        {
            lengthFound += GeneratePoint(segment, currentLenghtOfRope, lengthFound);
        }
        currentSegments.Reverse();
    }
    float GetLength()
    {
        float currentLenghtAmount = 0;
        for(int i = 1; i< allPoints.Count; i++)
        {
            currentLenghtAmount += (allPoints[i]-allPoints[i-1]).magnitude;
        }
        return currentLenghtAmount;
    }
    float GeneratePoint(Segment segment, float currentLenghtAmount, float currentFound)
    {
        Vector3 startingPoint = segment.startingEdge.point;
        Vector3 endingPoint = segment.endingEdge.point;
        float segmentLenght = (startingPoint-endingPoint).magnitude;
        int pointAmount = Mathf.FloorToInt((targetLength*segmentLenght/currentLenghtAmount)/distanceBetweenPoints)-1;
        if(pointAmount*distanceBetweenPoints < segmentLenght)
            pointAmount = Mathf.FloorToInt(segmentLenght/distanceBetweenPoints)-1;
        if(currentFound + pointAmount*distanceBetweenPoints > targetLength)
            pointAmount = Mathf.FloorToInt((targetLength - currentFound)/distanceBetweenPoints)-1;
        pointAmount = Mathf.Clamp(pointAmount, 0, maxPointsInSegment);
        if(segment.inBetween.Count < pointAmount)
        {
            if(segment.inBetween.Count>0)
            {
                if((endingPoint-segment.inBetween[segment.inBetween.Count-1]).magnitude > distanceBetweenPoints-0.1f)
                {   
                    Vector3 position = endingPoint + (startingPoint-endingPoint).normalized*(distanceBetweenPoints-0.3f);//*i*distanceBetweenPoints;
                    segment.inBetween.Add(position);
                }
            }
            else
            {
                Vector3 position = endingPoint + (startingPoint-endingPoint).normalized*distanceBetweenPoints;//*i*distanceBetweenPoints;
                segment.inBetween.Add(position);
            }
        }
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
        float realLength = temporalPoints.Count*distanceBetweenPoints;
        temporalPoints.RemoveAt(0);
        temporalPoints.RemoveAt(temporalPoints.Count-1);
        for(int i = 0; i< temporalPoints.Count;i++)
        {
            if((segment.inBetween[i]-temporalPoints[i]).magnitude > inBetweenDistanceTreshold)
            {
                segment.inBetween[i] = temporalPoints[i]; //Vector3.Lerp(segment.inBetween[i], temporalPoints[i], Time.deltaTime*smoothness);
            }
        }
        return realLength;
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
                if(showRopeKinematicLines)
                    Debug.DrawLine(trans1,trans2, Color.black);
                positionsTemp[i] = trans1;
                if(showRopeKinematicLines)
                    Debug.DrawRay(trans1,Vector3.up, Color.green);
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
                Edge middlePoint = CollisionDetectionBetweenPoints(ref trans1, trans2);
                float distanceBetween = Vector3.Distance(trans2,trans1);
                float minDistance = distanceBetweenPoints;
                Vector3 directionForward = trans2-trans1;
                if(middlePoint != null)
                {
                    middles.Add(new MiddlePoint(middlePoint, positionsTemp.Count-1-i));
                    distanceBetween = Vector3.Distance(trans1,middlePoint.point);
                    minDistance -= Vector3.Distance(middlePoint.point,trans2);      
                    if(canStrech)
                    {
                        if(distanceBetween>minDistance)
                            trans1 += (distanceBetween-minDistance)*(middlePoint.point-trans1).normalized;  
                    }
                    else
                        trans1 += (distanceBetween-minDistance)*(middlePoint.point-trans1).normalized;
                }
                
                if(canStrech)
                {
                    if(Vector3.Distance(trans2,trans1)>distanceBetweenPoints)
                        trans1 += (Vector3.Distance(trans2,trans1)-distanceBetweenPoints)*(directionForward).normalized;  
                }
                else
                    trans1 += (Vector3.Distance(trans2,trans1)-distanceBetweenPoints)*(directionForward).normalized; 

                if(showRopeKinematicLines)
                    Debug.DrawLine(trans1,trans2, Color.magenta);
                positionsTemp[i] = trans1;  
                if(showRopeKinematicLines)                
                    Debug.DrawRay(trans1,Vector3.up, Color.cyan);     
            }
        }
        positionsTemp.Reverse();
        middles.Reverse();
        return middles;
    }
    Edge CollisionDetectionBetweenPoints(ref Vector3 forwardPoint, Vector3 backPoint)
    {
        RaycastHit hit1;
        RaycastHit hit2;
        Vector3 directionVector = forwardPoint-backPoint;
        if(TwoWayRaycast(backPoint, directionVector, forwardPoint, -directionVector, out hit1, out hit2, directionVector.magnitude))
        {
            Plane plane1 = new Plane(hit1.normal, hit1.point);
            Plane plane2 = new Plane(hit2.normal, hit2.point);
            Vector3 middle = (hit1.point+hit2.point)/2;
            middle = plane1.ClosestPointOnPlane(middle);
            middle = plane2.ClosestPointOnPlane(middle);
            middle += (hit1.normal+hit2.normal).normalized*ropeRadious*2;
            float distance = Vector3.Distance(backPoint, middle);
            Debug.DrawLine(backPoint, middle);
            Debug.DrawRay(backPoint, Vector3.up, Color.red);
            if(distance< distanceBetweenPoints+0.1f)
            {
                return new Edge(middle, hit1.normal, hit2.normal,hit1.transform);       
            }          
        }
        if(Physics.Raycast(backPoint, directionVector, out hit1, directionVector.magnitude, ropeLayers))
        {
            Plane plane = new Plane(hit1.normal, hit1.point+hit1.normal*ropeRadious);
            forwardPoint = plane.ClosestPointOnPlane(forwardPoint);
            return null;
        }
        else
            return null;
    }
}*/

