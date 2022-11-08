using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldTransition : MonoBehaviour
{
    public GameObject bigEyeWorld;
    public GameObject ground;
    public Material worldMat,grassMat;
    public AnimationCurve floatAnimationCurve,transitionSpeedCurve,cameraDolleyCurve;
    public Camera cam;
    public LayerMask groundMask;
    public float bigEyeRoomYOffset,dolleyDepth;
    // private Rigidbody rb;

    private Material bigEyeWorldMat;

    private bool isFloating = false, isInTransition = false, isInBigEyeWorld = false;

    
    void Start()
    {
        /*if (!GetComponent<Rigidbody>())
        {
            Debug.LogWarning("Please Attach Rigidbody to player");
            return;
        }
        rb = GetComponent<Rigidbody>();*/
        bigEyeWorld.SetActive(false);
        bigEyeWorldMat = bigEyeWorld.GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !isFloating && !isInTransition) 
        {
            if (!isInBigEyeWorld)
            {
                isInBigEyeWorld = true;
                //ground.GetComponent<Collider>().enabled = false;
            }


            else
            {
                isInBigEyeWorld = false;
                // ground.GetComponent<Collider>().enabled = true;
            }
                
            InitiateWorldTransition();
        }
    }
    void InitiateWorldTransition() 
    {
        StartCoroutine(TransitionAnimation(1f));
        StartCoroutine(TransitionEffect(1f, isInBigEyeWorld));
        StartCoroutine(CameraEffect(1f));
    }

    IEnumerator CameraEffect(float speed) 
    {
        float percentage = 0;
        float initialFieldOfView = cam.fieldOfView;
        float targetFieldOfView = initialFieldOfView + dolleyDepth;
        while (percentage < 1)
        {
            percentage += Time.deltaTime * speed;
            float animationValue = cameraDolleyCurve.Evaluate(percentage);
            cam.fieldOfView = Mathf.Lerp(initialFieldOfView, targetFieldOfView, animationValue);
            yield return null;
        }
    }

    IEnumerator TransitionAnimation(float floatingSpeed) 
    {
        isFloating = true;
        float percentage = 0;
        //GetComponent<CharacterController>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        float startY = transform.position.y;
        //rb.isKinematic = true;
        while (percentage < 1) 
        {
            percentage += Time.deltaTime * floatingSpeed;
            float targetY = floatAnimationCurve.Evaluate(percentage);
            transform.position = new Vector3 (transform.position.x,startY+ targetY,transform.position.z);
            yield return null;
        }
        //GetComponent<CharacterController>().enabled = true;
         GetComponent<Rigidbody>().isKinematic = false;
        isFloating = false;
    }

    IEnumerator TransitionEffect(float dissolvingSpeed, bool expanding) 
    {
        isInTransition = true;
        float percentage = 0;
        float initialValue = expanding ? 0 : 30;
        float targetValue = expanding ? 30 : 0;
        if (expanding) 
        {
            bigEyeWorld.SetActive(true);
            /*
            Ray groundRay = new Ray(transform.position, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(groundRay, out hit, 100f, groundMask)) 
            {
                bigEyeWorld.transform.position = hit.point + new Vector3 (0,-transform.position.y + bigEyeRoomYOffset,0);
            } */
            bigEyeWorld.transform.position = transform.position + new Vector3(0, bigEyeRoomYOffset, 0);
        };
           
        while (percentage < 1)
        {
            percentage += Time.deltaTime * dissolvingSpeed;
            float progress = transitionSpeedCurve.Evaluate(percentage);
            bigEyeWorldMat.SetFloat("Effect_Radius", Mathf.Lerp(initialValue, targetValue, progress));
            bigEyeWorldMat.SetVector("Effect_Center", transform.position);
            worldMat.SetFloat("Effect_Radius", Mathf.Lerp(initialValue, targetValue, progress));
            worldMat.SetVector("Effect_Center", transform.position);
            if (grassMat != null) 
            {
                grassMat.SetFloat("Effect_Radius", Mathf.Lerp(initialValue, targetValue, progress));
                grassMat.SetVector("Effect_Center", transform.position);
            }
            yield return null;
        }
        if (!expanding)
            bigEyeWorld.SetActive(false);
        isInTransition = false;
    }
}
