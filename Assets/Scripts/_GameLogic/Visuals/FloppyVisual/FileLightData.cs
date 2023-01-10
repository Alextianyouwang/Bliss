using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName =  "FileLightDataObject")]
public class FileLightData : ScriptableObject
{
    [ColorUsage(true, false)]
    public Color matColor;
    public Color lightColor;

}
