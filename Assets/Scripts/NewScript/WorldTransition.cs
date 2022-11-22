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
using Unity.VisualScripting.Antlr3.Runtime.Tree;

public class WorldTransition : MonoBehaviour
{

    public GameObject bigEyeWorld ;
    public GameObject ground;
    public Material worldMat,grassMat,clippyMat;
    public AnimationCurve floatAnimationCurve,transitionSpeedCurve,cameraDolleyCurve,AnchorAnimationCurve;
    public Camera cam;
    public LayerMask groundMask;
    public float bigEyeRoomYOffset,dolleyDepth;


    private bool isInClippy = false;

    private Vector3 previousBlissPosition;
    private GameObject blizzWrapper, clippyWrapper, clippyLoadPoint;

    public VolumeProfile blissVolume, clippyVolume;
    public Volume localVolume;

    public static Action<bool> OnClippyToggle;
    public static Action<FileObject,FileObject> OnSelectedFileChange;


    // ClippyFileSyatem

    private FileObject prevFile, currFile;
    private ClippyFileSystem clippyFileSystem;
    private List<Transform> clippyFileLoadPosition;
    private List<FileObject> clippyFileLoaded = new List<FileObject>();
    private int fileIndex = 0;

    private Coroutine anchorCo;


    private bool isAnchoring = false;
    void Start()
    {
        StartCoroutine(WaitUntilSceneLoad());
    }

    IEnumerator WaitUntilSceneLoad() 
    {
        blizzWrapper = FindObjectOfType<BlissWrapper>().gameObject;
        AsyncOperation load = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Floppy", LoadSceneMode.Additive);
        while (!load.isDone) 
        {
            yield return null;
        }
        clippyWrapper = FindObjectOfType<ClippyWrapper>().gameObject;
        clippyLoadPoint = FindObjectOfType<ClippyLoadpoint>().gameObject;
        clippyFileSystem = FindObjectOfType<ClippyFileSystem>();
        clippyFileLoadPosition = clippyFileSystem.fileTransform;
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

        anchorCo = StartCoroutine(PlayerAnchorAnimation(target.position, target.eulerAngles, 1.2f, player));
    }

    IEnumerator PlayerAnchorAnimation(Vector3 targetPos,Vector3 targetRot,float speed ,FirstPersonController player) 
    {
        isAnchoring = true;
        float percent = 0;
        Vector3 initialPos = player.transform.position;
        Vector3 initialRot = player.transform.eulerAngles;
        while (percent < 1) 
        {
            float progress = AnchorAnimationCurve.Evaluate(percent);
            player.transform.position = Vector3.Lerp(initialPos, targetPos, progress);

            percent += Time.deltaTime * speed;
            yield return null;
        }
        if (isInClippy)
            player.transform.parent = clippyFileSystem.transform;
        
    }
    private void DisablePlayerAnchor() 
    {
        FirstPersonController player = GetComponent<FirstPersonController>();
        player.playerCanMove = true;
        player.GetComponent<Rigidbody>().isKinematic = false;
        isAnchoring = false;
        player.transform.parent = null;
        StopCoroutine(anchorCo);
    }

    void GetFileObject(FileObject file) 
    {
        currFile = file;
        if (currFile != prevFile && prevFile != null) 
        {
            OnSelectedFileChange?.Invoke(currFile,prevFile);
            print("You have changed File");
        }
        prevFile = currFile;
        print("You have got " + prevFile.name + "selected");
    }

    void SaveCurrentFile()
    {
        print("You have Saved " + prevFile.name + "Congratulations!");
        if (!Array.Find(clippyFileLoaded.ToArray(),x => x.name == prevFile.name +  "(Clone)")) 
        {
           
            if (fileIndex <= clippyFileLoadPosition.Count - 1)
            {
                FileObject f = Instantiate(prevFile);
                f.SwitchToClippyWorld();
                clippyFileLoaded.Add(f);
                f.transform.position = clippyFileLoadPosition[fileIndex].position;
                
                f.transform.parent = clippyFileSystem.transform;
                f.transform.forward = (clippyLoadPoint.transform.position - f.transform.position).normalized;
                f.transform.localScale *= 0.8f;
                f.ResetIsAnchoredInClippy();
                f.SetCloseButtonPosition(clippyFileSystem.transform);
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
            }


            else
            {
                isInClippy = false;
                clippyLoadPoint.transform.position = transform.position;
                transform.position = previousBlissPosition;
                localVolume.profile = blissVolume;
                clippyWrapper.SetActive(false);
                blizzWrapper.SetActive(true);
            }
            OnClippyToggle?.Invoke(isInClippy);
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

    }

    IEnumerator TransitionEffect(float dissolvingSpeed, bool expanding) 
    {

        float percentage = 0;
        float initialValue = expanding ? 0 : 200;
        float targetValue = expanding ? 200 : 0;
        if (expanding) 
        {
            bigEyeWorld.SetActive(true);

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

        }
        else 
        {

            bigEyeWorld.SetActive(false);
        }
    }


}
