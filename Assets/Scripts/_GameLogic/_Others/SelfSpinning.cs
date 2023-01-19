using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class SelfSpinning : MonoBehaviour
{

    [SerializeField] private float rotationSpeed = 0.01f;
    private bool canRotate = true;

    private Vector3 initialRotation;

    private void OnEnable()
    {
        FileObject.OnPlayerAnchored += StopSpin_fromFileObject;
        FileObject.OnPlayerReleased += CanSpin;
    }
    private void OnDisable()
    {
        FileObject.OnPlayerAnchored -= StopSpin_fromFileObject;
        FileObject.OnPlayerReleased -= CanSpin;
    }
    private void Awake()
    {
        initialRotation = transform.eulerAngles;
    }
    void StopSpin_fromFileObject(FileObject f) 
    {
        if (SceneSwitcher.sd.howManyFileSaved.Equals(1))
        {
            transform.eulerAngles = initialRotation;
        }

        canRotate = false;
    }
    void CanSpin()
    {
        canRotate = true;
    }

    void Update()
    {
        if (canRotate && !FileProjectorManager.isPerformingFileDisplayAnimation)
        {
            transform.Rotate(0, rotationSpeed, 0);
        }
    }

    public void SignalStopSpin()
    {
        StopSpin_fromFileObject(null);
    }

    public void SignalEnd()
    {
        CanSpin();
    }
}
