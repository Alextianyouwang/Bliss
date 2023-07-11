using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDataMaster : MonoBehaviour
{

    public bool LoadSecondaryScene = true;
    // Create a static SceneData object.
    public static SceneData sd;
    public static bool isInFloppy = false;

    // Invoked when teleported between floppy and bliss.
    public static Action<bool> OnFloppyToggle;
    // Invoke to notify other class to set a local reference of the SceneData object.
    public static Action OnSceneDataLoaded;
    //Invoke when saved file for the first time then entering Floppy for cinematics.
    public static Action OnFloppyCinematics;

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
        LoadResources();
        StartCoroutine(WaitUntilSceneLoad());

    }
    private void Start()
    {

    }

    private void Update()
    {
        SceneSwitchingCheck();
    }

    private void LoadResources() 
    {
        sd.gem_prefab = Resources.Load("Props/Gem/P_Gem") as GameObject;
        sd.gemCollPlat_prefab = Resources.Load("Props/GemCollPlat/P_GemCollPlatform") as GameObject;
        sd.saveEffect_prefab = Resources.Load("Props/FileSaveEffect/P_FileSaveAnimationProp") as GameObject;
        sd.tile_prefab = Resources.Load("Props/LongTile/P_LongTile") as GameObject;
        sd.saveButton_prefab = Resources.Load("Props/SaveButton/P_DownloadM") as GameObject;
        sd.deleteButton_prefab = Resources.Load("Props/DeleteButton/P_CrossButtonM") as GameObject;
    }
    // Set reference for all the members of the SceneData Object.
    IEnumerator WaitUntilSceneLoad()
    {
        
    

        if (LoadSecondaryScene) 
        {
            sd.blizzWrapper = FindObjectOfType<BlissWrapper>().gameObject;
            AsyncOperation load = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Floppy_IK", LoadSceneMode.Additive);

            while (!load.isDone)
            {
                yield return null;
            }
            sd.needleManager = FindObjectOfType<NeedleManager>();
            sd.needleManager.FloppyFirstSavedTimeline = FindObjectOfType<TimelineManager>();
            sd.timelineManager = FindObjectOfType<TimelineManager>();
            sd.floppyWraper = FindObjectOfType<ClippyWrapper>().gameObject;
            sd.floppyLoadPoint = FindObjectOfType<ClippyLoadpoint>().gameObject;
            sd.floppyFileSystem = FindObjectOfType<ClippyFileSystem>();
            sd.floppyFileManagers = sd.floppyFileSystem.fileProjectors;
            sd.clippyFileLoaded = new FileObject[sd.floppyFileManagers.Count];

            for (int i = 0; i < sd.clippyFileLoaded.Length; i++) { sd.clippyFileLoaded[i] = null; }
            OnSceneDataLoaded?.Invoke();
            sd.floppyWraper.SetActive(false);
        }
       
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
                Vector3 targetDir = (sd.mostRecentSavedFile.transform.position - transform.position).normalized;
                transform.forward = targetDir;
                transform.GetComponentInChildren<Camera>().transform.forward = targetDir;
            }

            sd.blizzWrapper.SetActive(false);
            sd.floppyWraper.SetActive(true);

            // Invoke timeline to play when saved a file for the first time for UnfortunateKid.
            if (sd.howManyFileSaved.Equals(1))
                OnFloppyCinematics?.Invoke();
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
