using System;
using UnityEngine;

// This class manages the file system and implement specific methord for individual file.
public class FileManager : MonoBehaviour
{
    private SceneData sd;
    public static Action<FileObject, FileObject> OnFileChange;
    public static Action<FileObject> OnTriggerSaveMatrix;

    private void OnEnable()
    {
        SceneSwitcher.OnSceneDataLoaded += GetSceneData;
        SaveButton.OnStartSaveEffect += InitiateCurrentFileAnimation;
        SaveButton.OnSaveCurrentFile += SaveCurrentFile;
        DeleteButton.OnDeleteObject += DeleteCurrentFile;
        FileObject.OnFlieCollected += GetFileObject;
    }
    private void OnDisable()
    {
        SaveButton.OnSaveCurrentFile -= SaveCurrentFile;
        DeleteButton.OnDeleteObject -= DeleteCurrentFile;
        FileObject.OnFlieCollected -= GetFileObject;
        SceneSwitcher.OnSceneDataLoaded -= GetSceneData;
        SaveButton.OnStartSaveEffect -= InitiateCurrentFileAnimation;
    }

    void GetSceneData()
    {
        sd = SceneSwitcher.sd;
    }

    void SaveCurrentFile()
    {
        // only proceed to save if current list doesn't already contains it to prevent duplication.
        if (Array.Find(SceneSwitcher.sd.clippyFileLoaded, x => x != null && x.name == sd.prevFile.name + "(Clone)"))
            return;
        sd.fileIndex = Utility.GetFirstNullIndexInList(sd.clippyFileLoaded);
        if (sd.fileIndex < sd.clippyFileLoaded.Length)
        {
            sd.currFile.SetIsAnchored(false);
            sd.currFile.ResetFileAnimationValue();
            sd.currFile.SetIsSaved(true);
            
            FileObject f = Instantiate(sd.currFile);
            f.transform.position = sd.clippyFileLoadPosition[sd.fileIndex].position;
            f.transform.parent = sd.clippyFileSystem.transform;
            f.transform.forward = (sd.clippyFileSystem.transform.position - f.transform.position).normalized;
            f.transform.localScale *= 0.8f;
            f.SetIsAnchored(false);
            f.SetGroundPos();
            f.ResetFileAnimationValue();
            f.SetPairedMainFile(sd.currFile);
            sd.clippyFileLoaded[sd.fileIndex] = f;
        }
    }

    void DeleteCurrentFile()
    {
        RemoveFile(sd.currFile);
        sd.currFile.pairedMainFileWhenCloned.SetIsSaved(false);
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
    void BatchDeactivateBetweenDirectoryDepth(FileObject from,FileObject to) 
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
                // the animation will be set to open from its own instancescript but then set to close by the prevFile.parentFolder
                else if (GetRoot(sd.prevFile) == sd.currFile.GetComponent<FolderManager>()){}
                // Different Directory Movement
                else if (GetRoot(sd.prevFile) != GetRoot(sd.currFile) && !SceneSwitcher.isInClippy)
                    BatchDeactivateBetweenDirectoryDepth(sd.prevFile, null);
                else
                    if(!SceneSwitcher.isInClippy)
                        BatchDeactivateBetweenDirectoryDepth(sd.prevFile, sd.currFile.parent);
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
                    BatchDeactivateBetweenDirectoryDepth(sd.prevFile, null);
                else
                    BatchDeactivateBetweenDirectoryDepth(sd.prevFile, sd.currFile.parent);
            }
        }

        if (!sd.currFile.GetComponent<FolderManager>())
            OnTriggerSaveMatrix?.Invoke(sd.currFile);
        sd.prevFile = sd.currFile;
    }
}
