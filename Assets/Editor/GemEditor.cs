using UnityEngine;
using UnityEditor;

/*[CustomEditor(typeof(Gem))]
public class GemEditor :Editor
{
    private Gem _target;
    private Gem.GemTypes originalType;
    private void OnEnable()
    {
        _target = (Gem)target;
    }

    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        _target.gemType = (Gem.GemTypes)EditorGUILayout.EnumPopup("Type", _target.gemType);

        if (originalType != _target.gemType)
        {
            _target.ChangeColor();
        }

        originalType = _target.gemType;

    }
}*/

