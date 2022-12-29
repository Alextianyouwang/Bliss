using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class SceneManager : MonoBehaviour
{
    //public RectTransform loadBar;

    private bool isGameStartScreenLoaded = false;


    public GameObject loadSceneScreen;
    public FirstPersonController fps;
    public GameObject zoomTarget;

    public static event Action<Vector3,Quaternion> OnZoomingToScreenRequested;

    private void OnEnable()
    {
       // Portal.OnEnterPortal += LoadScene;
    }
    private void OnDisable()
    {
       // Portal.OnEnterPortal -= LoadScene;

    }
    void Start()
    {
        loadSceneScreen.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !isGameStartScreenLoaded) 
        {
            isGameStartScreenLoaded = true;
            //OnZoomingToScreenRequested?.Invoke(zoomTarget.transform.position,zoomTarget.transform.rotation);
            StartLoadScreen();
        }
    }

    public void StartLoadScreen() 
    {
        loadSceneScreen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        fps.cameraCanMove = false;
        // Do load Animation

    }

    public void OnClickSwitchToBliss() 
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Bliss");
    }

    void LoadScene(int scene) 
    {
        StartCoroutine(LoadSceneProgress(scene));
        loadSceneScreen.SetActive(true);
    }
   
    IEnumerator LoadSceneProgress(int sceneIndex) 
    {
        
        AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneIndex,LoadSceneMode.Single);
        float percentage = 0;
        while (!operation.isDone) 
        {
            percentage += Time.deltaTime;
            operation.allowSceneActivation = true;
            float progress = percentage / 0.9f;
            float loadBarX = Mathf.Lerp(-140f, 140f, progress);

            print(percentage);
            yield return null;
        }
        loadSceneScreen.SetActive(false);
    }


}
