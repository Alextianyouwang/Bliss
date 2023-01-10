using UnityEngine;
using System;
using System.Collections;
using System.Threading.Tasks;
using System.Linq;

public class SaveButton : MonoBehaviour
{
    public static Action OnSaveCurrentFile;
    public static Action<bool> OnRetreatSaveButton;
    public static Action OnInitiateSaveAnimation;
    public static Action OnStartSaveEffect;
    public static Action OnPreIterateFileIndex;
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
                        if (!SceneSwitcher.sd.currFile.isSaved)
                            OnPreIterateFileIndex?.Invoke();

                        OnRetreatSaveButton?.Invoke(SceneSwitcher.sd.currFile.isSaved);
                        
                        if (!SceneSwitcher.sd.currFile.isSaved)
                            OnStartSaveEffect?.Invoke();
                        WaitExecute(!SceneSwitcher.sd.currFile.isSaved );
                        OnSaveCurrentFile?.Invoke();

                    }
                }
            }
        }
    }
   
    async void WaitExecute(bool willExecute) 
    {
        if (willExecute)
        {
            await Task.Delay(200);
            OnInitiateSaveAnimation?.Invoke();
        }
          

    }
}
