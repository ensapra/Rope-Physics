using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeGenerator2 : MonoBehaviour
{
    [Header("Rope Settings")]
    public float ropeGravity = 4;
    public float ropeAirFriction = 0.97f;
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

    [Header("Results")]
    public List<Segment> segments;
    public float ropeTension;
    public float currentLenghtOfSegments;
    public float currentGeneralLength;
    public float targetLength;
    List<Vector3> allPoints = new List<Vector3>();

    [Header("Components")]
    LineRenderer lineRenderer;
    Coroutine updateCorners;
    float timesincelastReduce;
    
    [Header("DEBUG OPTIONS")]
    public bool showRopeKinematicLines;
    public bool showCornerVectors;

#region Initialization
    void Start()
    {
        GenerateComponents();
    }

    void GenerateComponents()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if(lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.widthMultiplier = ropeRadious*2;
        ropeLayers = LayerMask.GetMask("Default");
        lineRenderer.material = ropeMaterial;
    }
#endregion

#region Call Functions
    public void GenerateTwoPointRope(Vector3 startingPoint, Vector3 endingPoint, float distance)
    {
        targetLength = distance;       
        GetSegments(startingPoint, endingPoint);
        GeneratePoints(segments);
        VisualizeSegments(segments, endingPoint); 
        if(updateCorners == null)
            updateCorners = StartCoroutine(UpdateCorners());
    }

    public void GenerateRope(Vector3 startingPosition, Vector3 endingPosition, bool pinged, bool grabbed, float maxPossibleLenght)
    {
        //NEEDS TO ME MODIFIED
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
        GeneratePoints(segments);
        VisualizeSegments(segments, endingPosition);        
    }
#endregion

#region  Specific Functions
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
    
#endregion

    void GetSegments(Vector3 startingPosition, Vector3 endingPosition)
    {
        if(segments.Count <= 0)
        {
            Edge StartingEdge = new Edge(startingPosition, Vector3.up, Vector3.up, null);
            Edge EndingEdge = new Edge(endingPosition, Vector3.up, Vector3.up, null);
            CreateSegment(StartingEdge, EndingEdge, new List<InertiaPoint>(), 0);
        }
        else
        {
            segments[0].startingEdge.point = startingPosition;
            segments[segments.Count-1].endingEdge.point = endingPosition;
            List<Segment> temporalSegments = new List<Segment>();
            for(int i = 0; i < segments.Count; i++)
            {
                List<MiddlePoint> middlesPoints = segments[i].middlePoints;
                Edge startingPoint = segments[i].startingEdge;
                if(middlesPoints.Count > 0)                
                    temporalSegments.AddRange(AddMiddleSegments(segments[i], middlesPoints));
                else
                    temporalSegments.Add(segments[i]);
                
            }
            segments = temporalSegments;
        }
    }
    List<Segment> AddMiddleSegments(Segment segment, List<MiddlePoint> middlesPoints)
    {
        int lastPoint = 0;
        int initialX = -1;
        List<InertiaPoint> inbetweens = segment.inBetween;
        Edge startingPoint = segment.startingEdge;
        Edge endingPoint = segment.endingEdge;
        List<Segment> temp = new List<Segment>();
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
                temp.Add(middleSegment);
                initialX = x;
            }
        }
        return temp;
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
            pointBefore += (currrentSegment.startingEdge.previousPoint-pointBefore)*ropeAirFriction;
            currrentSegment.startingEdge.previousPoint = pointBefore;

            if(currrentSegment.inBetween.Count>0)
            {
                starting = currrentSegment.inBetween[0].point;
                if(Vector3.Distance(starting, startingEdge.point) < distanceBetweenPoints/2 && currrentSegment.inBetween.Count > 1)
                    starting = currrentSegment.inBetween[1].point;
            }           
            else            
                starting = currrentSegment.endingEdge.point;            
            
            if(beforeSegment.inBetween.Count>0)            
            {
                ending = beforeSegment.inBetween[beforeSegment.inBetween.Count-1].point;  
                if(Vector3.Distance(ending, startingEdge.point) < distanceBetweenPoints/2 && beforeSegment.inBetween.Count > 1)
                    ending = beforeSegment.inBetween[beforeSegment.inBetween.Count-2].point;   
            }       
            else            
                ending = beforeSegment.startingEdge.point;

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
            
            if(showCornerVectors)
            {
                Debug.DrawRay(relocatedStart, -startingEdge.normal1*ropeRadious*2, Color.cyan);
                Debug.DrawRay(relocatedEnd, -startingEdge.normal2*ropeRadious*2, Color.blue);
            }
            bool collided = false;  
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
                    startingEdge.point = (middle+pointBefore)/2;
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
            float moveDistance = (startingEdge.point.y-startingEdge.point.y);
            /*if(Angle < cornerAngleTrehsold | startingEdge.normal1 == -startingEdge.normal2 || distance < ropeRadious/2 || !collided || moveDistance > ropeGravity*Time.deltaTime) 
            {              
                Debug.LogError("yup");
                beforeSegment.inBetween.Add(currrentSegment.startingEdge.inertiaPoint);
                beforeSegment.inBetween.AddRange(currrentSegment.inBetween);
                beforeSegment.endingEdge.DestroyPoint();
                beforeSegment.endingEdge = currrentSegment.endingEdge;
                segments.RemoveAt(i);
                i-=1;
                continue;
            }*/
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
                AddPoint(segment.inBetween[z].point, ref currentAmount, false);
            
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
    void CreateSegment(Edge startingEdge, Edge endingEdge, List<InertiaPoint> inbetweens, int position)
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
        if(Physics.Raycast(startPoint,direction1, out hit1, magnitude, ropeLayers) && Physics.Raycast(endPoint, direction2, out hit2, magnitude, ropeLayers))  
            return true;
        else
        {
            hit1 = new RaycastHit();
            hit2 = hit1;
            return false;
        }
    }
    
    void GeneratePoints(List<Segment> currentSegments)
    {
        currentSegments.Reverse();
        currentLenghtOfSegments = 0;
        currentGeneralLength = GetGeneralLength();
        float theoricEndLength = currentSegments[currentSegments.Count-1].segmentLenght;
        float lengthFound = 0;
        for(int i = 0; i< currentSegments.Count; i++)
        {
            lengthFound += GeneratePoint(segments[i], lengthFound, i == currentSegments.Count-1, theoricEndLength);        
        }
        currentSegments.Reverse();
    }
    float GetGeneralLength()
    {
        float temp = 0;
        for(int i = 2; i < allPoints.Count; i++)
        {
            temp += (allPoints[i]-allPoints[i-1]).magnitude;
        }
        return temp;
    }
    float CalculateSegmentLength(Segment segment)
    {
        float length = (segment.startingEdge.point-segment.endingEdge.point).magnitude;
        segment.segmentLenght = length;
        currentLenghtOfSegments += length;
        return length;
    }
    InertiaPoint reduceStartingPoint(Segment segment, float currentLengthFound, ref float segmentLenght)
    {
        float tempSegLen = (targetLength-currentLengthFound);
        Vector3 startingPoint = segment.startingEdge.point;
        Vector3 endingPoint = segment.endingEdge.point;
        Vector3 segmentDirection = (endingPoint-startingPoint).normalized;

        ropeTension = (segmentLenght-tempSegLen+distanceBetweenPoints)/(distanceBetweenPoints);
        ropeTension = Mathf.Clamp(ropeTension,0,1);
        
        if(segmentLenght+currentLengthFound >= targetLength)
        {
            if(ropeTension >= 1)
                startingPoint = endingPoint-segmentDirection*tempSegLen; //Podriem dir que ha petat la corda o algo per l'estil
        }
        segmentLenght = targetLength-currentLengthFound;
        return new InertiaPoint(startingPoint, Vector3.zero, true);
    }

    int GetPointAmount(float segmentLenght, out float extra)
    {
        int pointAmount; 
        pointAmount = Mathf.FloorToInt(segmentLenght/distanceBetweenPoints);
        //pointAmount += Mathf.FloorToInt(targetLength-segmentLenght/distanceBetweenPoints);
        extra = segmentLenght-(pointAmount)*distanceBetweenPoints;
        if(extra > 0)
            pointAmount += 1;
        Debug.Log(pointAmount + " vs " + extra);
        return pointAmount;
    }
    float GeneratePoint(Segment segment, float currentLengthFound, bool lastOne, float theoricEndLength)
    {   
        float segmentLenght = CalculateSegmentLength(segment);
        InertiaPoint endingPoint = segment.endingEdge.inertiaPoint;
        InertiaPoint startingPoint = segment.startingEdge.inertiaPoint;
        if(lastOne)        
            startingPoint = reduceStartingPoint(segment, currentLengthFound, ref segmentLenght);        

        float extra = -1;
        int pointAmount = GetPointAmount(segmentLenght, out extra);      

        ModifyAmountOfPoints(segment, pointAmount, startingPoint.point);

        List<InertiaPoint> temporalPoints = new List<InertiaPoint>(segment.inBetween);      
        temporalPoints.Insert(0, startingPoint);
        temporalPoints.Add(endingPoint);
                
        ApplyVelocities(segment, temporalPoints);
        for(int i = 0; i < 10; i++)
        {
            SimpleTwoWay(temporalPoints, extra);
        }
        OneWayDetecion(temporalPoints, out segment.middlePoints, extra);

        //StartToEnd(temporalPoints, extra);   
        //EndToStart(temporalPoints, extra, out segment.middlePoints);

        temporalPoints.RemoveAt(0);
        temporalPoints.RemoveAt(temporalPoints.Count-1);

        return AssingPoints(temporalPoints, segment, lastOne);
    }
    
    void SimpleTwoWay(List<InertiaPoint> positionsTemp, float extra)
    {
        for(int i = 1; i< positionsTemp.Count; i++)
        {
            if(positionsTemp[i].fixedPoint)
                continue;
            Vector3 trans1 = positionsTemp[i].point;
            Vector3 trans2 = positionsTemp[i-1].point;                 
            float distanceBetween = Vector3.Distance(trans2,trans1);
            float tempDistance = distanceBetweenPoints;
            Vector3 directionForward = (trans2-trans1).normalized;
            float distance = Vector3.Distance(trans2,trans1);
            if(i == 1 && extra > 0)
                tempDistance *= extra;
            if(distance>tempDistance)
            {
                trans1 += (distance-tempDistance)*(directionForward).normalized/2;  
                trans2 -= (distance-tempDistance)*(directionForward).normalized/2;  
            }
            InertiaPoint current = positionsTemp[i];
            current.point = trans1;
            positionsTemp[i] = current;        
            InertiaPoint current2 = positionsTemp[i-1];
            current2.point = trans2;
            positionsTemp[i-1] = current2;      
        }
    }
    void OneWayDetecion(List<InertiaPoint> temporalPoints, out List<MiddlePoint> middlePoints, float extra)
    {
        List<MiddlePoint> middles = new List<MiddlePoint>();
        for(int i = 1; i<temporalPoints.Count; i++)
        {
            if(temporalPoints[i].fixedPoint)
                continue;
            Vector3 trans1 = temporalPoints[i].point;
            Vector3 trans2 = temporalPoints[i-1].point;                 
            float tempDistance = distanceBetweenPoints;  
            float distance = (trans1-trans2).magnitude;          
            if(i == temporalPoints.Count-1 && extra > 0)
                tempDistance *= extra;
            Edge temp = CollisionDetectionBetweenPoints(trans1, trans2);
            if(temp != null)
            {
                MiddlePoint middle = new MiddlePoint(temp, i);
                middles.Add(middle);  
            }     

            if(distance > tempDistance)     
            {       
                Debug.DrawRay(trans1, Vector3.up*2);
                trans1 += (trans1-trans2).normalized*(distance-tempDistance);   
            }         

            if(showRopeKinematicLines)
            {
                Debug.DrawLine(trans1,trans2, Color.magenta);
                Debug.DrawRay(trans1,Vector3.up, Color.cyan);     
            }
            InertiaPoint inertia = temporalPoints[i];
            inertia.point = trans1;
            temporalPoints[i] = inertia;
        }
        middlePoints = middles;
    }
    float AssingPoints(List<InertiaPoint> temporalPoints, Segment segment, bool lastOne)
    {
        float realLength = 0;
        Vector3 startingPoint = segment.startingEdge.point;
        Vector3 endingPoint = segment.endingEdge.point;
        if(temporalPoints.Count > 0)
        {
            if(!lastOne)
                realLength += (temporalPoints[0].point-startingPoint).magnitude;
            realLength += (temporalPoints[temporalPoints.Count-1].point-endingPoint).magnitude;
            for(int i = 0; i< temporalPoints.Count;i++)
            {
                if(i > 0)
                        realLength += (temporalPoints[i].point-temporalPoints[i-1].point).magnitude;
                segment.inBetween[i] = temporalPoints[i];                
            }
        }
        else
            realLength += (startingPoint-endingPoint).magnitude;
        segment.realLength = realLength;
        return realLength;
    }
    void ModifyAmountOfPoints(Segment segment, int pointAmount, Vector3 fakeStartingPoint)
    {
        pointAmount = Mathf.Clamp(pointAmount, 0, maxPointsInSegment);
        Vector3 startingPoint = segment.startingEdge.point;
        Vector3 endingPoint = segment.endingEdge.point;
        if(segment.inBetween.Count < pointAmount)
        {         
            InertiaPoint tempIn = new InertiaPoint();
            if(segment.inBetween.Count>0)
            {
                if((endingPoint-segment.inBetween[segment.inBetween.Count-1].point).magnitude > distanceBetweenPoints-0.1f)     
                {         
                    tempIn.point = startingPoint;
                    tempIn.previousPoint = tempIn.point;
                    segment.inBetween.Insert(0, tempIn);
                }
            }
            else    
            {            
                tempIn.point = endingPoint+(fakeStartingPoint-endingPoint).normalized*distanceBetweenPoints/2;
                tempIn.previousPoint = tempIn.point;
                segment.inBetween.Add(tempIn);
            }
        }
        else 
        if(segment.inBetween.Count >= pointAmount)      
        {   
            segment.inBetween.RemoveRange(0, segment.inBetween.Count-pointAmount);   
        }
    }
    void ApplyVelocities(Segment segment, List<InertiaPoint> pointsTemp)
    {
        Vector3 startingPoint;
        Vector3 currentPoint;
        Vector3 lastPoint;
        Vector3 directionPlane;
        Plane plane;
        Vector3 gravityVector = Vector3.down*ropeGravity*Time.fixedDeltaTime;
        InertiaPoint currentInertia;
        RaycastHit hit;
        Collider[] onPointCollisions;

        for(int i = 1; i< pointsTemp.Count-1; i++)
        {
            if(pointsTemp[i].fixedPoint)
                continue;
            
            currentInertia = pointsTemp[i];
            startingPoint = segment.startingEdge.point;
            currentPoint = currentInertia.point;
            
            lastPoint = currentInertia.previousPoint;
                        Debug.DrawLine(currentPoint, lastPoint, Color.red);

            currentInertia.previousPoint = currentPoint;
            directionPlane = segment.startingEdge.point-segment.endingEdge.point;
            plane = new Plane(directionPlane, currentPoint);
            Vector3 velocityVector = (currentPoint-lastPoint)*ropeAirFriction;
            Debug.DrawRay(currentPoint, velocityVector);
            Vector3 pointInPlaneClose = plane.ClosestPointOnPlane(segment.startingEdge.point)+velocityVector*(1-ropeTension);
            Vector3 tensionPoint = Vector3.Lerp(currentPoint+gravityVector+velocityVector, pointInPlaneClose, ropeTension);
            directionPlane = tensionPoint-currentPoint;
            //Problema amb la tensio, estic modificant el punt ja modificat, cosa que dona a l'extranya velocitat de tensio
            //que apareix ara mateix            
            onPointCollisions = Physics.OverlapSphere(tensionPoint, ropeRadious, ropeLayers);            
            if(Physics.SphereCast(currentPoint, ropeRadious-ropeRadious*1.2f, directionPlane, out hit, directionPlane.magnitude+ropeRadious,ropeLayers))            
                currentPoint = hit.point-directionPlane.normalized*ropeRadious;            
            else if(onPointCollisions.Length <= 0)            
                currentPoint = tensionPoint;   

            currentInertia.point = currentPoint;
            pointsTemp[i] = currentInertia;
        }
    }
    
    void StartToEnd(List<InertiaPoint> positionsTemp, float extra)
    {
        int max = positionsTemp.Count;
        for(int i = 1; i<max-1; i++)
        {
            if(positionsTemp[i].fixedPoint)
                continue;
            Vector3 trans1 = positionsTemp[i].point;
            Vector3 trans2 = positionsTemp[i-1].point;
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

            InertiaPoint current = positionsTemp[i];
            current.point = trans1;
            positionsTemp[i] = current;         
        }
    }
    void EndToStart(List<InertiaPoint> positionsTemp, float extra, out List<MiddlePoint> middlePoints)
    {        
        positionsTemp.Reverse();
        List<MiddlePoint> middles = new List<MiddlePoint>();
        for(int i = 1; i<positionsTemp.Count; i++)
        {
            if(positionsTemp[i].fixedPoint)
                continue;
            Vector3 trans1 = positionsTemp[i].point;
            Vector3 trans2 = positionsTemp[i-1].point;                 
            float tempDistance = distanceBetweenPoints;
            if(i == positionsTemp.Count-1 && extra > 0)
                tempDistance *= extra;
            
            trans1 += MiddleModifier(ref middles, positionsTemp.Count-1-i, trans1, trans2);
            
            Vector3 directionForward = (trans2-trans1).normalized;
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
            InertiaPoint current = positionsTemp[i];
            current.point = trans1;
            positionsTemp[i] = current;        
        }
        positionsTemp.Reverse();
        middles.Reverse();
        middlePoints = middles;
    }
    Vector3 MiddleModifier(ref List<MiddlePoint> middles, int insert, Vector3 trans1, Vector3 trans2)
    {
        Edge middlePoint = CollisionDetectionBetweenPoints(/*ref*/ trans1, trans2);
        float distanceBetween;
        float minDistance = distanceBetweenPoints;
        if(middlePoint != null)
        {
            middles.Add(new MiddlePoint(middlePoint, insert));
            distanceBetween = Vector3.Distance(trans1,middlePoint.point);
            minDistance -= Vector3.Distance(middlePoint.point,trans2);      
            if(canStrech)
            {
                if(distanceBetween>minDistance)
                    return (distanceBetween-minDistance)*(middlePoint.point-trans1).normalized;  
            }
            else
                return (distanceBetween-minDistance)*(middlePoint.point-trans1).normalized;
        }
        return Vector3.zero;
    }
    Edge CollisionDetectionBetweenPoints(Vector3 forwardPoint, Vector3 backPoint)
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
        return null;
        /* No necessari pel nou metode
        if(Physics.Raycast(backPoint, directionVector, out hit1, directionVector.magnitude, ropeLayers))
        {
            Plane plane = new Plane(hit1.normal, hit1.point+hit1.normal*ropeRadious);
            forwardPoint = plane.ClosestPointOnPlane(forwardPoint);
            return null;
        }
        else
            return null;*/
    }
}
