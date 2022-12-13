using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public static SceneData sd;

    public static bool isInClippy = false;

    public static Action<bool> OnClippyToggle;
    public static Action OnSceneDataLoaded;

    private void OnEnable()
    {
        PlayerAnchorAnimation.OnRequestSceneSwitch += SwitchScene;
    }
    private void OnDisable()
    {
        PlayerAnchorAnimation.OnRequestSceneSwitch -= SwitchScene;
        isInClippy = false;
    }

    private void Awake()
    {
        sd = new SceneData();

    }
    private void Start()
    {
        StartCoroutine(WaitUntilSceneLoad());
    }

    private void Update()
    {
        SceneSwitchingCheck();
    }
    IEnumerator WaitUntilSceneLoad()
    {
        sd.blizzWrapper = FindObjectOfType<BlissWrapper>().gameObject;
        AsyncOperation load = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Floppy", LoadSceneMode.Additive);
        while (!load.isDone)
        {
            yield return null;
        }
        sd.clippyWrapper = FindObjectOfType<ClippyWrapper>().gameObject;
        sd.clippyLoadPoint = FindObjectOfType<ClippyLoadpoint>().gameObject;
        sd.clippyFileSystem = FindObjectOfType<ClippyFileSystem>();
        sd.clippyFileLoadPosition = sd.clippyFileSystem.fileTransform;
        sd.clippyWrapper.SetActive(false);
        sd.clippyFileLoaded = new FileObject[sd.clippyFileSystem.transform.childCount];
        for (int i = 0; i < sd.clippyFileLoaded.Length; i++) { sd.clippyFileLoaded[i] = null; }
        OnSceneDataLoaded?.Invoke();
    }
    void SwitchScene()
    {

        if (!isInClippy)
        {
            isInClippy = true;
            sd.previousBlissPosition = transform.position;
            transform.position = sd.clippyLoadPoint.transform.position;

            sd.blizzWrapper.SetActive(false);
            sd.clippyWrapper.SetActive(true);

        }

        else
        {
            isInClippy = false;
            sd.clippyLoadPoint.transform.position = transform.position;

            transform.position = sd.previousBlissPosition;
            sd.clippyWrapper.SetActive(false);
            sd.blizzWrapper.SetActive(true);

        }
        OnClippyToggle?.Invoke(isInClippy);

    }
    void SceneSwitchingCheck()
    {
        if (Input.GetKeyDown(KeyCode.F) && ! PlayerAnchorAnimation. isAnchoring)
        {
            SwitchScene();
        }
    }
}
