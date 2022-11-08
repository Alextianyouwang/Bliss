using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    private CharacterController character;

    [SerializeField][Range(0,0.5f)] private float smoothTime;
    private Vector3 currentVelocity;
    private Vector3 currentDir;

    [SerializeField] private float gravityFactor = -13;
    private float YVelocity;

    private bool canMove = true;
    RaycastHit camPosRay;
    
    public Camera cam;
    public LayerMask ceiling;
    Vector3 targetPostion;
    Vector3 switchSpeed;

    private void OnEnable()
    {

    }
    private void OnDisable()
    {


    }
    void Start()
    {
        character = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (canMove) 
        {
            Move();
        }
        Gravity();
        if (Physics.SphereCast(transform.position, 1f, transform.up, out camPosRay, 1.5f,ceiling))
        {
            targetPostion = transform.InverseTransformPoint( camPosRay.point + 1 * camPosRay.normal);
        }
        else 
        {
            targetPostion = new Vector3(0, 1.5f, 0);
        }
        cam.transform.localPosition = Vector3.SmoothDamp(cam.transform.localPosition, targetPostion, ref switchSpeed, 0.05f);
    }

    void Move() 
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector3 targetDir = (transform.right * moveX + transform.forward * moveY).normalized ;
        currentDir = Vector3.SmoothDamp(currentDir, targetDir, ref currentVelocity, smoothTime);
        Vector3 velocity = currentDir * moveSpeed + YVelocity * Vector3.up;

        character.Move(velocity * Time.deltaTime);
    }

    void ToggleMove(bool toggle) 
    {
        canMove = toggle;
    }
    void Gravity() 
    {
        YVelocity += character.isGrounded ? 0.0f :  gravityFactor * Time.deltaTime;
    }
    
}
