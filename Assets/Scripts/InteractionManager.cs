using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
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
        SceneManager.OnGameStart += ToggleStart;
    }
    private void OnDisable()
    {
        SceneManager.OnGameStart -= ToggleStart;

    }
    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = trPointNumber;
        currentKey = KeyCode.None;
    }

    void Update()
    {
        
       
        
    }

    void ToggleStart() 
    {
        canStartControl = true;
    }

    private void LateUpdate()
    {
        if (canStartControl) 
        {
            TrailUpdate();
        }

    }

    private void TrailUpdate() 
    {
        Ray camRay = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 targetVelocity = Vector3.zero;
        if (Physics.Raycast(camRay, out hit, 30f, interactionMask))
        {
            lr.enabled = true;
            targetVelocity = CalculateVelocity(hit.point, throwPoint.position, 1f);
            GetNumber(targetVelocity);
        }
        else 
        {
            lr.enabled = false;
            currentNumber = null;
        }

        for (int i = 0; i < trPointNumber; i++)
        {
            float timeBetweenEachIncrement = 0.05f;
            Vector3 straightLineVelocity = targetVelocity * i * timeBetweenEachIncrement;
            Vector3 downVelocity = 0.5f * 9.81f * i * timeBetweenEachIncrement * i * timeBetweenEachIncrement * Vector3.down;
            lr.SetPosition(i, lrStartPoint.position + straightLineVelocity + downVelocity);
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
            if (Input.GetKeyDown(alphaKeys[i]) && !prepareToThrow) 
            {
                currentKey = alphaKeys[i];
                currentNumber = Instantiate(numbers[i]).GetComponent<NumberBlocks>();
                currentNumber.transform.position = throwPoint.position;
                prepareToThrow = true;
            }
            
            if (Input.GetKeyUp(alphaKeys[i]) && prepareToThrow && currentKey == alphaKeys[i] )
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
                currentNumber.transform.eulerAngles = transform.eulerAngles - new Vector3(0, 180, 0);
            }
            
           
        }

    }
}