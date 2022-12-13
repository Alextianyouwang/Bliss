using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class FileObject : MonoBehaviour
{

    public Transform saveLoadPoint, closeLoadPoint, playerAnchor, deletePos;
    public Vector3 groundPosition, groundPositionInBliss;
    public bool isAnchored = false;
    public static Action OnClickReset;
    public static Action<FileObject> OnFlieCollected;
    public static Action<FileObject> OnPlayerAnchored;
    public static Action OnPlayerReleased;

    private RaycastHit hit;
    public LayerMask groundMask;
    void Start()
    {
        SetGroundPos();
    }

    private void OnEnable()
    {

        FileManager.OnSelectedFileChange +=  ResetAnchorFlag;
        OnFlieCollected += SetGroundPosition_wrapper;

    }
    private void OnDisable()
    {
        FileManager.OnSelectedFileChange -= ResetAnchorFlag;
        OnFlieCollected -= SetGroundPosition_wrapper;

    }

    public void SwitchToClippyWorld() 
    {
        isAnchored = false;
    }
    public void ResetIsAnchoredInClippy() 
    {
        isAnchored = false;
    }
    void SetGroundPosition_wrapper(FileObject f) 
    {
        SetGroundPos();
    }
    public void SetGroundPos() 
    {
        Ray groundRay = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(groundRay, out hit, 100f, groundMask))
        {
            groundPosition = hit.point;

        }
    }
    public void ResetAnchorFlag(FileObject newFile,FileObject prevFile) 
    {
        prevFile.isAnchored = false;
        newFile.isAnchored = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Cursor")) 
        {
            if (collision.gameObject.GetComponent<CursorBlock>())
            {
                if (collision.gameObject.GetComponent<CursorBlock>().clickTimes == 1)
                {
                    if (!isAnchored)
                    {
                        isAnchored = true;
                        OnFlieCollected?.Invoke(this);
                        OnPlayerAnchored?.Invoke(this);
                    }
                    else
                    {
                        isAnchored = false;
                        OnPlayerReleased?.Invoke();

                    }
                }
            }
        }
    }
}
