using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    private bool isRayHit = false;
    RaycastHit hit;
    public GameObject[] numbers;
    private KeyCode[] alphaKeys = {
        KeyCode.Alpha0,
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
        KeyCode.Mouse0
    };
    public Transform throwPoint;
    private bool prepareToThrow;
    NumberBlocks currentNumber = null;
    KeyCode currentKey;
    Vector3 refVel;

    private LineRenderer lr;
    public int trPointNumber;
    public Transform lrStartPoint;

    public Camera cam;
    public LayerMask interactionMask;

    public bool canStartControl;
    private void OnEnable()
    {
        //SceneManager.OnGameStart += ToggleStart;
    }
    private void OnDisable()
    {
        //SceneManager.OnGameStart -= ToggleStart;
    }

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = trPointNumber;
        currentKey = KeyCode.None;
    }

    private void Update()
    {
        if (isRayHit && canStartControl)
        {
            Vector3 targetVelocity = CalculateVelocity(hit.point, throwPoint.position, 0.4f);
            GetNumber(targetVelocity);

            for (int i = 0; i < trPointNumber; i++)
            {
                float timeBetweenEachIncrement = 0.05f;
                Vector3 straightLineVelocity = targetVelocity * i * timeBetweenEachIncrement;
                Vector3 downVelocity = 0.5f * Mathf.Abs(Physics.gravity.y) * i * timeBetweenEachIncrement * i * timeBetweenEachIncrement * Vector3.down;
                lr.SetPosition(i, lrStartPoint.position + straightLineVelocity + downVelocity);
            }
        }
    }

    void FixedUpdate()
    {
        if (canStartControl)
        {
            TrailUpdate();
        }
     
    }

    void ToggleStart() 
    {
        canStartControl = true;
    }

    private void TrailUpdate() 
    {
        Ray camRay = cam.ScreenPointToRay(Input.mousePosition);
       
        Vector3 targetVelocity = Vector3.zero;
        isRayHit = false;
        if (Physics.Raycast(camRay, out hit, 30f, interactionMask))
        {
            lr.enabled = true;
            
            isRayHit = true;
        }
        else 
        {
            lr.enabled = false;
            currentNumber = null;
        }

        
    }
    Vector3 CalculateVelocity(Vector3 target, Vector3 origin, float time) 
    {
        Vector3 distance = target - origin;
        Vector3 distanceXZ = distance;
        distanceXZ.y = 0f;

        float Sy = distance.y;
        float Sxz = distanceXZ.magnitude;

        float Vxz = Sxz / time;
        float Vy = Sy / time + 0.5f * Mathf.Abs(Physics.gravity.y) * time;

        Vector3 result = distanceXZ.normalized;
        result *= Vxz;
        result.y = Vy;

        return result;
    } 
    private void GetNumber(Vector3 targetVelocity) 
    {
        
        for (int i = 0; i < 11; i++) 
        {
            if (Input.GetKeyDown(alphaKeys[i]) && !prepareToThrow && !PlayerAnimationManager.isInTeleporting) 
            {
                currentKey = alphaKeys[i];
                currentNumber = Instantiate(numbers[i]).GetComponent<NumberBlocks>();
                currentNumber.transform.position = throwPoint.position;
                prepareToThrow = true;

                if (SceneSwitcher.isInClippy)
                {
                    currentNumber.transform.parent = FindObjectOfType<ClippyWrapper>().transform;
                }
                else
                {
                    currentNumber.transform.parent = FindObjectOfType<BlissWrapper>().transform;
                }
            }
            
            if (Input.GetKeyUp(alphaKeys[i]) && prepareToThrow && currentKey == alphaKeys[i] && !PlayerAnimationManager.isInTeleporting)
            {
                prepareToThrow = false;
                if (currentNumber != null) 
                {
                 currentNumber.GetComponent<Rigidbody>().velocity = targetVelocity;
                }
                currentNumber = null;
            }
        }

        if (prepareToThrow)
        {
            if (currentNumber != null) 
            {
                Rigidbody currentNumRb = currentNumber.GetComponent<Rigidbody>();
                currentNumRb?.Sleep();
                //currentNumber.transform.position = Vector3.SmoothDamp(currentNumber.transform.position, throwPoint.position, ref refVel, 0.1f);
                currentNumber.transform.position = throwPoint.position;
                currentNumber.transform.eulerAngles = transform.eulerAngles ;
            }   
        }

    }
}
