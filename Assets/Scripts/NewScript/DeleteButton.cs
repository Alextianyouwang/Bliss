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
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Cursor"))
        {
            if (collision.gameObject.GetComponent<CursorBlock>())
            {
                print("HasClicked");
                if (collision.gameObject.GetComponent<CursorBlock>().clickTimes == 1) 
                {
                    if (!hasBeenClicked)
                    {
                        print("File" + pairedFile.name + "Has been deleted");
                        
                        Destroy(pairedFile.gameObject);
                        Destroy(gameObject);
                        OnDeleteObject?.Invoke(pairedFile);
                        print(pairedFile.name);
                        hasBeenClicked = true;
                    }
                }
            }
          
        }
    }

}
