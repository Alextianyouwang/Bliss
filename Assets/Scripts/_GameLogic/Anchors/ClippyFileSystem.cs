using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ClippyFileSystem : MonoBehaviour
{
    // Contains a list of transforms, which will be the parent object of saved files in FloppyWorld.
    //public List<Transform> fileTransform = new List<Transform>();
    public List<FileProjectorManager> fileProjectors = new List<FileProjectorManager>();
    private void Awake()
    {
        fileProjectors = GetComponentsInChildren<FileProjectorManager>().ToList();
        fileProjectors = fileProjectors.Where(x => x.transform.parent == transform).ToList();
        //fileTransform = GetComponentsInChildren<Transform>().ToList();
        //fileTransform = fileTransform.Where(x => x.name == "FileLoadingPoint").ToList();
    }
}
