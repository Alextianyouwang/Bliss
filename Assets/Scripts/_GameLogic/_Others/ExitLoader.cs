using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity;

public class ExitLoader : MonoBehaviour
{
    public Transform loadPoint;

    public ThreeDUI quitObject;
    public ThreeDUI restartObject;


    public bool willPlayStartScreen;
    public FirstPersonController player;
    public GameObject loadScreen;

    bool escPressed;

    public static event System.Action OnPressEsc;

    bool isInClippy;

    private void Awake()
    {
        if (willPlayStartScreen) 
        {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            player.playerCanMove = false;
            player.cameraCanMove = false;
            loadScreen.SetActive(true);
        }
    }
    void Start()
    {
        
    }

    private void OnEnable()
    {
        SceneDataMaster.OnFloppyToggle += SetIsInClippy;
    }

    private void OnDisable()
    {
        SceneDataMaster.OnFloppyToggle -= SetIsInClippy;
        
    }

    void SetIsInClippy(bool _clippy) 
    {
        isInClippy = _clippy;
    }

    public void StartGame() 
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        player.playerCanMove = true;
        player.cameraCanMove = true;
        loadScreen.SetActive(false);
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
                quit.transform.position = loadPoint.position + Vector3.up * 8f;
                quit.transform.rotation = loadPoint.rotation;
                

                ThreeDUI restart = Instantiate(restartObject);
                
                restart.isDisplayed = true;
                restart.transform.position = loadPoint.position + Vector3.up * 8f;
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
