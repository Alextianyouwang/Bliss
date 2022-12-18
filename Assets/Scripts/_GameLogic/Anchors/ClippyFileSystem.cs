using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ClippyFileSystem : MonoBehaviour
{
    // Contains a list of transforms, which will be the parent object of saved files in FloppyWorld.
    public List<Transform> fileTransform = new List<Transform>();
    private void Awake()
    {
        fileTransform = gameObject.GetComponentsInChildren<Transform>().ToList();
        fileTransform.Remove(transform);
    }
}
