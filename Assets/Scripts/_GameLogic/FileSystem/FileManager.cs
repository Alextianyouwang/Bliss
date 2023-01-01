using System;
using UnityEngine;

// This class manages the file system and implement specific methord for individual file.
public class FileManager : MonoBehaviour
{
    private SceneData sd;
    // Invoked when the current file selection has been changed.
    public static Action<FileObject, FileObject> OnSelectedFileChange;

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
        if (sd.currFile != sd.prevFile && sd.prevFile != null)
        {
            OnSelectedFileChange?.Invoke(sd.currFile, sd.prevFile);
            sd.prevFile.SetIsAnchored(false);
            sd.currFile.SetIsAnchored(true);
            sd.prevFile.CloseFileAnimation();
        }
        sd.prevFile = sd.currFile;
    }
}
