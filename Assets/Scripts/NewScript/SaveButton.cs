using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SaveButton : MonoBehaviour
{
    public static Action OnSaveCurrentFile;
    public static Action<Vector3,Quaternion> OnInitiateSaveAnimation;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Cursor"))
        {
            if (collision.gameObject.GetComponent<CursorBlock>()) 
            {
                if (collision.gameObject.GetComponent<CursorBlock>().clickTimes == 1) 
                {
                    OnSaveCurrentFile?.Invoke();
                    OnInitiateSaveAnimation?.Invoke(transform.position + Vector3.up * 5 , Quaternion.LookRotation(Vector3.down,Vector3.up));
                }
            }
        }
    }
}
