using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SaveButton : MonoBehaviour
{
    public static Action OnSaveCurrentFile;
    private bool hasBeenClicked = false;

    private void OnEnable()
    {
        QuitButton.OnQuitCurrentFile += RemoveSelfFromScene;
        OnSaveCurrentFile += RemoveSelfFromScene;
        WorldTransition.OnSelectedFileChange += RemoveFromScene_FileChange;

    }
    private void OnDisable()
    {
        QuitButton.OnQuitCurrentFile -= RemoveSelfFromScene;
        OnSaveCurrentFile -= RemoveSelfFromScene;
        WorldTransition.OnSelectedFileChange -= RemoveFromScene_FileChange;



    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Cursor"))
        {
            if (collision.gameObject.GetComponent<CursorBlock>()) 
            {
                if (collision.gameObject.GetComponent<CursorBlock>().clickTimes == 1) 
                {


                    if (!hasBeenClicked)
                    {
                        OnSaveCurrentFile?.Invoke();
                    }
                }
            }
    

        }
    }
    void RemoveFromScene_FileChange(FileObject newFile, FileObject prevFile)
    {
        RemoveSelfFromScene();

    }
    void RemoveSelfFromScene()
    {
  
        Destroy(gameObject);
    }
    void ResetClicked()
    {
        hasBeenClicked = false;
    }
}
