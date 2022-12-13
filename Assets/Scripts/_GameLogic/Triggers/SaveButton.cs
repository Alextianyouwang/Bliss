using UnityEngine;
using System;
using System.Collections;
using System.Threading.Tasks;

public class SaveButton : MonoBehaviour
{
    public static Action OnSaveCurrentFile;
    public static Action OnRetreatSaveButton;
    public static Action OnInitiateSaveAnimation;
    public static Action OnStartSaveEffect;
    public bool hasBeenClicked = false;
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
                        WaitExecute();
                        OnRetreatSaveButton?.Invoke();
                        OnSaveCurrentFile?.Invoke();
                        OnStartSaveEffect?.Invoke();
                    }
                }
            }
        }
    }
   
    async void WaitExecute() 
    {
        await Task.Delay(200);
        OnInitiateSaveAnimation?.Invoke();

    }
}
