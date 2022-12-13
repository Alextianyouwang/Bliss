using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfSpinning : MonoBehaviour
{

    [SerializeField] private float rotationSpeed = 0.01f;
    private bool canRotate = true;
    private void OnEnable()
    {
        FileObject.OnPlayerAnchored += StopSpin;
        FileObject.OnPlayerReleased += CanSpin;
    }
    private void OnDisable()
    {
        FileObject.OnPlayerAnchored -= StopSpin;
        FileObject.OnPlayerReleased -= CanSpin;
    }

    void StopSpin(FileObject f) 
    {
        canRotate = false;
    }
    void CanSpin()
    {
        canRotate = true;
    }

    void Update()
    {
        if (canRotate) 
        {
            transform.Rotate(0, rotationSpeed, 0);
        }
    }
}
