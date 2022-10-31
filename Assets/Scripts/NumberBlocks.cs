using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberBlocks : MonoBehaviour
{
    Rigidbody rb;

    protected bool hasCollided;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
    }
    void Update()
    {
        
    }
    public void Addforce(Vector3 direction, float force) 
    {
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Application") ||
            collision.gameObject.tag.Equals("Quit")||
            collision.gameObject.tag.Equals("Restart")) 
        {
            if (!hasCollided)
            {
                AudioManager.instance.Play("Click");
                hasCollided = true;
            }
        }
       
    }
    public void Sleep() 
    {
        rb.Sleep();
    }
}
