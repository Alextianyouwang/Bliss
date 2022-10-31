using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    public GameObject loadSceneScreen;
    public RectTransform loadBar;

    public FirstPersonController fps;
    public bool isGameStarted;

    public static event System.Action OnGameStart;
    private void OnEnable()
    {
        Portal.OnEnterPortal += LoadScene;
    }
    private void OnDisable()
    {
        Portal.OnEnterPortal -= LoadScene;

    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void StartGame() 
    {
        fps.playerCanMove = true;
        fps.cameraCanMove = true;
        Cursor.lockState = CursorLockMode.Locked;

        OnGameStart?.Invoke();
        AudioManager.instance.Play("P1Music");
        AudioManager.instance.Play("P1Ambience");
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
            loadBar.position = new Vector2(loadBarX, loadBar.position.y);
            print(percentage);
            yield return null;
        }
        loadSceneScreen.SetActive(false);
    }


}
