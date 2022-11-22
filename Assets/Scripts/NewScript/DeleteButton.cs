using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DeleteButton : MonoBehaviour
{
    private FileObject pairedFile;
    public static Action<FileObject> OnDeleteObject;
    private bool hasBeenClicked = false;

    public void SetPariedFile(FileObject file) 
    {
        pairedFile = file;
    }
    private void OnEnable()
    {

    }
    private void OnDisable()
    {

    }
    void ToggleVisibility(bool isAnchoring) 
    {
        gameObject.SetActive(!isAnchoring);
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
                        print("File" + pairedFile.name + "Has been deleted");
                        OnDeleteObject?.Invoke(pairedFile);
                        Destroy(pairedFile.gameObject);
                        Destroy(gameObject);
                        hasBeenClicked = true;
                    }
                }
            }
          
        }
    }

}
