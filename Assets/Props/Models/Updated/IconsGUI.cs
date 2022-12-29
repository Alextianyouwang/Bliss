using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class IconsGUI : EditorWindow
{
    static float Indexer = 0;

    [MenuItem("Custom/Indexer")]
    static void Init()
    {
        EditorWindow window = GetWindow(typeof(IconsGUI));
        window.Show();
    }

    void OnGUI()
    {
        Indexer = EditorGUILayout.Slider(Indexer, 1, 100);
    }
}
