using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Interface3D : MonoBehaviour
{

    public bool FilesDubugger = false;

    void Update()
    {

        float value = FilesDubugger ? 1 : 0;

        var allFiles = FindObjectsOfType<MonoBehaviour>().OfType<IClickable>();
        foreach (IClickable Icons in allFiles)
        {
            Icons.FileClickControl(FilesDubugger, value);
        }
    }

}

    public interface IClickable
    {
        void FileClickControl(bool animState, float targetValue);
    }


