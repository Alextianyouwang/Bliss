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
            sd.currFile.isSaved = true;
            
            FileObject f = Instantiate(sd.currFile);
            f.transform.position = sd.clippyFileLoadPosition[sd.fileIndex].position;
            f.transform.parent = sd.clippyFileSystem.transform;
            f.transform.forward = (sd.clippyFileSystem.transform.position - f.transform.position).normalized;
            f.transform.localScale *= 0.8f;
            f.SetIsAnchored(false);
            f.SetGroundPos();
            f.ResetFileAnimationValue();
            f.pairedMainFileWhenCloned = sd.currFile;
            sd.clippyFileLoaded[sd.fileIndex] = f;
        }
    }

    void DeleteCurrentFile()
    {
        RemoveFile(sd.currFile);
        sd.currFile.pairedMainFileWhenCloned.isSaved = false;
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
            // From File on the Field to File on the Filed 
            if (!sd.prevFile.GetComponent<FolderManager>()
               && !sd.currFile.GetComponent<FolderManager>()
               && sd.prevFile.parentFolder == null
               && sd.currFile.parentFolder == null
               )
            {
                sd.prevFile.SetIsAnchored(false);
                sd.currFile.SetIsAnchored(true);
                sd.prevFile.CloseFileAnimation();
            }
            // From File on the Field to Folder
            else if (
               !sd.prevFile.GetComponent<FolderManager>()
               && sd.currFile.GetComponent<FolderManager>()
                && sd.prevFile.parentFolder == null
                && sd.currFile.parentFolder == null
                )
            {
                sd.prevFile.SetIsAnchored(false);
                sd.currFile.SetIsAnchored(true);
                sd.prevFile.CloseFileAnimation();
            }
            // From Folder to File in Folder
            else if (
                sd.prevFile.GetComponent<FolderManager>()
                && sd.currFile.parentFolder != null
                )
            {
                sd.currFile.SetIsAnchored(true);
            }
            // From File in Folder to File on the Field
            else if (
                !sd.prevFile.GetComponent<FolderManager>()
                && !sd.currFile.GetComponent<FolderManager>()
                && sd.prevFile.parentFolder != null
                && sd.currFile.parentFolder == null
                )
            {

                sd.currFile.SetIsAnchored(true);
                sd.prevFile.parentFolder.SetIsAnchored(false);
                sd.prevFile.parentFolder.CloseFileAnimation();
                sd.prevFile.SetIsAnchored(false);
                sd.prevFile.CloseFileAnimation();
            }
            // From File in Folder to File in Folder 
            else if (sd.prevFile.parentFolder != null
                && sd.currFile.parentFolder != null)
            {
                sd.prevFile.SetIsAnchored(false);
                sd.currFile.SetIsAnchored(true);
                sd.prevFile.CloseFileAnimation();
            }

            // From File in Folder to Folder
            else if (
                !sd.prevFile.GetComponent<FolderManager>()
                && sd.currFile.GetComponent<FolderManager>()
                && sd.prevFile.parentFolder != null
                && sd.currFile.parentFolder == null
         
                )
            {
               
                sd.prevFile.SetIsAnchored(false);
                sd.currFile.SetIsAnchored(true);
                sd.prevFile.CloseFileAnimation();
                sd.prevFile.parentFolder.SetIsAnchored(false);
                // if enter a folder right after exit from a file inside that folder,
                // the animation will be set to open from its own instancescript but then set to close by the prevFile.parentFolder
                if (sd.prevFile.parentFolder != sd.currFile.GetComponent<FolderManager>())
                    sd.prevFile.parentFolder.CloseFileAnimation();

            }
            // From Folder to File on the Field
            else if (
                 sd.prevFile.GetComponent<FolderManager>()
                && sd.currFile.parentFolder == null
                )
            {
                sd.prevFile.SetIsAnchored(false);
                sd.currFile.SetIsAnchored(true);
                sd.prevFile.CloseFileAnimation();
            }
        }

        if (!sd.currFile.GetComponent<FolderManager>())
            OnTriggerSaveMatrix?.Invoke(sd.currFile);
        sd.prevFile = sd.currFile;
    }
}
