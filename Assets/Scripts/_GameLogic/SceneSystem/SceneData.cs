using System.Collections.Generic;
using UnityEngine;
// A global-accessable data container.
public class SceneData 
{
    public Vector3 previousBlissPosition;
    public GameObject blizzWrapper, floppyWraper, floppyLoadPoint;
    public GameObject gem_prefab, gemCollPlat_prefab, saveEffect_prefab , tile_prefab, saveButton_prefab, deleteButton_prefab;
    public FileObject prevFile, currFile, fileBeforeSwitchScene, mostRecentSavedFile;
    public ClippyFileSystem floppyFileSystem;
    public List<FileProjectorManager> floppyFileManagers;
    public FileObject[] clippyFileLoaded;
    public int fileIndex = 0;
    public SceneData() { }

    public NeedleManager needleManager;
    public TimelineManager timelineManager;
    public int howManyFileSaved = 0;




}
