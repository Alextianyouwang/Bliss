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
        AM_Menu.OnZoomFinished += StartLoadScreen;
    }
    private void OnDisable()
    {
        AM_Menu.OnZoomFinished -= StartLoadScreen;
    }
    void Start()
    {
        loadSceneScreen.SetActive(false);
        fps.allowYawLock = true;
        fps.maxYawAngle = 40f;
        fps.maxPitchAngle = 50f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !isGameStartScreenLoaded) 
        {
            isGameStartScreenLoaded = true;
            OnZoomingToScreenRequested?.Invoke(zoomTarget.transform.position,zoomTarget.transform.rotation);
        }
    }

    public void StartLoadScreen() 
    {
        loadSceneScreen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        fps.cameraCanMove = false;


    }

    public void OnClickSwitchToBliss() 
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Bliss");
        fps.cameraCanMove = true;

        fps.allowYawLock = false;
        //fps.maxYawAngle = 40f;
        fps.maxPitchAngle = 80f;
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
