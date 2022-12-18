using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private float mouseSensitivityX;
    [SerializeField] private float mouseSensitivityY;

    [SerializeField] private Transform playerBody;
    private float verticalRotation = 0;
    private bool canFreeRotate= true;

    float smoothedX;
    float smoothedY;
    float xVel;
    float yVel;
    private void OnEnable()
    {

    }
    private void OnDisable()
    {


    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        if (canFreeRotate) 
        {
            RotateCamera();
        }
    }

    private void RotateCamera() 
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY * Time.deltaTime;

        smoothedX = Mathf.SmoothDamp(smoothedX, mouseX, ref xVel, 0.02f);
        smoothedY = Mathf.SmoothDamp(smoothedY, mouseY, ref yVel, 0.02f);

        verticalRotation += smoothedY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        Vector3 eulerAngle = transform.eulerAngles;
        eulerAngle.x = -verticalRotation;
        transform.eulerAngles = eulerAngle;

        playerBody.Rotate(Vector3.up * smoothedX);    
    }

    void ToggleRotation(bool toggle) 
    {
        canFreeRotate = toggle;
    }
}
