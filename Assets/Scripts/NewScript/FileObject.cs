using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class FileObject : MonoBehaviour
{

    public SaveButton save;
    public QuitButton quit;
    public DeleteButton delete;
    private DeleteButton delete_instance;

    public Transform saveLoadPoint, closeLoadPoint, playerAnchor, deletePos;
    private bool hasBeenClickedMenuActivated = false, isInClippyWorld = false, isAnchoredInClippy = false, isNotAnchoredInClippy = true;
    
    public static Action OnClickReset;
    public static Action<FileObject> OnFlieCollected;
    public static Action<Transform> OnPlayerAnchored;
    public static Action OnPlayerReleased;

    public LayerMask clippyGroundMask;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnEnable()
    {
        QuitButton.OnQuitCurrentFile += ResetClick;
        SaveButton.OnSaveCurrentFile += ResetClick;
        WorldTransition.OnSelectedFileChange +=  ResetClick;

    }
    private void OnDisable()
    {
        QuitButton.OnQuitCurrentFile -= ResetClick;
        SaveButton.OnSaveCurrentFile -= ResetClick;
        WorldTransition.OnSelectedFileChange -= ResetClick;

    }

    void ResetClick() 
    {
        hasBeenClickedMenuActivated = false;
    }

    public void SwitchToClippyWorld() 
    {
        isInClippyWorld = true;
    }

    public void SetCloseButtonPosition() 
    {
  /*      Ray downRay = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        print("Delete Spawned");
        if (Physics.Raycast(downRay, out hit, 100f, clippyGroundMask)) 
        {
            DeleteButton newDelete = Instantiate(delete);
            delete.transform.position = hit.point;
            delete.transform.parent = gameObject.transform;
            print("Delete Spawned");
            newDelete.SetPariedFile(this);
        }
*/
        delete_instance = Instantiate(delete);
        delete_instance.transform.position = deletePos.position;
        //delete.transform.parent = transform;
        delete_instance.SetPariedFile(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Cursor")) 
        {
            if (collision.gameObject.GetComponent<CursorBlock>())
            {
                if (collision.gameObject.GetComponent<CursorBlock>().clickTimes == 1)
                {

                    if (!isInClippyWorld)
                    {
                        if (!hasBeenClickedMenuActivated)
                        {

                            OnFlieCollected?.Invoke(this);
                            hasBeenClickedMenuActivated = true;
                            SaveButton newSave = Instantiate(save);
                            newSave.transform.position = saveLoadPoint.position;
                            newSave.transform.forward = transform.forward;
                            QuitButton newQuit = Instantiate(quit);
                            newQuit.transform.position = closeLoadPoint.position;
                            newQuit.transform.forward = transform.forward;
                            OnPlayerAnchored?.Invoke(playerAnchor);
                        }
                    }
                    else
                    {
                        if (!isAnchoredInClippy)
                        {
                            isAnchoredInClippy = true;
                            OnPlayerAnchored?.Invoke(playerAnchor);
                            delete_instance.gameObject.SetActive(false);

                        }
                        else
                        {
                            OnPlayerReleased?.Invoke();
                            isAnchoredInClippy = false;
                            delete_instance.gameObject.SetActive(true);

                        }
                    }
                }
            }
                   
           
        }
    }
}
