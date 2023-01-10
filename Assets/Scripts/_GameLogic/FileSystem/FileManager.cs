using System;
using UnityEngine;

// This class manages the file system and implement specific methord for individual file.
public class FileManager : MonoBehaviour
{
    private SceneData sd;
    public static bool isFileFull = false;
    public static Action<FileObject, FileObject> OnFileChange;
    public static Action<FileObject> OnTriggerSaveMatrix;

    private void OnEnable()
    {
        SceneSwitcher.OnSceneDataLoaded += GetSceneData;
        SceneSwitcher.OnFloppyToggle += UpdateFileBeforeSwitchScene_fromSceneSwitcher;
        SaveButton.OnStartSaveEffect += InitiateCurrentFileAnimation;
        SaveButton.OnSaveCurrentFile += SaveCurrentFile;
        SaveButton.OnPreIterateFileIndex += FindFirstEmptySpotAndCheckFullStatus;
        DeleteButton.OnDeleteObject += DeleteCurrentFile;
        DeleteButton.OnRefreshFileFullState += SetFileFullToFalse;
;
        FileObject.OnFlieCollected += GetFileObject;

    }
    private void OnDisable()
    {
        SceneSwitcher.OnFloppyToggle -= UpdateFileBeforeSwitchScene_fromSceneSwitcher;
        SceneSwitcher.OnSceneDataLoaded -= GetSceneData;

        SaveButton.OnStartSaveEffect -= InitiateCurrentFileAnimation;
        SaveButton.OnSaveCurrentFile -= SaveCurrentFile;
        SaveButton.OnPreIterateFileIndex -= FindFirstEmptySpotAndCheckFullStatus;

        DeleteButton.OnDeleteObject -= DeleteCurrentFile;
        DeleteButton.OnRefreshFileFullState -= SetFileFullToFalse;
        FileObject.OnFlieCollected -= GetFileObject;

        isFileFull = false;
    }

    void GetSceneData()
    {
        sd = SceneSwitcher.sd;
    }

    void FindFirstEmptySpotAndCheckFullStatus() 
    {
        isFileFull = Utility.CheckIfHasNumberOfNullInList(sd.clippyFileLoaded) == 1;
        if (Array.Find(SceneSwitcher.sd.clippyFileLoaded, x => x != null && x.pairedMainFileWhenCloned == sd.currFile))
            return;
        sd.fileIndex = Utility.GetFirstNullIndexInList(sd.clippyFileLoaded);
    }
    void SetFileFullToFalse()
    {
        isFileFull = false;
    }
    void SaveCurrentFile()
    {
        // only proceed to save if current list doesn't already contains it to prevent duplication.
        if (Array.Find(SceneSwitcher.sd.clippyFileLoaded, x => x != null && x.pairedMainFileWhenCloned == sd.currFile))
            return;
        if (sd.fileIndex < sd.clippyFileLoaded.Length)
        {
            sd.currFile.SetIsAnchored(false);
            sd.currFile.ResetFileAnimationValue();
            sd.currFile.SetIsSaved(true);

            FileObject f = Instantiate(sd.currFile);
            f.transform.position = sd.floppyFileManagers[sd.fileIndex].GetFileLoadPosition();
            f.transform.parent = sd.floppyFileSystem.transform;
            f.transform.forward = (sd.floppyFileSystem.transform.position - f.transform.position).normalized;
            f.transform.localScale *= 0.8f;
            f.SetIsAnchored(false);
            f.SetGroundPos();
            f.ResetFileAnimationValue();
            f.SetPairedMainFile(sd.currFile);
            sd.mostRecentSavedFile = f;
            sd.clippyFileLoaded[sd.fileIndex] = f;
            sd.floppyFileManagers[sd.fileIndex].SetContainedFile(f);
            sd.floppyFileManagers[sd.fileIndex].SetFileLightData(sd.currFile.lightData);
        }
    }

    void DeleteCurrentFile()
    {
        FileProjectorManager fileProjector = Array.Find(sd.floppyFileManagers.ToArray(), x => x.contianedFile == sd.currFile);
        fileProjector.TurnOff();

        RemoveFile(sd.currFile);
        sd.currFile.pairedMainFileWhenCloned.SetIsSaved(false);
        if (sd.mostRecentSavedFile == sd.currFile)
            sd.mostRecentSavedFile = null;
        Destroy(sd.currFile.gameObject);
    }
    void RemoveFile(FileObject fileToRemove)
    {
        for (int i = 0; i < sd.clippyFileLoaded.Length; i++)
        {
            if (sd.clippyFileLoaded[i] == fileToRemove)
                sd.clippyFileLoaded[i] = null;
        }
    }
    void InitiateCurrentFileAnimation()
    {
        sd.currFile.StartSaveEffect();
    }
    FileObject GetRoot(FileObject current) 
    {
        FileObject ultimate = current;
        while (ultimate.parent != null) 
        {
            ultimate = ultimate.parent;
        }
        return ultimate;
    }
    void BatchDeactivateAcrossDirectoryDepth(FileObject from,FileObject to) 
    {
        FileObject[] childs = from.childs;
        while (from != to)
        {
            if (childs != null)
            if (childs.Length != 0)
            foreach (FileObject f in childs)
            {
                if (f.isAnchored)
                    f.CloseFileAnimation();
                f.SetIsAnchored(false);
            }
            from.CloseFileAnimation();
            from.SetIsAnchored(false);
            from = from.parent;
        }
    }
    void UpdateFileBeforeSwitchScene_fromSceneSwitcher(bool b) 
    {
        if (b)
            sd.fileBeforeSwitchScene = sd.currFile;
        else
            sd.prevFile = sd.fileBeforeSwitchScene;
    }
    void GetFileObject(FileObject file)
    {
        sd.currFile = file;
        if (
            // Make sure the new file and the old file is not the same
            sd.currFile != sd.prevFile
            // Make sure this is not the first file selected
            && sd.prevFile != null
            )
        {
            OnFileChange?.Invoke(sd.currFile, sd.prevFile);
            SetFileStatusUponChange();
        }

        if (!sd.currFile.GetComponent<FolderManager>())
            OnTriggerSaveMatrix?.Invoke(sd.currFile);
        sd.prevFile = sd.currFile;
    }

    void SetFileStatusUponChange() 
    {
        //No matter how to traverse between files, the previous file Anchored flag has to be set to false.
        sd.prevFile.SetIsAnchored(false);
        sd.currFile.SetIsAnchored(true);
        // move from files
        if (
             !sd.prevFile.GetComponent<FolderManager>()
            )
        {
            // Same Level Movement
            if (sd.currFile.parent == sd.prevFile.parent)
                sd.prevFile.CloseFileAnimation();
            // Special Occation if enter a folder right after exit from a file inside that folder,
            // the animation will be set to open from its own instance script but then set to close by the prevFile.parentFolder
            else if (GetRoot(sd.prevFile) == sd.currFile.GetComponent<FolderManager>()) { }
            // Different Directory Movement 
            else if (GetRoot(sd.prevFile) != GetRoot(sd.currFile) && !SceneSwitcher.isInFloppy)
                BatchDeactivateAcrossDirectoryDepth(sd.prevFile, null);
            else
                if (!SceneSwitcher.isInFloppy)
                BatchDeactivateAcrossDirectoryDepth(sd.prevFile, sd.currFile.parent);
        }
        // move from folder
        else if (
            sd.prevFile.GetComponent<FolderManager>()
            )
        {
            // Same level movement
            if (sd.currFile.parent == sd.prevFile.parent)
                sd.prevFile.CloseFileAnimation();
            // Fetch childs within directory
            else if (sd.currFile.parent == sd.prevFile.GetComponent<FolderManager>())
                sd.currFile.SetIsAnchored(true);
            // Different Directory movement
            else if (GetRoot(sd.prevFile) != GetRoot(sd.currFile))
                BatchDeactivateAcrossDirectoryDepth(sd.prevFile, null);
            else
                BatchDeactivateAcrossDirectoryDepth(sd.prevFile, sd.currFile.parent);
        }
    }
}
