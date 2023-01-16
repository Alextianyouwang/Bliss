using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // Create a static SceneData object.
    public static SceneData sd;
    public static bool isInFloppy = false;

    // Invoked when teleported between floppy and bliss.
    public static Action<bool> OnFloppyToggle;
    // Invoke to notify other class to set a local reference of the SceneData object.
    public static Action OnSceneDataLoaded;

    private void OnEnable()
    {
        AM_BlissMain.OnRequestSceneSwitch += SwitchScene;
    }
    private void OnDisable()
    {
        AM_BlissMain.OnRequestSceneSwitch -= SwitchScene;
        isInFloppy = false;
    }

    private void Awake()
    {
        sd = new SceneData();
        StartCoroutine(WaitUntilSceneLoad());

    }
    private void Start()
    {
    }

    private void Update()
    {
        SceneSwitchingCheck();
    }
    // Set reference for all the members of the SceneData Object.
    IEnumerator WaitUntilSceneLoad()
    {
        sd.blizzWrapper = FindObjectOfType<BlissWrapper>().gameObject;
        AsyncOperation load = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Floppy_IK", LoadSceneMode.Additive);
        while (!load.isDone)
        {
            yield return null;
        }
        sd.floppyWraper = FindObjectOfType<ClippyWrapper>().gameObject;
        sd.floppyLoadPoint = FindObjectOfType<ClippyLoadpoint>().gameObject;
        sd.floppyFileSystem = FindObjectOfType<ClippyFileSystem>();
        sd.floppyFileManagers = sd.floppyFileSystem.fileProjectors;
        sd.floppyWraper.SetActive(false);
        sd.clippyFileLoaded = new FileObject[sd.floppyFileManagers.Count];
        for (int i = 0; i < sd.clippyFileLoaded.Length; i++) { sd.clippyFileLoaded[i] = null; }
        OnSceneDataLoaded?.Invoke();
    }
    void SwitchScene()
    {
        if (!isInFloppy)
        {
            isInFloppy = true;
            sd.previousBlissPosition = transform.position;
            transform.position = sd.floppyLoadPoint.transform.position;

            if (sd.mostRecentSavedFile) 
            {
                Vector3 targetDir =(sd.mostRecentSavedFile.transform.position - transform.position).normalized;
                transform.forward = targetDir;
                transform.GetComponentInChildren<Camera>().transform.forward = targetDir;
            }

            sd.blizzWrapper.SetActive(false);
            sd.floppyWraper.SetActive(true);
        }

        else
        {
            isInFloppy = false;
            //sd.floppyLoadPoint.transform.position = transform.position;
            transform.position = sd.previousBlissPosition;

          
            sd.floppyWraper.SetActive(false);
            sd.blizzWrapper.SetActive(true);
        }
        OnFloppyToggle?.Invoke(isInFloppy);

    }
    void SceneSwitchingCheck()
    {
        if (Input.GetKeyDown(KeyCode.F) && ! AnchorAnimation. isAnchoring)
        {
            SwitchScene();
        }
    }
}
