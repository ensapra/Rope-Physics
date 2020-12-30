using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowRope : MonoBehaviour
{
    public bool ropeMode;
    public bool thrown;
    public GameObject thrownHook;
    public GameObject thrownHookPB;
    public Transform holder;
    RopeGenerator2 ropeGenerator;
    Vector3 directionCamera;
    Camera cam;
    private List<GameObject> objectsTrail = new List<GameObject>();
    //private List<Edge> points = new List<Edge>();
    LineRenderer renderer;
    public int amount;
    public float timeGone;
    public float speed;
    public float targetLengthRope = 10;
    public bool grabbed;
    Rigidbody finalPointRB;
    RopeMove finalPointMove;
    Rigidbody rb;
    void Start()
    {
        cam = Camera.main;
        renderer = GetComponent<LineRenderer>();
        ropeGenerator = GetComponent<RopeGenerator2>();
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            ropeMode = !ropeMode;
            AimStart(ropeMode);
        }
        if(Input.GetKeyDown(KeyCode.Mouse0))
            Throw();
        float var = Input.GetAxis("Scroll");
        grabbed = Input.GetKey(KeyCode.V);
    }
    void FixedUpdate()
    {
        directionCamera = cam.transform.forward+Vector3.up;
        if(ropeMode)
            CalculatePoints(true);
        if(thrown)
        {
            Vector3 startingPoint = transform.position+Vector3.up+transform.right/2f;
            Vector3 endingPoint = thrownHook.transform.position;
            ropeGenerator.GenerateRope(startingPoint, endingPoint, finalPointMove.ObjectClinged, grabbed, 50);
            /*if((ropeGenerator.ropeTension-1) <= 1)
            {
                //rb.velocity = (rb.velocity.normalized-(ropeGenerator.segments[0].startingEdge.point-ropeGenerator.segments[0].endingEdge.point).normalized)*(rb.velocity.magnitude+ropeGenerator.currentLenghtOfRope-targetLengthRope);
            }*/
        }
    }
    void AimStart(bool start)
    {
        if(start)
        {
            thrownHook = Instantiate(thrownHookPB, transform.position+Vector3.up*1+transform.forward,transform.rotation, transform);
            Rigidbody rb = thrownHook.GetComponent<Rigidbody>();
            rb.isKinematic = true;
        }
        else
        {
            Destroy(thrownHook);
            //ropeGenerator.DestroyRope();
            thrownHook = null;
            thrown = false;
            CalculatePoints(false);
        }
    }
    void Throw()
    {
        if(thrownHook && !thrown)
        {
            thrownHook.transform.position = transform.position+Vector3.up*1+transform.forward;            
            finalPointRB = thrownHook.GetComponent<Rigidbody>();
            finalPointRB.velocity = Vector3.zero;
            finalPointRB.isKinematic = false;
            finalPointRB.velocity = directionCamera*speed;
            finalPointMove = thrownHook.GetComponent<RopeMove>();
            thrownHook.transform.SetParent(null);
            thrown = true;
        }
    }
    void CalculatePoints(bool active)
    {
        int currentI= 0;
        if(!thrown && active)
        {
            Vector3 inital = thrownHook.transform.position;
            Vector3 oldPos = inital;
            float timeHap = 0;
            for(int i=0; i< amount; i++)
            {
                GameObject current;
                Vector3 position = inital + directionCamera*speed*timeHap+Physics.gravity*Mathf.Pow(timeHap,2)/2;
                RaycastHit hit;
                if(Physics.Raycast(oldPos, position-oldPos, out hit,(position-oldPos).magnitude))
                {
                    position = hit.point;
                }
                if(objectsTrail.Count< amount)
                {
                    current = Instantiate(thrownHookPB, transform.position, Quaternion.identity, holder);
                    current.transform.localScale *= 0.5f;
                    current.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
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
