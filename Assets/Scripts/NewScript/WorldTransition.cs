using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting.FullSerializer;

public class WorldTransition : MonoBehaviour
{
    public GameObject bigEyeWorld;
    public GameObject ground;
    public Material worldMat,grassMat,clippyMat;
    public AnimationCurve floatAnimationCurve,transitionSpeedCurve,cameraDolleyCurve,AnchorAnimationCurve;
    public Camera cam;
    public LayerMask groundMask;
    public float bigEyeRoomYOffset,dolleyDepth;

    public SceneDataObject sceneDataObj;
    // private Rigidbody rb;

    private Material bigEyeWorldMat;

    private bool isFloating = false, isInTransition = false, isInClippy = false;

    private List<GameObject> blissGameObjects = new List<GameObject>(), clippyGameObjects = new List<GameObject>();
    private Vector3 previousBlissPosition,previousClippyPosition;
    private GameObject blizzWrapper, clippyWrapper, clippyLoadPoint;

    public VolumeProfile blissVolume, clippyVolume;
    public Volume localVolume;

    public static Action<bool> OnClippyToggle;
    public static Action OnSelectedFileChange;



    private FileObject currentFile,previousFile;

    // ClippyFileSyatem
    private ClippyFileSystem clippyFileSystem;
    private List<Transform> clippyFileList;
    private List<FileObject> clippyFileLoaded = new List<FileObject>();
    private int fileIndex = 0;


    private bool isAnchoring = false;
    void Start()
    {
        StartCoroutine(WaitUntilSceneLoad());
    }

    IEnumerator WaitUntilSceneLoad() 
    {
        blizzWrapper = FindObjectOfType<BlissWrapper>().gameObject;
        AsyncOperation load = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Clippy", LoadSceneMode.Additive);
        while (!load.isDone) 
        {
            yield return null;
        }
        clippyWrapper = FindObjectOfType<ClippyWrapper>().gameObject;
        clippyLoadPoint = FindObjectOfType<ClippyLoadpoint>().gameObject;
        clippyFileSystem = FindObjectOfType<ClippyFileSystem>();
        clippyFileList = clippyFileSystem.fileTransform;
        clippyWrapper.SetActive(false);
    }
    private void OnEnable()
    {
        FileObject.OnFlieCollected += GetFileObject;
        SaveButton.OnSaveCurrentFile += SaveCurrentFile;

        SaveButton.OnSaveCurrentFile += DisablePlayerAnchor;
        QuitButton.OnQuitCurrentFile += DisablePlayerAnchor;

        FileObject.OnPlayerAnchored += AnchorPlayer;
        DeleteButton.OnDeleteObject += RemoveFile;
        FileObject.OnPlayerReleased += DisablePlayerAnchor;
            
    }
    private void OnDisable()
    {
        FileObject.OnFlieCollected -= GetFileObject;
        SaveButton.OnSaveCurrentFile -= SaveCurrentFile;

        SaveButton.OnSaveCurrentFile -= DisablePlayerAnchor;
        QuitButton.OnQuitCurrentFile -= DisablePlayerAnchor;

        FileObject.OnPlayerAnchored -= AnchorPlayer;
        DeleteButton.OnDeleteObject -= RemoveFile;
        FileObject.OnPlayerReleased -= DisablePlayerAnchor;

    }

    private void AnchorPlayer(Transform target) 
    {
        FirstPersonController player = GetComponent<FirstPersonController>();
        player.playerCanMove = false;
        player.GetComponent<Rigidbody>().isKinematic = true;
        //player.transform.position = target.position;

        StartCoroutine(PlayerAnchorAnimation(target.position, target.eulerAngles, 1.2f, player));
    }

    IEnumerator PlayerAnchorAnimation(Vector3 targetPos,Vector3 targetRot,float speed ,FirstPersonController player) 
    {
       // OnToggleDeleteButton?.Invoke(true);
        isAnchoring = true;
        float percent = 0;
        Vector3 initialPos = player.transform.position;
        Vector3 initialRot = player.transform.eulerAngles;
        while (percent < 1) 
        {
            float progress = AnchorAnimationCurve.Evaluate(percent);
            player.transform.position = Vector3.Lerp(initialPos, targetPos, progress);
            //player.transform.eulerAngles = Vector3.Lerp(initialRot, targetRot, progress);

            percent += Time.deltaTime * speed;
            yield return null;
        }
        
    }
    private void DisablePlayerAnchor() 
    {

        FirstPersonController player = GetComponent<FirstPersonController>();
        player.playerCanMove = true;
        player.GetComponent<Rigidbody>().isKinematic = false;
        //OnToggleDeleteButton?.Invoke(false);
        isAnchoring = false;
    }

    void GetFileObject(FileObject file) 
    {
        previousFile = file;
        if (previousFile != currentFile && currentFile != null) 
        {
            OnSelectedFileChange?.Invoke();
            print("You have changed File");
        }
        currentFile = previousFile;
        print("You have got " + currentFile.name + "selected");
    }

    void SaveCurrentFile()
    {
        print("You have Saved " + currentFile.name + "Congratulations!");
        if (!Array.Find(clippyFileLoaded.ToArray(),x => x.name == currentFile.name +  "(Clone)")) 
        {
           
            if (fileIndex <= clippyFileList.Count - 1)
            {
                FileObject f = Instantiate(currentFile);
                f.SwitchToClippyWorld();
                clippyFileLoaded.Add(f);
                f.transform.position = clippyFileList[fileIndex].position;
                
                f.transform.parent = clippyWrapper.transform;
                f.transform.forward = Vector3.right;
                f.SetCloseButtonPosition();
                fileIndex += 1;
            }
        }
    }
 
    void RemoveFile(FileObject fileToRemove) 
    {
        clippyFileLoaded.Remove(fileToRemove);
    }
    
    void Update()
    {
        SceneSwithcer();
        //OldScenenSwithcer();
    }
    void OldScenenSwithcer() 
    {
        if (Input.GetKeyDown(KeyCode.F) && !isAnchoring)
        {
            if (!sceneDataObj.isInClippyWorld)
            {
                sceneDataObj.isInClippyWorld = true;
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Clippy");
            }


            else
            {
                sceneDataObj.isInClippyWorld = false;
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Bliss");
            }
        }
    }
    void SceneSwithcer() 
    {
        if (Input.GetKeyDown(KeyCode.F) && !isAnchoring)
        {
            if (!isInClippy)
            {
                isInClippy = true;
                previousBlissPosition = transform.position;
                transform.position = clippyLoadPoint.transform.position;
                localVolume.profile = clippyVolume;
                blizzWrapper.SetActive(false);
                clippyWrapper.SetActive(true);

                //UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Clippy");
            }


            else
            {
                isInClippy = false;
                clippyLoadPoint.transform.position = transform.position;
                transform.position = previousBlissPosition;
                localVolume.profile = blissVolume;

                clippyWrapper.SetActive(false);
                blizzWrapper.SetActive(true);


                //UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Bliss");
            }
            OnClippyToggle?.Invoke(isInClippy);
            //InitiateWorldTransition();
        }
    }
    void InitiateWorldTransition() 
    {
        StartCoroutine(TransitionAnimation(1f));
        StartCoroutine(TransitionEffect(1f, isInClippy));
        StartCoroutine(CameraEffect(1f));
    }

    IEnumerator CameraEffect(float speed) 
    {
        float percentage = 0;
        float initialFieldOfView = cam.fieldOfView;
        float targetFieldOfView = initialFieldOfView + dolleyDepth;
        while (percentage < 1)
        {
            percentage += Time.deltaTime * speed;
            float animationValue = cameraDolleyCurve.Evaluate(percentage);
            cam.fieldOfView = Mathf.Lerp(initialFieldOfView, targetFieldOfView, animationValue);
            yield return null;
        }
    }

    IEnumerator TransitionAnimation(float floatingSpeed) 
    {
        isFloating = true;
        float percentage = 0;
        if (GetComponent<CharacterController>())
            GetComponent<CharacterController>().enabled = false;
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().isKinematic = true;
        float startY = transform.position.y;
        while (percentage < 1) 
        {
            percentage += Time.deltaTime * floatingSpeed;
            float targetY = floatAnimationCurve.Evaluate(percentage);
            transform.position = new Vector3 (transform.position.x,startY+ targetY,transform.position.z);
            yield return null;
        }
        if (GetComponent<CharacterController>())
            GetComponent<CharacterController>().enabled = true;
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().isKinematic = false;
        isFloating = false;
    }

    IEnumerator TransitionEffect(float dissolvingSpeed, bool expanding) 
    {
        isInTransition = true;
        float percentage = 0;
        float initialValue = expanding ? 0 : 200;
        float targetValue = expanding ? 200 : 0;
        if (expanding) 
        {
            bigEyeWorld.SetActive(true);
            /*
            Ray groundRay = new Ray(transform.position, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(groundRay, out hit, 100f, groundMask)) 
            {
                bigEyeWorld.transform.position = hit.point + new Vector3 (0,-transform.position.y + bigEyeRoomYOffset,0);
            } */
            bigEyeWorld.transform.position = transform.position + new Vector3(0, bigEyeRoomYOffset, 0);
        };
           
        while (percentage < 1)
        {
            percentage += Time.deltaTime * dissolvingSpeed;
            float progress = transitionSpeedCurve.Evaluate(percentage);
            clippyMat.SetFloat("Effect_Radius", Mathf.Lerp(initialValue, targetValue, progress));
            clippyMat.SetVector("Effect_Center", transform.position);
            worldMat.SetFloat("Effect_Radius", Mathf.Lerp(initialValue, targetValue, progress));
            worldMat.SetVector("Effect_Center", transform.position);
            if (grassMat != null) 
            {
                grassMat.SetFloat("Effect_Radius", Mathf.Lerp(initialValue, targetValue, progress));
                grassMat.SetVector("Effect_Center", transform.position);
            }
            yield return null;
        }
        if (expanding)
        {
            //if (isInBigEyeWorld)
                //UnityEngine.SceneManagement.SceneManager.LoadScene("Clippy");
        }
        else 
        {
            //if (!isInBigEyeWorld)
               // UnityEngine.SceneManagement.SceneManager.LoadScene("Bliss");
            bigEyeWorld.SetActive(false);
        }
          
        isInTransition = false;
    }

    public void Load() 
    {
        
        //UnityEngine.SceneManagement.SceneManager.UnloadScene("Bliss");
    }
}
