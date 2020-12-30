using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecuredCopy : MonoBehaviour
{
    public bool ropeMode;
    public bool thrown;
    public GameObject thrownObjectPr;
    public GameObject thrownO;
    Vector3 direction;
    Camera cam;
    private List<GameObject> objectsTrail = new List<GameObject>();
    //private List<Edge> points = new List<Edge>();
    LineRenderer renderer;
    public int amount;
    public float timeGone;
    public float speed;

    public List<Transform> points = new List<Transform>();
    public List<Vector3> positions = new List<Vector3>();
    public float distanceBetween;
    public float maxDistance;
    public Transform fakePoint;
    public GameObject ropesHolder;
    public float ropeGravity;
    public LayerMask ropeLayers;
    public float pointLerping;
    public float ropeRadious;
    public bool canStrech;
    void Start()
    {
        cam = Camera.main;
        renderer = GetComponent<LineRenderer>();
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
            ropeMode = !ropeMode;
        if(Input.GetKeyDown(KeyCode.Mouse0))
            Throw();
    }
    void FixedUpdate()
    {
        direction = cam.transform.forward+Vector3.up;
        if(thrownO == null && ropeMode && !thrown)
        {
            thrownO = Instantiate(thrownObjectPr, transform.position+Vector3.up*1+transform.forward,transform.rotation, transform);
            Rigidbody rb = thrownO.GetComponent<Rigidbody>();
            rb.isKinematic = true;
        }
        CalculatePoints();
        

        if(!ropeMode)
        {
            Destroy(thrownO);
            points.Clear();
            thrownO = null;
            thrown = false;
        }

        if(thrown)
            GenerateRope();
    }
    void GenerateRope()
    {
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
        points = currentPoints;*/
        //float currentDistance = 0;

        RopeMove MOVE = thrownO.GetComponent<RopeMove>();
        positions.Clear();
        positions.Add(transform.position+Vector3.up);
        for(int i = 0; i< points.Count; i++)
        {
            positions.Add(points[i].position);
        }
        if(MOVE.ObjectClinged)        
            GrabedRope(positions);        
        else
            AirRope(positions);             
        positions.RemoveAt(0);

        ApplyCollisions(positions, points);
        
        for(int i= 0; i< positions.Count-1; i++)
        {
            points[i].position = Vector3.Lerp(points[i].position,positions[i], Time.deltaTime*pointLerping);
        }    

        if(Vector3.Distance(transform.position, thrownO.transform.position) > maxDistance)
        {
            Rigidbody rb = thrownO.GetComponent<Rigidbody>();
            Vector3 gotIt = (points[points.Count-1].position-transform.position);
            Vector3 current = Vector3.Project(rb.velocity, gotIt);
            rb.velocity -= current;
        }

    }
    void AirRope(List<Vector3> enterPositions)
    {
        float currentLength = GetLength(enterPositions);
        ApplyGravity(enterPositions);
        if(currentLength < maxDistance)
        {
            float initialDis = Vector3.Distance(enterPositions[0], enterPositions[1]);
            if(initialDis > distanceBetween)
            {
                Transform tempTr = Instantiate(fakePoint, enterPositions[0], Quaternion.identity, ropesHolder.transform);
                points.Insert(0,tempTr);
                enterPositions.Insert(1, tempTr.position);
            }  
            LastToFirst(enterPositions.Count-1,enterPositions);
            //FirstToLast(enterPositions.Count-1, enterPositions);
            //MovePoints(-1, -1,-1, enterPositions);
            //MovePoints(enterPositions.Count-1, 1, 1,enterPositions);        
        }
        else
        {
            LastToFirst(0, enterPositions);
            FirstToLast(0, enterPositions);
            //MovePoints(-1, -1,-1, enterPositions);
            //MovePoints(-1, 1, -1,enterPositions);
        }
    }

    void ApplyGravity(List<Vector3> pointsTemp)
    {
        for(int i = 0; i< pointsTemp.Count-1; i++)
        {
            Vector3 trans1 = pointsTemp[i];
            Vector3 gravity = Vector3.down*ropeGravity*Time.deltaTime;
            trans1 += gravity;
            pointsTemp[i] = trans1;
        }
    }
    void ApplyCollisions(List<Vector3> tempPoints, List<Transform> oldPositions)
    {
        for(int i = 0; i< tempPoints.Count; i++)
        {
            Vector3 directionVec = tempPoints[i]-oldPositions[i].position;
            Collider[] collisions = Physics.OverlapSphere(tempPoints[i], 0.4f, ropeLayers);
            RaycastHit hit;
            /*if(collisions.Length > 0)
            {
                if(Physics.Raycast(oldPositions[i].position, directionVec, out hit, directionVec.magnitude+0.1f, ropeLayers))
                {
                    Plane temPlane = new Plane(hit.normal, hit.point+hit.normal*0.2f);
                    tempPoints[i] = temPlane.ClosestPointOnPlane(tempPoints[i]);
                }
            }*/
            /*else          
            if(Physics.Raycast(tempPoints[i], Vector3.down, out hit, ropeRadious+0.02f, ropeLayers))
            {
                tempPoints[i] = hit.point+Vector3.up*ropeRadious;
            }*/
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
                positionsTemp[i] = trans1;  
            }
        }
    }
    void LastToFirst(int fixedPoint, List<Vector3> positionsTemp)
    {
        int tempFix = (positionsTemp.Count-1)-fixedPoint;
        positionsTemp.Reverse();
        for(int i = 1; i<positionsTemp.Count; i++)
        {
            if(i == tempFix)
                continue;
            else
            {
                Vector3 trans1 = positionsTemp[i];
                Vector3 trans2 = positionsTemp[i-1];
                Vector3 middlePoint = CollisionDetectionBetweenPoints(trans1, trans2);
                float distanceBetweenPoints = Vector3.Distance(trans2,trans1);
                float minDistance = distanceBetween;
                Vector3 directionForward = trans2-trans1;
                if(middlePoint != Vector3.zero)
                {
                    distanceBetweenPoints = Vector3.Distance(trans1,middlePoint);
                    directionForward = middlePoint-trans1;
                    minDistance -= Vector3.Distance(trans2,middlePoint);
                }
                
                if(canStrech)
                {
                    if(distanceBetweenPoints>minDistance)
                        trans1 += (distanceBetweenPoints-minDistance)*(directionForward).normalized;  
                }
                else
                    trans1 += (distanceBetweenPoints-minDistance)*(directionForward).normalized;  

                positionsTemp[i] = trans1;  
            }
        }
        positionsTemp.Reverse();
    }
    Vector3 CollisionDetectionBetweenPoints(Vector3 forwardPoint, Vector3 backPoint)
    {
        RaycastHit hit1;
        RaycastHit hit2;
        Vector3 middle = Vector3.zero;
        Vector3 directionVector = backPoint-forwardPoint;
        if(Physics.Raycast(backPoint, direction, out hit1, direction.magnitude, ropeLayers) && Physics.Raycast(forwardPoint, -direction, out hit2, direction.magnitude, ropeLayers))
        {
            //Corner detection
            Plane plane1 = new Plane(hit1.normal, hit1.point);
            Plane plane2 = new Plane(hit2.normal, hit2.point);
            middle = (hit1.point+hit2.point)/2;
            middle = plane1.ClosestPointOnPlane(middle);
            middle = plane2.ClosestPointOnPlane(middle);
        }
        /*else
        if(Physics.Raycast(backPoint, direction, out hit1, direction.magnitude, ropeLayers))
        {
            Plane plane = new Plane(hit1.normal, hit1.point+hit1.normal*ropeRadious);
            trans1 = plane.ClosestPointOnPlane(trans1);
        }*/
        return middle;
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
    /*
    List<Vector3> MovePoints(int FixedPoint, int directionFor, int directioMovement, List<Vector3> pointsTemp)
    {
        //FABRIK ALGORITHM
        if(directionFor == -1)
        {
            pointsTemp.Reverse();
            FixedPoint = (pointsTemp.Count-1)-FixedPoint;
        }
        int final;
        if(directioMovement == -1)
            final = pointsTemp.Count;
        else
            final = pointsTemp.Count-1;
        for(int i= 0; i< final;i++)
        {
            if(i == FixedPoint)
                continue;
            else
            {
                Vector3 trans1 = pointsTemp[i];
                Vector3 trans2;
                if(i+directioMovement < final)
                {
                    if(i+directioMovement < 0 && directionFor == 1)
                        trans2 = this.transform.position+Vector3.up;
                    else
                    {
                        if(i+directioMovement >= 0)
                            trans2 = pointsTemp[i+directioMovement];
                        else
                            continue;
                    }
                }
                else
                    continue;

                RaycastHit hit1;
                RaycastHit hit2;
                Vector3 middle = Vector3.zero;
                float distanceSecond = 0;
                if(Physics.Raycast(trans2, trans1-trans2, out hit1, (trans1-trans2).magnitude, ropeLayers) && Physics.Raycast(trans1, trans2-trans1, out hit2, (trans2-trans1).magnitude, ropeLayers))
                {
                    //Corner detection
                    Plane plane1 = new Plane(hit1.normal, hit1.point);
                    Plane plane2 = new Plane(hit2.normal, hit2.point);
                    middle = (hit1.point+hit2.point)/2;
                    middle = plane1.ClosestPointOnPlane(middle);
                    middle = plane2.ClosestPointOnPlane(middle);
                    distanceSecond = Vector3.Distance(trans2,middle);
                    Debug.DrawRay(middle, hit1.normal+hit2.normal);
                }
                else
                if(Physics.Raycast(trans2, trans1-trans2, out hit1, (trans1-trans2).magnitude, ropeLayers) && directionFor == -1)
                {
                    Plane plane = new Plane(hit1.normal, hit1.point+hit1.normal*ropeRadious);
                    trans1 = plane.ClosestPointOnPlane(trans1);
                }
                float distance = Vector3.Distance(trans1,trans2);
                Vector3 directionBack;
                float distanceMov;
                if(distanceSecond > 0)
                {
                    directionBack = middle-trans1;
                    distanceMov = Vector3.Distance(trans1, middle)-(distanceBetween-distanceSecond);
                }
                else
                {
                    directionBack = trans2-trans1;
                    distanceMov = distance-distanceBetween;
                }

                if(canStrech)
                {
                    if(distance+distanceSecond>distanceBetween)
                        trans1 += (distanceMov)*(directionBack).normalized;  
                }
                else
                    trans1 += (distanceMov)*(directionBack).normalized;  
                pointsTemp[i] = trans1;

            }
        }
        if(directionFor == -1)
        {
            pointsTemp.Reverse();
        }
        return pointsTemp;
        
    }*/
    void GrabedRope(List<Vector3> enterPositions)
    {
        ApplyGravity(enterPositions);
        FirstToLast(enterPositions.Count-1, enterPositions);
        LastToFirst(enterPositions.Count-1, enterPositions);
        /*
        MovePoints(enterPositions.Count-1,1,-1,enterPositions);
        MovePoints(-1,-1,-1,enterPositions);*/
    }
    
    
    void Throw()
    {
        if(thrownO && !thrown)
        {
            thrownO.transform.position = transform.position+Vector3.up*1+transform.forward;
            Rigidbody rb = thrownO.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.isKinematic = false;
            rb.velocity = direction*speed;
            thrownO.transform.SetParent(null);
            thrown = true;
            points.Clear();
            points.Add(thrownO.transform);
        }
    }
    void CalculatePoints()
    {
        int currentI= 0;
        if(ropeMode && !thrown)
        {
            Vector3 inital = thrownO.transform.position;
            Vector3 oldPos = inital;
            float timeHap = 0;
            for(int i=0; i< amount; i++)
            {
                GameObject current;
                Vector3 position = inital + direction*speed*timeHap+Physics.gravity*Mathf.Pow(timeHap,2)/2;
                RaycastHit hit;
                if(Physics.Raycast(oldPos, position-oldPos, out hit,(position-oldPos).magnitude))
                {
                    position = hit.point;
                }
                if(objectsTrail.Count< amount)
                {
                    current = Instantiate(thrownObjectPr, transform.position, Quaternion.identity);
                    current.GetComponent<Rigidbody>().isKinematic = true;
                    current.GetComponent<SphereCollider>().isTrigger = true;
                    objectsTrail.Add(current);
                }
                else
                {
                    current = objectsTrail[i];
                }
                current.transform.position = Vector3.Lerp(current.transform.position,position,Time.deltaTime*10);
                current.SetActive(true);
                currentI = i;
                if(hit.point != Vector3.zero)
                    break;
                oldPos = position;
                timeHap += timeGone/amount;
            }
        }
        if(objectsTrail.Count > currentI)
        {
            for(int i = currentI+1; i< objectsTrail.Count; i++)
            {
                objectsTrail[i].SetActive(false);
            }
        }
    }
}
