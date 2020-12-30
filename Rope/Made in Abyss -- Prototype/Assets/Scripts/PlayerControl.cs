using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float speed;
    public float jumpSpeed;
    public float sprintSpeed;
    private bool jump;
    private bool sprint;
    private Vector2 direction;
    private Vector2 directionRaw;
    private Camera cam;
    private Rigidbody rb;
    public bool ground;
    public float smooth;
    public int amountRaycast;
    public float radius;
    public float angle;
    public LayerMask groundMask;
    public Vector3 backWardNormal;
    void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        var yValue = Input.GetAxis("Horizontal");
        var xValue = Input.GetAxis("Vertical");
        direction = new Vector2(xValue, yValue);

        var yValueR = Input.GetAxisRaw("Horizontal");
        var xValueR = Input.GetAxisRaw("Vertical");
        directionRaw = new Vector2(xValueR, yValueR);

        sprint = Input.GetKey(KeyCode.LeftShift);
        if(Input.GetKeyDown(KeyCode.Space) && ground)
            Jump(direction);
    }
    void FixedUpdate()
    {
        ground = Physics.Raycast(transform.position+Vector3.up*0.1f, Vector3.down, 0.2f,groundMask);
        Vector3 velocity = rb.velocity;
        Vector3 forwardCam = Vector3.Cross(Vector3.up, -cam.transform.right);
        Vector3 dir = direction.x*forwardCam + direction.y*cam.transform.right;
        if(ground && !jump)
        {
            if(directionRaw.magnitude != 0)
            {
                if(sprint)
                    velocity = transform.forward*sprintSpeed;
                else
                    velocity = transform.forward*speed;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir, Vector3.up), Time.deltaTime*smooth);
            }
            else
                velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime*10);
        }
        if(!ground)
        {
            if(jump)
                if(velocity.y < 0)
                    jump = false;
            DetectHighGrabs();
        }
        rb.velocity = velocity;
    }
    void DetectHighGrabs()
    {
        Vector3 directionFor = (rb.velocity-rb.velocity.y*Vector3.up);
        Vector3 extraFor;
        Vector3 basic;
        if(direction.magnitude*Time.deltaTime > 0.05f)
        {
            basic = directionFor.normalized*0.5f;
            extraFor = directionFor*Time.deltaTime;
        }
        else
        {
            basic = transform.forward*0.5f;
            extraFor = transform.forward*0.05f;
        }
        Vector3 finalPoint = transform.position + basic + extraFor;
        RaycastHit hit;
        if(Physics.Raycast(finalPoint+Vector3.up*2, Vector3.down, out hit, 2, groundMask))
        {
            transform.position = hit.point+Vector3.up*0.05f;
        }
    }
    void Jump(Vector2 currentDirection)
    {
        jump = true;
        Vector3 direction = transform.forward*currentDirection.x +transform.right*currentDirection.y;
        rb.AddForce(direction*speed/2+Vector3.up*jumpSpeed*100);
    }
}
