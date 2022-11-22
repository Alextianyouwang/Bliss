using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ClippyFileSystem : MonoBehaviour
{
    public List<Transform> fileTransform = new List<Transform>();
    private void Awake()
    {
        fileTransform = gameObject.GetComponentsInChildren<Transform>().ToList();
        fileTransform.Remove(transform);
    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
