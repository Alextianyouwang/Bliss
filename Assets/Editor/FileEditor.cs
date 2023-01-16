using UnityEngine;
using UnityEditor;

/*[CustomEditor(typeof(FolderManager))]
public class FolderEditor : Editor
{
    private FolderManager _target;
    protected FolderManager.State originalDestructedState;
    private void OnEnable()
    {
        _target = (FolderManager)target;

    }


    public override void OnInspectorGUI()
    {
        Debug.Log(_target.state);
        _target.state = (FolderManager.State)EditorGUILayout.EnumPopup("DestructState", _target.state);

        if (originalDestructedState != _target.state)
        {
            _target.SetFileDestructionStateAndAppearance();
        }

        originalDestructedState = _target.state;

        base.DrawDefaultInspector();

    }


}*/
/*[CustomEditor(typeof(WordDocManager))]
public class WordDocEditor : Editor
{
    private WordDocManager _target;
    protected bool originalDestructedState;
    private void OnEnable()
    {
        _target = (WordDocManager)target;
    }

    public override void OnInspectorGUI()
    {
        
        _target.fileDestructed = EditorGUILayout.Toggle("IsFileDestructed", _target.fileDestructed);

        if (originalDestructedState != _target.fileDestructed)
        {
            _target.SetFileDestructionStateAndAppearance();
        }

        originalDestructedState = _target.fileDestructed;

        base.DrawDefaultInspector();

    }
}

[CustomEditor(typeof(JPGManager))]
public class JPGEditor : Editor
{
    private JPGManager _target;
    protected bool originalDestructedState;
    private void OnEnable()
    {
        _target = (JPGManager)target;
    }

    public override void OnInspectorGUI()
    {

        _target.fileDestructed = EditorGUILayout.Toggle("IsFileDestructed", _target.fileDestructed);

        if (originalDestructedState != _target.fileDestructed)
        {
            _target.SetFileDestructionStateAndAppearance();
        }

        originalDestructedState = _target.fileDestructed;

        base.DrawDefaultInspector();

    }
}

[CustomEditor(typeof(NotePadManager))]
public class NotePadEditor : Editor
{
    private NotePadManager _target;
    protected bool originalDestructedState;
    private void OnEnable()
    {
        _target = (NotePadManager)target;
    }

    public override void OnInspectorGUI()
    {

        _target.fileDestructed = EditorGUILayout.Toggle("IsFileDestructed", _target.fileDestructed);

        if (originalDestructedState != _target.fileDestructed)
        {
            _target.SetFileDestructionStateAndAppearance();
        }

        originalDestructedState = _target.fileDestructed;

        base.DrawDefaultInspector();

    }
}
[CustomEditor(typeof(VideoPlayerManager))]
public class VideoPlayerEditor : Editor
{
    private VideoPlayerManager _target;
    protected bool originalDestructedState;
    private void OnEnable()
    {
        _target = (VideoPlayerManager)target;
    }

    public override void OnInspectorGUI()
    {

        _target.fileDestructed = EditorGUILayout.Toggle("IsFileDestructed", _target.fileDestructed);

        if (originalDestructedState != _target.fileDestructed)
        {
            _target.SetFileDestructionStateAndAppearance();
        }

        originalDestructedState = _target.fileDestructed;

        base.DrawDefaultInspector();

    }
}

[CustomEditor(typeof(MusicPlayerManager))]
public class MusicPlayerEditor : Editor
{
    private MusicPlayerManager _target;
    protected bool originalDestructedState;
    private void OnEnable()
    {
        _target = (MusicPlayerManager)target;
    }

    public override void OnInspectorGUI()
    {

        _target.fileDestructed = EditorGUILayout.Toggle("IsFileDestructed", _target.fileDestructed);

        if (originalDestructedState != _target.fileDestructed)
        {
            _target.SetFileDestructionStateAndAppearance();
        }

        originalDestructedState = _target.fileDestructed;

        base.DrawDefaultInspector();

    }
}*/