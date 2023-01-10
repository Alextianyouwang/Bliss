using System.Collections.Generic;
using UnityEngine;
// A global-accessable data container.
public class SceneData 
{
    public Vector3 previousBlissPosition;
    public GameObject blizzWrapper, floppyWraper, floppyLoadPoint;
    public FileObject prevFile, currFile, fileBeforeSwitchScene, mostRecentSavedFile;
    public ClippyFileSystem floppyFileSystem;
    public List<FileProjectorManager> floppyFileManagers;
    public FileObject[] clippyFileLoaded;
    public int fileIndex = 0;
    public SceneData() { }

}
