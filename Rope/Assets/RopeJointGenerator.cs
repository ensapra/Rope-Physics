using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeJointGenerator : MonoBehaviour
{
 [Header("End Points configuration")]
    public List<GameObject> currentPoints = new List<GameObject>();
    public GameObject nodePrefab;
    public Transform startingPoint; 
    public Vector3 startOffset;
    public Transform endingPoint;
    public Vector3 endingOffset;

    [Header("Rope simulation settings")]
    public int maximumIterations = 30;
    public int maxAmountOfPoints = 100;
    public float ropeGravity = 4;
    [Range(0,1)] public float ropeFlexibility = 0.97f;
    [Range(0,1)] public float groundFriction = 0.3f;
    [Range(0,1)] public float maxTension;
    public float maxAcomulatedTension;
    public Vector2 distanceMinMax = new Vector2(0,1);

    [Header("Rope Configuration")]
    public float ropeLength = 5;
    [Range(0.05f, 1f)] public float ropeRadious = 0.2f;
    public bool canBreak;
    public LayerMask collisionLayer;

    [Header("Rope Information")]
    public float currentLenght;
    public float currentTension;

    [Header("Debug Options")]
    public int VisualizeIteration = -1;
    public bool overallDebug;

    [Header("Components")]
    public RopeSim attachRope;
    private bool justBroke;
    private float delay;

    // Start is called before the first frame update
    void Start()
    {
        GameObject startingPointGO = GenerateNode(startingPoint, null, startingPoint != null);
        currentPoints.Add(startingPointGO);
        GameObject endingPointGO = GenerateNode(endingPoint, endingPoint != null);
    }
    public GameObject GenerateNode(Transform reference, GameObject childReference, bool itsStatic)
    {
        Vector3 nodePosition = reference != null ? reference.position : this.transform.position; 
        Quaternion rotation = reference != null ? reference.rotation : childReference != null ? childReference.transform.rotation : Quaternion.identity;
        Transform parent = reference != null ? reference : this.transform;
        GameObject newNode = Instantiate(nodePrefab, nodePosition, rotation, parent);
        newNode.name = itsStatic ? "StaticNode" : "DynamicNode";
        if(itsStatic)
        {
            Collider collider = newNode.GetComponent<Collider>();
            collider.isTrigger = true;
            Rigidbody thisRB = newNode.GetComponent<Rigidbody>();
            thisRB.constraints = RigidbodyConstraints.FreezeAll;
        }
        
        if(childReference != null)
        {
            CharacterJoint joint = childReference.GetComponent<CharacterJoint>();
            Rigidbody rb = newNode.GetComponent<Rigidbody>();
            joint.connectedBody = rb;
        }
        return newNode;
    }
    public GameObject GenerateNode(Transform reference, bool itsStatic)
    {
        GameObject newNode = GenerateNode(reference, currentPoints[0], itsStatic);
        currentPoints.Insert(0, newNode);
        return newNode;
    }
    // Update is called once per frame
    void FixedUpdate()
    {   
        //UpdateStartAndEnd();
        CreateNewSegments();
        if(canBreak)
            CheckBreakPoints();
        CheckAttachment();
        Visualize();
    }
    public void CheckAttachment()
    {
        if(attachRope != null)
        {
            this.ropeLength += attachRope.ropeLength;
            this.endingPoint = attachRope.endingPoint;
            this.endingOffset = attachRope.endingOffset;
            Destroy(attachRope.gameObject);
            attachRope = null;
        }
    }
    public void CheckBreakPoints()
    {
/*         if(justBroke)
        {
            delay += Time.deltaTime;
            if(delay < 2f)
                return;
            else
            {
                delay = 0;
                justBroke = false;
            }
        }
        int breakingPoints = -1;
        int brokenPoints = 0;
        for(int i = 0; i < currentSegments.Count; i++)
        {
            SegmentSim segmentSim = currentSegments[i];
            if(segmentSim.breakRope(maxTension, maxAcomulatedTension))
            {
                breakingPoints += i;
                brokenPoints++;
            }
        }
        if(breakingPoints != -1)
            BreakRope((int)breakingPoints/brokenPoints); */
    }
    private void BreakRope(int placeOfBreak)
    {
/*         this.justBroke = true;
        GameObject newRope = Instantiate(this.gameObject);
        RopeSim newRopeSim = newRope.GetComponent<RopeSim>();
        newRopeSim.currentSegments = new List<SegmentSim>();
        newRopeSim.startingPoint = null;
        newRopeSim.ropeLength = ropeLength-placeOfBreak*distanceMinMax.y;
        this.endingPoint = null;
        this.ropeLength = placeOfBreak*distanceMinMax.y; */

    }
    private void CreateNewSegments()
    {
        if(distanceMinMax.y < 0.02f)
            distanceMinMax.y = 0.02f;
        currentLenght = GetRopeLength();
        currentTension = currentLenght/ropeLength-1;
        int amountOfPoints = (int)(ropeLength/distanceMinMax.y)+1;
        int pointsDifference = amountOfPoints-currentPoints.Count;
        if(pointsDifference < 0)
            RemovePoints(pointsDifference);
        if(pointsDifference > 0)
            AddPoints(pointsDifference); 
    }
    private float GetRopeLength()
    {
        float Lenght = 0;
        for(int i = 1; i < currentPoints.Count; i++)
        {
            Lenght += Vector3.Distance(currentPoints[i-1].transform.position, currentPoints[i].transform.position);
        }
        return Lenght; 
    }
    private void RemovePoints(int amount)
    {
/*         amount = amount*-1;
        PointSim originalStarting = currentSegments[0].startingPoint;
        for(int i = 0; i < amount; i++)
        {
            currentSegments.RemoveAt(0);
        }
        currentSegments[0].startingPoint.RepurposeObject(originalStarting); */
    }
    private void AddPoints(int amount)
    {
        //Modify correctly initial segment
        if(amount > maxAmountOfPoints)
            amount = maxAmountOfPoints;
        for(int i = 0; i < amount-1; i++)
        {
            GenerateNode(null, false);
        }
    }
    private void UpdateStartAndEnd()
    {
        //currentPoints[0].transform.position = (startingPoint, startOffset);
        //currentPoints[currentPoints.Count-1].endingPoint.UpdatePosition(endingPoint, endingOffset);
    }
    private void Visualize()
    {
/*         List<Vector3> points = new List<Vector3>();
        lineRenderer.widthMultiplier = ropeRadious;
        for(int i = 0; i< currentSegments.Count; i++)
        {
            SegmentSim segment = currentSegments[i];
            points.AddRange(segment.getPoints());
            if(i == currentSegments.Count-1)
                points.Add(segment.endingPoint.getPosition());
        }
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray()); */
    }
}
