using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    public Transform loadPoint;

    public ThreeDUI quitObject;
    public ThreeDUI restartObject;

    bool escPressed;

    public static event System.Action OnPressEsc;

    bool isInClippy;
    void Start()
    {
        
    }

    private void OnEnable()
    {
        WorldTransition.OnClippyToggle += SetIsInClippy;
    }

    private void OnDisable()
    {
        WorldTransition.OnClippyToggle -= SetIsInClippy;

    }

    void SetIsInClippy(bool _clippy) 
    {
        isInClippy = _clippy;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) ) 
        {
            //AudioManager.instance.Play("Alert");
            OnPressEsc?.Invoke();
            if (!FindObjectOfType<ThreeDUI>()) 
            {
                
                ThreeDUI quit = Instantiate(quitObject);
                quit.isDisplayed = true;
                quit.transform.position = loadPoint.position;
                quit.transform.rotation = loadPoint.rotation;
                

                ThreeDUI restart = Instantiate(restartObject);
                
                restart.isDisplayed = true;
                restart.transform.position = loadPoint.position;
                restart.transform.rotation = loadPoint.rotation;

                if (isInClippy)
                {
                    quit.transform.parent = FindObjectOfType<ClippyWrapper>().transform;
                    restart.transform.parent = FindObjectOfType<ClippyWrapper>().transform;

                }
                else 
                {
                    quit.transform.parent = FindObjectOfType<BlissWrapper>().transform;
                    restart.transform.parent = FindObjectOfType<BlissWrapper>().transform;
                }
            }
            
        }
    }
}
