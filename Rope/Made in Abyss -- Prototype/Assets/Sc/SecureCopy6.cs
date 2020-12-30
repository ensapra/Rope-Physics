using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public class SecureCopy6 : MonoBehaviour
{
    [Header("Rope Settings")]
    public int maxPointsInSegment = 100;
    public float cornerAngleTrehsold = 10;
    public float cornerDistanceTrehsold = 0.1f;
    public float ropeRadious = 0.2f;
    public float grabDistance = 1.5f;
    public float speedGrab = 0.5f;
    public float distanceBetweenPoints = 1;
    public float lineRendererSmoothness = 10;
    public bool canStrech = true;
    public LayerMask ropeLayers;
    public Material ropeMaterial;
    public float ropeGravity = 15;

    [Header("Results")]
    public List<Segment> segments;
    LineRenderer lineRenderer;
    List<Vector3> allPoints = new List<Vector3>();
    public float ropeTension;
    public float currentLenghtOfRope;
    public float currentLenghtOfSegments;
    float timesincelastReduce;
    public float targetLength;
    Coroutine updateCorners;
    
    [Header("DEBUG OPTIONS")]
    public bool showRopeKinematicLines;
    public bool showCornerVectors;
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if(lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.widthMultiplier = ropeRadious*2;
        ropeLayers = LayerMask.GetMask("Default");
        lineRenderer.material = ropeMaterial;
    }

    public void GenerateTwoPointRope(Vector3 startingPoint, Vector3 endingPoint, float distance)
    {
        targetLength = distance;       
        //SetRopeTension();
        GetSegments(startingPoint, endingPoint);
        if(updateCorners == null)
            updateCorners = StartCoroutine(UpdateCorners());
        GenerateBetweenPoints(segments);
        VisualizeSegments(segments, endingPoint); 
    }
    public void SetRopeTension()
    {
        ropeTension = currentLenghtOfSegments/currentLenghtOfRope;
        ropeTension = Mathf.Clamp(ropeTension, 0, 1);
    }
    public void GenerateRope(Vector3 startingPosition, Vector3 endingPosition, bool pinged, bool grabbed, float maxPossibleLenght)
    {
        if(pinged)
        {
            //SetRopeTension();
        }
        else        
            targetLength = currentLenghtOfSegments;        

        if(!grabbed)
        {
            ReduceLength();
        }

        if(targetLength > maxPossibleLenght)
            targetLength = maxPossibleLenght;            
        
        GetSegments(startingPosition, endingPosition);
        if(updateCorners == null)
            updateCorners = StartCoroutine(UpdateCorners());
        GenerateBetweenPoints(segments);
        VisualizeSegments(segments, endingPosition);        
    }
    IEnumerator UpdateCorners()
    {
        yield return new WaitForFixedUpdate();
        MoveCorners();
        updateCorners = null;
    }
    void ReduceLength()
    {
        if(targetLength < currentLenghtOfSegments)
            targetLength = currentLenghtOfSegments;
        else
        {
            if(timesincelastReduce > speedGrab)
            {
                timesincelastReduce = 0;
                if(targetLength-grabDistance > currentLenghtOfSegments)
                    targetLength -= grabDistance;
                else
                    targetLength = currentLenghtOfSegments;
            }
            else
                timesincelastReduce += Time.deltaTime;
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
                if(startingPoint.realPosition != null)
                    startingPoint.point = startingPoint.realPosition.position;
                int lastPoint = 0;
                int initialX = -1;
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

                        if(initialX+1 <= 0)
                            middleSegment.startingEdge = startingPoint;
                        else
                            middleSegment.startingEdge = middlesPoints[initialX].edge;
                        
                        if(x == middlesPoints.Count)
                            middleSegment.endingEdge = endingPoint;
                        else
                            middleSegment.endingEdge =  middlesPoints[x].edge;
                        
                        if(currentPoint-lastPoint > 0)
                            middleSegment.inBetween.AddRange(inbetweens.GetRange(lastPoint, currentPoint-lastPoint));
                        lastPoint = currentPoint;
                        if(Vector3.Distance(middleSegment.startingEdge.point, middleSegment.endingEdge.point) > 0.1f) 
                        {
                            if(x < middlesPoints.Count)                            
                                middleSegment.endingEdge.GeneratePoint();                            
                            temporalSegments.Add(middleSegment);
                            initialX = x;
                        }
                    }
                }
                else
                    temporalSegments.Add(segments[i]);
                
            }
            segments = temporalSegments;
        }
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
            Vector3 pointBefore = currrentSegment.startingEdge.realPosition.position;
            if(currrentSegment.inBetween.Count>0)
            {
                starting = currrentSegment.inBetween[0];
                if(Vector3.Distance(starting, startingEdge.point) < distanceBetweenPoints/2 && currrentSegment.inBetween.Count > 1)
                    starting = currrentSegment.inBetween[1];
            }           
            else
            {
                if(currrentSegment.endingEdge.realPosition != null)
                    starting = currrentSegment.endingEdge.realPosition.position;
                else
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
                if(beforeSegment.startingEdge.realPosition != null)
                    ending = beforeSegment.startingEdge.realPosition.position;
                else
                    ending = beforeSegment.startingEdge.point;
            }
            RaycastHit hit1;
            RaycastHit hit2;
            float angle1 = Vector3.Dot((starting-pointBefore).normalized, -startingEdge.normal2);
            float angle2 = Vector3.Dot((ending-pointBefore).normalized, -startingEdge.normal1);
            Vector3 projected1 = (starting-pointBefore).normalized*(ropeRadious*2/angle1);
            Vector3 projected2 = (ending-pointBefore).normalized*(ropeRadious*2/angle2);
            projected1 = Vector3.ProjectOnPlane(projected1, startingEdge.normal1);
            projected2 = Vector3.ProjectOnPlane(projected2, startingEdge.normal2);
            Vector3 relocatedStart = pointBefore + projected1;
            Vector3 relocatedEnd = pointBefore + projected2;
            Vector3 middle = (starting+ending)/2;
            bool collided = false; 
            
            if(showCornerVectors)
            {
                Debug.DrawRay(relocatedStart, -startingEdge.normal1*ropeRadious*2, Color.cyan);
                Debug.DrawRay(relocatedEnd, -startingEdge.normal2*ropeRadious*2, Color.blue);
            }
                            
            if(Physics.Raycast(relocatedEnd, -startingEdge.normal2,out hit2, ropeRadious*2, ropeLayers) && Physics.Raycast(relocatedStart, -startingEdge.normal1,out hit1, ropeRadious*2, ropeLayers))
            {
                if(hit1.normal != -hit2.normal)
                {
                    collided = true;
                    Plane pointplane1 = new Plane(hit1.normal,  hit1.point);
                    Plane pointplane2 = new Plane(hit2.normal,  hit2.point);
                    middle = pointplane1.ClosestPointOnPlane(middle);
                    middle = pointplane2.ClosestPointOnPlane(middle);
                    middle += (startingEdge.normal1+startingEdge.normal2) * ropeRadious;
                    startingEdge.realPosition.position = (middle+pointBefore)/2;
                    startingEdge.normal1 = hit1.normal;
                    startingEdge.normal2 = hit2.normal;
                }
            }    

            Vector3 dir1 = pointBefore-starting;
            Vector3 dir2 = ending-pointBefore;
            Vector3 up = Vector3.Cross(startingEdge.normal1, startingEdge.normal2);
            dir1 = Vector3.ProjectOnPlane(dir1, up);
            dir2 = Vector3.ProjectOnPlane(dir2,up);
            float Angle = Vector3.SignedAngle(dir1, dir2, up);
            float distance = (startingEdge.point - endingEdge.point).magnitude;
            float moveDistance = (startingEdge.point.y-startingEdge.realPosition.position.y);
            if(Angle < cornerAngleTrehsold | startingEdge.normal1 == -startingEdge.normal2 || distance < ropeRadious/2 || !collided ||moveDistance > ropeGravity*Time.deltaTime) 
            {              
                Debug.LogError("yup");
                beforeSegment.inBetween.Add(currrentSegment.startingEdge.realPosition.position);
                beforeSegment.inBetween.AddRange(currrentSegment.inBetween);
                beforeSegment.endingEdge.DestroyPoint();
                beforeSegment.endingEdge = currrentSegment.endingEdge;
                segments.RemoveAt(i);
                i-=1;
                continue;
            }
        }
    }
    void VisualizeSegments(List<Segment> segments, Vector3 endingPoint)
    {
        int currentAmount = 0;
        for(int x = 0; x < segments.Count; x++)
        {
            Segment segment = segments[x];
            if(x == 0)
                AddPoint(segment.startingEdge.point,ref currentAmount, true);
            else
                AddPoint(segment.startingEdge.point,ref currentAmount, true);
            for(int z = 0; z < segment.inBetween.Count; z++)
                AddPoint(segment.inBetween[z], ref currentAmount, false);
            
        }
        AddPoint(endingPoint, ref currentAmount, true);
        if(currentAmount < allPoints.Count)        
            allPoints.RemoveRange(1, allPoints.Count-currentAmount);   
        ShowLineRenderer(allPoints); 
    }
    void ShowLineRenderer(List<Vector3> points)
    {
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(allPoints.ToArray());
    }
    void AddPoint(Vector3 point, ref int i, bool instant)
    {       
        if(i >= allPoints.Count)        
            allPoints.Add(point);        
        else
        {
            if(instant)            
                allPoints[i] = point;            
            else            
            {
                allPoints[i] = Vector3.Lerp(allPoints[i], point, Time.deltaTime*lineRendererSmoothness);
            }
        }
        i += 1;
    }
    void CreateSegment(Edge startingEdge, Edge endingEdge, List<Vector3> inbetweens, int position)
    {
        Segment segment = new Segment();
        segment.startingEdge = startingEdge;
        segment.endingEdge = endingEdge;
        segment.inBetween = inbetweens;
        segment.realLength = (startingEdge.point-endingEdge.point).magnitude;
        segment.segmentLenght = (startingEdge.point-endingEdge.point).magnitude;
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
    void GenerateBetweenPoints(List<Segment> currentSegments)
    {
        currentLenghtOfRope = GetLength();
        currentLenghtOfSegments = GetLengthSegments();
        currentSegments.Reverse();
        float theoricEndLength = currentSegments[currentSegments.Count-1].segmentLenght;
        float lengthFound = 0;
        for(int i = 0; i< currentSegments.Count; i++)
        {
            lengthFound += GeneratePoint(segments[i], lengthFound, i== currentSegments.Count-1, theoricEndLength);        
        }
        currentSegments.Reverse();
    }
    float GetLength()
    {
        float currentLenghtOfRope = 0;
        for(int i = 1; i< allPoints.Count-1; i++) //Aquest -1 es pot treure segurament
        {
            currentLenghtOfRope += (allPoints[i]-allPoints[i-1]).magnitude;
        }
        return currentLenghtOfRope;
    }
    float GetLengthSegments()
    {
        float currentLenghtOfSegments = 0;
        for(int i = 0; i < segments.Count; i++)
        {
            float distance = (segments[i].startingEdge.point-segments[i].endingEdge.point).magnitude;
            segments[i].segmentLenght = distance;
            currentLenghtOfSegments += distance;
        }
        return currentLenghtOfSegments;
    }
    float GeneratePoint(Segment segment, float currentLengthFound, bool lastOne, float theoricEndLength)
    {   
        Vector3 startingPoint = segment.startingEdge.point;
        Vector3 endingPoint = segment.endingEdge.point;
        float segmentLenght = segment.segmentLenght;
        Vector3 fakeStartingPoint = startingPoint;
        Vector3 segmentDirection = (endingPoint-startingPoint).normalized;
        if(lastOne)
        {
            float tempSegLen = (targetLength-currentLengthFound);
            ropeTension = (segmentLenght-tempSegLen+distanceBetweenPoints)/(distanceBetweenPoints);
            ropeTension = Mathf.Clamp(ropeTension,0,1);
            if(segmentLenght+currentLengthFound >= targetLength)
            {
                if(ropeTension >= 1)
                    fakeStartingPoint = endingPoint-segmentDirection*tempSegLen; //Podriem dir que ha petat la corda o algo per l'estil
            }
            segmentLenght = targetLength-currentLengthFound;
        }
        int currentAmount = segment.inBetween.Count;
        int pointAmount = currentAmount; 
        float extra = -1;
        pointAmount = Mathf.FloorToInt(segmentLenght/distanceBetweenPoints);
        //pointAmount += Mathf.FloorToInt(targetLength-segmentLenght/distanceBetweenPoints);

        extra = segmentLenght-(pointAmount)*distanceBetweenPoints;
        if(extra <= 0)
            pointAmount -= 1;
        /*
        if(!lastOne)             
        {
            pointAmount = Mathf.FloorToInt(((targetLength-theoricEndLength)*segment.realLength/currentLenghtOfRope)/distanceBetweenPoints)-1;
            if(pointAmount*distanceBetweenPoints <= segmentLenght)            
                pointAmount = Mathf.FloorToInt(segment.segmentLenght/distanceBetweenPoints)-1;   
            //extra = segmentLenght-(pointAmount)*distanceBetweenPoints;
            //if(extra > 0)
               //pointAmount += 1;     
        }    
        else      
        {
            pointAmount = Mathf.FloorToInt((targetLength-currentFound)/distanceBetweenPoints);  
            extra = segmentLenght-(pointAmount)*distanceBetweenPoints;
            if(extra > 0)
                pointAmount += 1;
            if(ropeTension > 0.99f)
                pointAmount -= 1;
        } 

        pointAmount = Mathf.Clamp(pointAmount, 0, maxPointsInSegment);
        if(segment.inBetween.Count < pointAmount)
        {         
            if(segment.inBetween.Count>0)
            {
                if((endingPoint-segment.inBetween[segment.inBetween.Count-1]).magnitude > distanceBetweenPoints-0.1f)                    
                    segment.inBetween.Add(endingPoint+(fakeStartingPoint-segment.inBetween[0]).normalized*distanceBetweenPoints/2);
            }
            else    
            {            
                segment.inBetween.Add(fakeStartingPoint + (endingPoint-fakeStartingPoint).normalized*distanceBetweenPoints);
            }
        }
        else 
        if(segment.inBetween.Count >= pointAmount)          
            segment.inBetween.RemoveRange(0, segment.inBetween.Count-pointAmount);            
        List<Vector3> temporalPoints = new List<Vector3>(segment.inBetween);      
        temporalPoints.Insert(0, fakeStartingPoint);
        temporalPoints.Add(endingPoint);
        float tens = (Mathf.Clamp(ropeTension,0.95f,1f)*20-19);
        Debug.DrawRay(temporalPoints[0], Vector3.up, Color.cyan);
        Debug.DrawRay(temporalPoints[temporalPoints.Count-1], Vector3.up, Color.magenta);
        ApplyTension(segment, temporalPoints);
        StartToEnd(temporalPoints, extra);
        
        temporalPoints.Insert(0, fakeStartingPoint);
        segment.inBetween.Insert(0, fakeStartingPoint);
        
        segment.middlePoints = EndToStart(temporalPoints, extra);
        
        float realLength = 0;
        temporalPoints.RemoveAt(0);
        temporalPoints.RemoveAt(temporalPoints.Count-1);

        if(temporalPoints.Count > 0)
        {
            if(!lastOne)
                realLength += (temporalPoints[0]-startingPoint).magnitude;
            realLength += (temporalPoints[temporalPoints.Count-1]-endingPoint).magnitude;
            for(int i = 0; i< temporalPoints.Count;i++)
            {
                if(i > 0)
                        realLength += (temporalPoints[i]-temporalPoints[i-1]).magnitude;
                segment.inBetween[i] = temporalPoints[i];                
            }
        }
        else
            realLength += (startingPoint-endingPoint).magnitude;
        segment.realLength = realLength;
        return realLength;
    }
    void ApplyTension(Segment segment, List<Vector3> pointsTemp)
    {
        for(int i = 1; i< pointsTemp.Count-1; i++)
        {
            Vector3 point = segment.startingEdge.point;
            Vector3 trans1 = pointsTemp[i];
            Vector3 directionPlane = segment.startingEdge.point-segment.endingEdge.point;
            Plane plane1 = new Plane(directionPlane, trans1);
            Vector3 pointInPlaneClose = plane1.ClosestPointOnPlane(segment.startingEdge.point);
            Vector3 gravityVector = Vector3.down*ropeGravity*Time.deltaTime;            
            Vector3 tensionPoint = Vector3.Lerp(trans1+gravityVector, pointInPlaneClose, ropeTension);
            directionPlane = tensionPoint-trans1;
            RaycastHit hit;
            Collider[] collisions = Physics.OverlapSphere(tensionPoint, ropeRadious, ropeLayers);
            if(Physics.Raycast(trans1, directionPlane, out hit, directionPlane.magnitude+ropeRadious,ropeLayers))            
                trans1 = hit.point-directionPlane.normalized*ropeRadious;            
            else if(collisions.Length <= 0)            
                trans1 = tensionPoint;            
            pointsTemp[i] = trans1;

            /*
            Vector3 trans1 = pointsTemp[i];
            float gravityMult = 0;
            if(i >= 1 && i <= pointsTemp.Count-2)
                gravityMult = (pointsTemp[i]-currentPoints[i-1]).magnitude;
            gravityMult = Mathf.Clamp(1-gravityMult,0, 1) * tens;
            Vector3 gravity = Vector3.down*ropeGravity*Time.deltaTime*(1);
            Collider[] collisions = Physics.OverlapSphere(trans1, 0.21f, ropeLayers);
            if(!Physics.Raycast(trans1, Vector3.down, gravity.magnitude*Time.deltaTime,ropeLayers) && collisions.Length <= 0)
            {
                trans1 += gravity;
                pointsTemp[i] = trans1;
            }
        }
    }
    void StartToEnd(List<Vector3> positionsTemp, float extra)
    {
        int max = positionsTemp.Count;
        for(int i = 1; i<max-1; i++)
        {
            Vector3 trans1 = positionsTemp[i];
            Vector3 trans2 = positionsTemp[i-1];
            float between = Vector3.Distance(trans2,trans1);
            float tempDistance = distanceBetweenPoints;
            Vector3 directionForward = -(trans2-trans1).normalized;
            if(i == 1 && extra > 0)
                tempDistance *= extra;
            if(canStrech)
            {
                if(between>tempDistance)
                    trans1 = trans2 + (tempDistance)*(directionForward).normalized;  
            }
            else
                trans1 = trans2 + (tempDistance)*(directionForward).normalized;  
            
            if(showRopeKinematicLines)
            {
                Debug.DrawLine(trans1,trans2, Color.black);
                Debug.DrawRay(trans1,Vector3.up*1.2f, Color.red);
            }
            positionsTemp[i] = trans1;            
        }
    }
    List<MiddlePoint> EndToStart(List<Vector3> positionsTemp, float extra)
    {        
        positionsTemp.Reverse();
        List<MiddlePoint> middles = new List<MiddlePoint>();
        for(int i = 1; i<positionsTemp.Count; i++)
        {
            Vector3 trans1 = positionsTemp[i];
            Vector3 trans2 = positionsTemp[i-1];                 
            Edge middlePoint = CollisionDetectionBetweenPoints(ref trans1, trans2);
            float distanceBetween = Vector3.Distance(trans2,trans1);
            float minDistance = distanceBetweenPoints;
            float tempDistance = distanceBetweenPoints;
            if(i == positionsTemp.Count-1 && extra > 0)
                tempDistance *= extra;
            //Vector3 directionForward = trans2-trans1;
            Vector3 directionForward = (trans2-trans1).normalized;
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
            float distance = Vector3.Distance(trans2,trans1);
            if(canStrech)
            {
                if(distance>tempDistance)
                    trans1 += (distance-tempDistance)*(directionForward).normalized;  
            }
            else
                trans1 += (distance-tempDistance)*(directionForward).normalized; 

            if(showRopeKinematicLines)
            {
                Debug.DrawLine(trans1,trans2, Color.magenta);
                Debug.DrawRay(trans1,Vector3.up, Color.cyan);     
            }
            positionsTemp[i] = trans1;     
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
            if(distance < distanceBetweenPoints*2)
            {
                return new Edge(middle, hit1.normal, hit2.normal, hit1.transform);       
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
    Vector3 randomVector3(Vector3 min, Vector3 max, float detail)
    {
        float x = Random.Range(min.x, max.x);
        float y = Random.Range(min.y, max.y);
        float z = Random.Range(min.z, max.z);
        return new Vector3(x,y,z)/detail;
    }
}*/
