using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
public class FileManager : MonoBehaviour
{
    private SceneData sd;
    public static Action<FileObject, FileObject> OnSelectedFileChange;

    private void OnEnable()
    {
        SaveButton.OnSaveCurrentFile += SaveCurrentFile;
        DeleteButton.OnDeleteObject += DeleteCurrentFile;
        FileObject.OnFlieCollected += GetFileObject;
        SceneSwitcher.OnSceneDataCreated += ReceiveSceneData;
    }
    private void OnDisable()
    {
        SaveButton.OnSaveCurrentFile -= SaveCurrentFile;
        DeleteButton.OnDeleteObject -= DeleteCurrentFile;
        FileObject.OnFlieCollected -= GetFileObject;
        SceneSwitcher.OnSceneDataCreated -= ReceiveSceneData;

    }

    void ReceiveSceneData(SceneData _sd) 
    {
        sd = _sd;
    }
    void SaveCurrentFile()
    {
        if (!Array.Find(sd.clippyFileLoaded, x => x != null && x.name == sd.prevFile.name + "(Clone)"))
        {
            sd.fileIndex = GetFirstNullIndexInList(sd.clippyFileLoaded);
            if (sd.fileIndex < sd.clippyFileLoaded.Length)
            {
                FileObject f = Instantiate(sd.currFile);
                f.SwitchToClippyWorld();

                f.transform.position = sd.clippyFileLoadPosition[sd.fileIndex].position;
                f.transform.parent = sd.clippyFileSystem.transform;
                f.transform.forward = (sd.clippyFileSystem.transform.position - f.transform.position).normalized;
                f.transform.localScale *= 0.8f;
                f.ResetIsAnchoredInClippy();
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
