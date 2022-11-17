using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneDataObject", menuName = "ScriptableObjects")]
public class SceneDataObject : ScriptableObject
{

    public bool isInClippyWorld = false;

    public GameObject blissSceneWrapper, clippySceneWrapper;

}
