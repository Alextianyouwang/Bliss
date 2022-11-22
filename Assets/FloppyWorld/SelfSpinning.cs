using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfSpinning : MonoBehaviour
{

    [SerializeField]
    private float rotationSpeed = 0.01f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(0, rotationSpeed, 0);
    }
}
