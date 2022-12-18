using System.Collections.Generic;
using UnityEngine;
// A global-accessable data container.
public class SceneData 
{
    public Vector3 previousBlissPosition;
    public GameObject blizzWrapper, clippyWrapper, clippyLoadPoint;
    public FileObject prevFile, currFile;
    public ClippyFileSystem clippyFileSystem;
    public List<Transform> clippyFileLoadPosition;
    public FileObject[] clippyFileLoaded;
    public int fileIndex = 0;
    public SceneData() { }

}
