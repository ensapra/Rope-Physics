using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TheRopeGenerator : MonoBehaviour
{
    [Header("Performance Settings")]
    public bool RUN = true;
    [Range(1,200)] public int maxPointsLimit = 100;
    public Vector2 minMaxResolution = new Vector2(1,100);
    int previousRemovedPoints;

    [Header("The Points")]
    public List<DynamicPoint> dynamicPoints;
    
    [Header("RopeConfiguration")]
    public Vector2 Lenght;
    //public float Lenght;
    //[SerializeField] private float realLenght;
    public float ropeRadious;
    [Range(0,1)] public float groundFriction;
    public LayerMask ropeLayers;
    public float distanceBetweenPoints;
    public float gravity;
    [Tooltip("Should the rope be treated as sticks, or bend in itself, therefore, flop")]
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
        AddConstraints();
    }

    void FixedUpdate()
    {
<<<<<<< Updated upstream
=======
        if(cornerDictionary.ContainsKey(targetCornerInt))
            currentTargetCorners = cornerDictionary[targetCornerInt].currentCorners;
        else
            currentTargetCorners = null;
>>>>>>> Stashed changes
        if(!RUN)
            return;
        GenerateInBetweenPoints();
        AddPhysics();
        AddConstraints();
        CollisionSolver(dynamicPoints);
        VisualizePoints();
    }
    void GenerateInBetweenPoints()
    {
        int amountOfPoints = ((int)(Lenght.x/distanceBetweenPoints)/2)*2;
        float extra = Lenght.x-distanceBetweenPoints*amountOfPoints;
        amountOfPoints += 1;
        ModifiedPercent(extra);       
        amountOfPoints = Mathf.Clamp(amountOfPoints, 0 , maxPointsLimit);
        if(dynamicPoints.Count-2 < amountOfPoints)
            DoPoints(amountOfPoints);
        else
            RemovePoints(amountOfPoints);        
    }
    void ModifiedPercent(float extraDistance)
    {
        extraDistance = Mathf.Clamp(extraDistance, 0,2);
        dynamicPoints[0].percent = (2-extraDistance/2);
        dynamicPoints[dynamicPoints.Count-1].percent = (extraDistance/2);
    }
    void RemovePoints(int amountOfPoints)
    {
        int quantity = dynamicPoints.Count-2-amountOfPoints;
        quantity = quantity/2;
        dynamicPoints.RemoveRange(1, quantity);
        dynamicPoints.RemoveRange(dynamicPoints.Count-2-quantity, quantity);
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
    void AddConstraints()
    {
        List<DynamicPoint> temporalCopy = new List<DynamicPoint>(dynamicPoints);
        int resolution = (int)(minMaxResolution.y - (Mathf.Lerp(minMaxResolution.x, minMaxResolution.y, Lenght.x/100))+minMaxResolution.x);
        for(int x = 0; x < resolution; x++)
<<<<<<< Updated upstream
        {
            DoConstraint(temporalCopy);
        }        
        dynamicPoints = temporalCopy;
    }
    void DoConstraint(List<DynamicPoint> temporalCopy)
    {
        int lastPoint = temporalCopy.Count;
        for(int i = 1; i < lastPoint; i++)
        {
            DynamicPoint currentI1 = temporalCopy[i];
            DynamicPoint currentI2 = temporalCopy[i-1];
            Vector3 trans1 = currentI1.currentPoint;
            Vector3 trans2 = currentI2.currentPoint; 
            
            Vector3 directionForward = (trans2-trans1).normalized;
            float distance = Vector3.Distance(trans2,trans1);
            
            Vector3 directionAddition;
            float tempDistance = distanceBetweenPoints*(currentI1.percent+(1-currentI2.percent));
            directionAddition = (distance-tempDistance)*(directionForward).normalized/2;

            if(!canFlop || (canFlop && distance > tempDistance))
            {
                trans1 += directionAddition;  
                trans2 -= directionAddition; 
            }   

            currentI1.currentPoint = trans1;
            currentI2.currentPoint = trans2;
        }
    }
=======
        {        
            for(int i = 1; i< lastPoint; i++)
            {
                DynamicPoint currentI1 = temporalCopy[i];
                DynamicPoint currentI2 = temporalCopy[i-1];
                if(cornerDictionary.ContainsKey(i))
                    DoCornerConstraints(currentI1, currentI2, i, x == resolution-1);
                else          
                    DoConstraint(currentI1, currentI2);                
            }
        }    
        MoveCorners();  
        dynamicPoints = temporalCopy;
    }
    void MoveCorners()
    {
        Dictionary<int, CornerPack> temp = new Dictionary<int, CornerPack>(cornerDictionary);
        for(int i = 0; i < dynamicPoints.Count; i++)
        {
            if(cornerDictionary.ContainsKey(i))
            {
                List<DynamicPoint> corners = cornerDictionary[i].currentCorners;
                DynamicPoint trans1 = dynamicPoints[i];
                DynamicPoint trans2 = dynamicPoints[i-1]; 
                DynamicPoint trans2C = corners[corners.Count-1];
                DynamicPoint trans1C = corners[0];
                Vector3 dir = trans1.currentPoint-trans2C.currentPoint;
                float distance = cornerDictionary[i].GetLength(trans1.currentPoint, trans2.currentPoint);
                Debug.DrawRay(trans1.currentPoint, -dir, Color.magenta);
                Debug.DrawLine(trans2C.currentPoint, trans1.currentPoint, Color.green);
                float distanceI1 = (trans1.currentPoint-trans2C.currentPoint).magnitude/distance;
                float distanceI2 = (trans2.currentPoint-trans1C.currentPoint).magnitude/distance;
                if(distanceI1 < 0.05f)
                    AddDynamicPoint(i, trans1C, 1, temp);
                if(distanceI2 < 0.05f)
                    AddDynamicPoint(i, trans2C, -1, temp);
            }
            else
                continue;
        }
        cornerDictionary = temp;
    }
    void DoCornerConstraints(DynamicPoint currentI1, DynamicPoint currentI2, int i, bool last)
    {
        List<DynamicPoint> corners = cornerDictionary[i].currentCorners;
        Vector3 trans1 = currentI1.currentPoint;
        Vector3 trans2 = currentI2.currentPoint; 
        DynamicPoint trans2C;
        DynamicPoint trans1C;
        float distance = cornerDictionary[i].GetLength(trans1, trans2);
        for(int x = 0; x <= corners.Count; x++)
        {
            if(x == 0)
                trans2C = currentI2;
            else
                trans2C = corners[x-1];

            if(x == corners.Count)
                trans1C = currentI1;
            else
                trans1C = corners[x];
            DoConstraint(trans1C, trans2C, distance, currentI1.percent, currentI2.percent);
        }
    }
    void DoConstraint(DynamicPoint currentI1,DynamicPoint currentI2, float distance, float percent1, float percent2)
    {
        Vector3 dfI1 = (currentI2.currentPoint-currentI1.currentPoint);
        float tempDistance = (distanceBetweenPoints/distance)*dfI1.magnitude*(percent1+(1-percent2));
        Vector3 directionAdditionI1 = (dfI1.magnitude-tempDistance)*(dfI1).normalized;
        if(!canFlop || (canFlop && dfI1.magnitude > tempDistance))
        {
            currentI1.currentPoint += directionAdditionI1;
            currentI2.currentPoint -= directionAdditionI1; 
        }
    }
    void AddDynamicPoint(int i, DynamicPoint target, int dir, Dictionary<int, CornerPack> final)
    {
        final[i].currentCorners.Remove(target);
        if(final[i].currentCorners.Count <= 0)
            final.Remove(i);
        if(i+dir < 0 || i+dir >= dynamicPoints.Count)
            return;
        if(final.ContainsKey(i+dir))
        {
            if(dir < 0)
                final[i+dir].currentCorners.Add(target);
            else
                final[i+dir].currentCorners.Insert(0, target);
        }
        else
        {
            if(i+dir > dynamicPoints.Count-1 || i+dir <= 0)
                return;
            CornerPack temp = new CornerPack(target);
            final.Add(i+dir, temp);
        }
    }
    void DoConstraint(DynamicPoint currentI1, DynamicPoint currentI2)
    {
        Vector3 trans1 = currentI1.currentPoint;
        Vector3 trans2 = currentI2.currentPoint; 
        Vector3 directionForward = (trans2-trans1).normalized;
        float distance = Vector3.Distance(trans2,trans1);
        Vector3 directionAddition;
        float tempDistance = distanceBetweenPoints*(currentI1.percent+(1-currentI2.percent));        
        directionAddition = (distance-tempDistance)*(directionForward).normalized/2;
        if(!canFlop || (canFlop && distance > tempDistance))
        {
            trans1 += directionAddition;  
            trans2 -= directionAddition; 
        }   
        currentI1.currentPoint = trans1;
        currentI2.currentPoint = trans2;
    }
    #endregion
    #region Collision Solver
>>>>>>> Stashed changes
    void CollisionSolver(List<DynamicPoint> pointsToEdit)
    {
        for(int i = 0; i < pointsToEdit.Count; i++)
        {
            ChangeFromRigidbodies(pointsToEdit[i]);
            ChangePositionCollision(pointsToEdit[i]);
<<<<<<< Updated upstream
            //AddForcesToRigidbodies(points[i], additive);
=======
            if(i > 0)            
                DoCornerDetection(i, pointsToEdit[i], pointsToEdit[i-1]);    
        }
    }

    void DoCornerDetection(int i, DynamicPoint currentI1, DynamicPoint currentI2)
    {
        if(cornerDictionary.ContainsKey(i))
            MultipleCornerDetection(cornerDictionary[i].currentCorners, i, currentI1, currentI2);
        else
        {
            DynamicPoint cornerPoint = GenerateDynamic(currentI1.currentPoint,currentI2.currentPoint);
            if(cornerPoint != null)
            {
                List<DynamicPoint> cornerPoints = new List<DynamicPoint>();
                cornerPoints.Add(cornerPoint);
                CornerPack pack = new CornerPack(cornerPoints);
                cornerDictionary.Add(i, pack);
            }
        }
    }
    void MultipleCornerDetection(List<DynamicPoint> corners, int i, DynamicPoint currentI1, DynamicPoint currentI2)
    {
        corners = ValidateCurrentCorners(corners, currentI1, currentI2);
        corners = GenerateNewCorners(corners, currentI1, currentI2);
        if(corners.Count > 0)
            cornerDictionary[i].currentCorners = corners;
        else
            cornerDictionary.Remove(i);
    }
    List<DynamicPoint> ValidateCurrentCorners(List<DynamicPoint> corners, DynamicPoint currentI1, DynamicPoint currentI2)
    {
        //NEEDS TO BE EDITED
        List<DynamicPoint> temporalCorners = new List<DynamicPoint>();
        for(int i = 0; i < corners.Count; i++)
        {
            DynamicPoint cornerPoint = null;
            DynamicPoint firstDynamic;
            DynamicPoint secondDynamic;
            if(i == 0)
                secondDynamic = currentI2;
            else
                secondDynamic = corners[i-1];
            
            if(i+1 >= corners.Count)
                firstDynamic = currentI1;
            else                
                firstDynamic = corners[i+1];
            if((corners[i].currentPoint-secondDynamic.currentPoint).magnitude < ropeRadious)
                continue;
            if(corners[i].cornerNormal1 == Vector3.zero|| corners[i].cornerNormal2 == Vector3.zero)
                continue;
            if(corners[i].cornerNormal1 == corners[i].cornerNormal2)
                continue;
            cornerPoint = corners[i];//GenerateDynamic(firstDynamic.currentPoint, secondDynamic.currentPoint);
            if(cornerPoint != null)                
            {
                corners[i].cornerNormal1 = cornerPoint.cornerNormal1;
                corners[i].cornerNormal2 = cornerPoint.cornerNormal2;
                temporalCorners.Add(corners[i]); 
            }
        }
        return temporalCorners;
    }
    List<DynamicPoint> GenerateNewCorners(List<DynamicPoint> corners, DynamicPoint currentI1, DynamicPoint currentI2)
    {
        List<DynamicPoint> temporalCorners = new List<DynamicPoint>();
        for(int i = 0; i <= corners.Count; i++)
        {
            DynamicPoint cornerPoint = null;
            DynamicPoint firstDynamic;
            DynamicPoint secondDynamic;
            if(i == 0)
                secondDynamic = currentI2;
            else
                secondDynamic = corners[i-1];
            
            if(i == corners.Count)
                firstDynamic = currentI1;
            else                
                firstDynamic = corners[i];

            cornerPoint = GenerateDynamic(firstDynamic.currentPoint, secondDynamic.currentPoint);
            //Aixo esta malament, no en bon ordre
            if(cornerPoint != null)            
                temporalCorners.Add(cornerPoint);            
            if(i < corners.Count)
                temporalCorners.Add(firstDynamic);
>>>>>>> Stashed changes
        }
    }
    void AddForcesToRigidbodies(DynamicPoint point, RaycastHit hit)
    {
<<<<<<< Updated upstream
        if(hit.collider)
=======
        RaycastHit hit1;
        RaycastHit hit2;
        Vector3 direction = I2-I1;
        if(DrawRopeLines)
            Debug.DrawLine(I2, I1, Color.yellow);
        float magnitude = direction.magnitude;
        Ray I1Ray = new Ray(I1, direction);
        Ray I2Ray = new Ray(I2, -direction);
        if(Physics.SphereCast(I1Ray, ropeRadious*0.9f, out hit1, magnitude, ropeLayers))
>>>>>>> Stashed changes
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
        Lenght.y = 0;
        List<Vector3> currentPoints = new List<Vector3>();
        lineRenderer.widthMultiplier = ropeRadious*2;
        for(int x = 0; x < temporalPoints.Count; x++)
        {
            currentPoints.Add(temporalPoints[x].currentPoint);
            if(x > 0)
                Lenght.y += Vector3.Distance(temporalPoints[x].currentPoint,temporalPoints[x-1].currentPoint);
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
<<<<<<< Updated upstream
[System.Serializable]
public class DynamicPoint
{
    private Vector3 current;
    private Vector3 last;
    [SerializeField] private bool _fixed;
=======
public class CornerPack
{
    public List<DynamicPoint> currentCorners;
    public float GetLength(Vector3 I1, Vector3 I2)
    {
        float tempLength = 0;
        tempLength += (I1-currentCorners[0].currentPoint).magnitude;
        tempLength += (I2-currentCorners[currentCorners.Count-1].currentPoint).magnitude;
        for(int i = 1; i < currentCorners.Count; i++)
        {
            tempLength += (currentCorners[i].currentPoint-currentCorners[i-1].currentPoint).magnitude;
        }
        return tempLength;
    }
    public CornerPack(List<DynamicPoint> currentCorners)
    {
        this.currentCorners = currentCorners;
    }
    public CornerPack(DynamicPoint currentCorner)
    {
        List<DynamicPoint> temp = new List<DynamicPoint>();
        temp.Add(currentCorner);
        this.currentCorners = temp;
    }
}
public enum DynamicType{Normal, Fixed, Corner}
[System.Serializable]
public class DynamicPoint
{
    public Vector3 current;
    public Vector3 last;
    [SerializeField] private DynamicType _type = DynamicType.Normal;
>>>>>>> Stashed changes
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