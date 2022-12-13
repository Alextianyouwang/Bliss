using System;
using UnityEngine;
public class FileManager : MonoBehaviour
{
    private SceneData sd;
    public static Action<FileObject, FileObject> OnSelectedFileChange;

    private void OnEnable()
    {
        SaveButton.OnSaveCurrentFile += SaveCurrentFile;
        DeleteButton.OnDeleteObject += DeleteCurrentFile;
        FileObject.OnFlieCollected += GetFileObject;
        SceneSwitcher.OnSceneDataLoaded += GetSceneData;
        SaveButton.OnStartSaveEffect += InitiateCurrentFileAnimation;
    }
    private void OnDisable()
    {
        SaveButton.OnSaveCurrentFile -= SaveCurrentFile;
        DeleteButton.OnDeleteObject -= DeleteCurrentFile;
        FileObject.OnFlieCollected -= GetFileObject;
        SceneSwitcher.OnSceneDataLoaded -= GetSceneData;
        SaveButton.OnStartSaveEffect += InitiateCurrentFileAnimation;

    }

    void GetSceneData() 
    {
        sd = SceneSwitcher.sd;
    }

    void SaveCurrentFile()
    {
        if (!Array.Find(SceneSwitcher.sd.clippyFileLoaded, x => x != null && x.name == sd.prevFile.name + "(Clone)"))
        {
            sd.fileIndex = GetFirstNullIndexInList(sd.clippyFileLoaded);
            if (sd.fileIndex < sd.clippyFileLoaded.Length)
            {
                sd.currFile.ResetIsAnchored();
                FileObject f = Instantiate(sd.currFile);
                f.SwitchToClippyWorld();

                f.transform.position = sd.clippyFileLoadPosition[sd.fileIndex].position;
                f.transform.parent = sd.clippyFileSystem.transform;
                f.transform.forward = (sd.clippyFileSystem.transform.position - f.transform.position).normalized;
                f.transform.localScale *= 0.8f;
                f.ResetIsAnchored();
                f.isAnchored = false;
                sd.clippyFileLoaded[sd.fileIndex] = f;
            }
        }
    }
    void DeleteCurrentFile()
    {
        RemoveFile(sd.currFile);
        Destroy(sd.currFile.gameObject);
    }
    int GetFirstNullIndexInList<T>(T[] array)
    {
        foreach (T t in array)
        {
            if (t == null)
                return Array.IndexOf(array, t);
        }
        return array.Length;
    }

    void RemoveFile(FileObject fileToRemove)
    {
        for (int i = 0; i < sd.clippyFileLoaded.Length; i++)
        {

            if (sd.clippyFileLoaded[i] == fileToRemove)
            {
                sd.clippyFileLoaded[i] = null;
            }
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
        }
        sd.prevFile = sd.currFile;
    }
}
