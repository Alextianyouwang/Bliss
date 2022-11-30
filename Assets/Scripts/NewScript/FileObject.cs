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
    public Vector3 groundPositionInClippy, groundPositionInBliss;
    private bool hasBeenClickedMenuActivated = false, isInClippyWorld = false, isAnchoredInClippy = false, isNotAnchoredInClippy = true;
    
    public static Action OnClickReset;
    public static Action<FileObject> OnFlieCollected;
    public static Action<FileObject> OnPlayerAnchored;
    public static Action OnPlayerReleased;

    private RaycastHit hit;
    public LayerMask groundMask;
    void Start()
    {
        SetGroundPosition();
    }

    void Update()
    {
        
    }

    private void OnEnable()
    {
        QuitButton.OnQuitCurrentFile += ResetIsClickedInBliss;
        SaveButton.OnSaveCurrentFile += ResetIsClickedInBliss;
        WorldTransition.OnSelectedFileChange +=  ResetAnchorFlag;

    }
    private void OnDisable()
    {
        QuitButton.OnQuitCurrentFile -= ResetIsClickedInBliss;
        SaveButton.OnSaveCurrentFile -= ResetIsClickedInBliss;
        WorldTransition.OnSelectedFileChange -= ResetAnchorFlag;

    }


    void ResetIsClickedInBliss() 
    {
        hasBeenClickedMenuActivated = false;
    }

    public void SwitchToClippyWorld() 
    {
        isInClippyWorld = true;
    }
    public void ResetIsAnchoredInClippy() 
    {
        isAnchoredInClippy = false;
    }
    public void SetGroundPosition() 
    {
        Ray groundRay = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(groundRay, out hit, 100f, groundMask)) 
        {
            groundPositionInBliss = hit.point;
            //print(groundPositionInBliss);
        }
    }
    public void SetGroundPositioninClippy() 
    {
        Ray groundRay = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(groundRay, out hit, 100f, groundMask))
        {
            groundPositionInClippy = hit.point;
            //print(groundPositionInClippy);


        }
    }
    public void SetCloseButtonPosition(Transform clippySystemTransform) 
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

        delete_instance.transform.parent = clippySystemTransform;
        delete_instance.SetPariedFile(this);
    }

    public void ResetAnchorFlag(FileObject newFile,FileObject prevFile) 
    {
        prevFile.hasBeenClickedMenuActivated = false;
        prevFile.isAnchoredInClippy = false;
        newFile.isAnchoredInClippy = true;
        if (isInClippyWorld)
            delete_instance.gameObject.SetActive(true);
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
                            OnPlayerAnchored?.Invoke(this);
                        }
                    }
                    else
                    {
                        if (!isAnchoredInClippy)
                        {
                            isAnchoredInClippy = true;
                            OnFlieCollected?.Invoke(this);
                            OnPlayerAnchored?.Invoke(this);
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
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(groundPositionInBliss, 3f);
    }
}
