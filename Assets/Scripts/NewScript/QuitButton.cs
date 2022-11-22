using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class QuitButton : MonoBehaviour
{
    public static Action OnQuitCurrentFile;
    private bool hasBeenClicked = false;


    private void OnEnable()
    {
        SaveButton.OnSaveCurrentFile += RemoveSelfFromScene;
        OnQuitCurrentFile += RemoveSelfFromScene;
        WorldTransition.OnSelectedFileChange += RemoveSelfFromScene;
    }
    private void OnDisable()
    {
        SaveButton.OnSaveCurrentFile -= RemoveSelfFromScene;
        OnQuitCurrentFile -= RemoveSelfFromScene;
        WorldTransition.OnSelectedFileChange -= RemoveSelfFromScene;


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
                        OnQuitCurrentFile?.Invoke();
                    }
                }
            }
        }
    }

    void RemoveSelfFromScene()
    {
        print("QuitButtonHasBeenDestroyed");

        Destroy(gameObject);
    }
    void ResetClicked() 
    {
        hasBeenClicked = false;
    }
}
