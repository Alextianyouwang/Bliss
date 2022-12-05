using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class WorldTransition : MonoBehaviour
{

    public GameObject bigEyeWorld ;
    public GameObject ground;
    public Material worldMat,grassMat,clippyMat;
    public AnimationCurve floatAnimationCurve,transitionSpeedCurve,cameraDolleyCurve,AnchorAnimationCurve
        ;
    public Camera cam;
    public LayerMask groundMask;
    public float bigEyeRoomYOffset,dolleyDepth;


    private bool isInClippy = false;

    private Vector3 previousBlissPosition;
    private Vector3 cameraFollowRef;
    private GameObject blizzWrapper, clippyWrapper, clippyLoadPoint;

    public VolumeProfile blissVolume, clippyVolume;
    public Volume localVolume;

    public static Action<bool> OnClippyToggle;
    public static Action<FileObject,FileObject> OnSelectedFileChange;
    public static Action<Vector3> OnStageFile;
    public static Action OnPlayerExitAnchor;


    // ClippyFileSyatem
    FirstPersonController player;
    private FileObject prevFile, currFile;
    private ClippyFileSystem clippyFileSystem;
    private List<Transform> clippyFileLoadPosition;
    private FileObject[] clippyFileLoaded;
    private int fileIndex = 0;

    private Coroutine anchorCo;

    private Vector3 playerPosRef,playerCamRotRef,playerRotRef,playerCamPosRef;
    private Vector3 playerPositionBeforeAnchorAnimation;

    // SaveFileAnimation

    

    private bool isAnchoring = false;
    void Start()
    {

        player = GetComponent<FirstPersonController>();
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
        InitiateStuffAfterLoad();
    }
    void InitiateStuffAfterLoad() 
    {
        clippyFileLoaded = new FileObject[clippyFileSystem.transform.childCount];
        for (int i = 0; i < clippyFileLoaded.Length; i++) { clippyFileLoaded[i] = null; }
    }
    private void OnEnable()
    {
        FileObject.OnFlieCollected += GetFileObject;
        SaveButton.OnSaveCurrentFile += SaveCurrentFile;

        //SaveButton.OnSaveCurrentFile += DisablePlayerAnchor;
        SaveButton.OnInitiateSaveAnimation += InitiateSaveAnimation;
        QuitButton.OnQuitCurrentFile += DisablePlayerAnchor;

        FileObject.OnPlayerAnchored += AnchorPlayer;
        DeleteButton.OnDeleteObject += RemoveFile;
        FileObject.OnPlayerReleased += DisablePlayerAnchor;

        ModularMatrix.OnInitiateTeleportFromMatrix += InitiateDiveAnimation;
            
    }
    private void OnDisable()
    {
        FileObject.OnFlieCollected -= GetFileObject;
        SaveButton.OnSaveCurrentFile -= SaveCurrentFile;

        //SaveButton.OnSaveCurrentFile -= DisablePlayerAnchor;
        SaveButton.OnInitiateSaveAnimation -= InitiateSaveAnimation;

        QuitButton.OnQuitCurrentFile -= DisablePlayerAnchor;

        FileObject.OnPlayerAnchored -= AnchorPlayer;
        DeleteButton.OnDeleteObject -= RemoveFile;
        FileObject.OnPlayerReleased -= DisablePlayerAnchor;

        ModularMatrix.OnInitiateTeleportFromMatrix -= InitiateDiveAnimation;


    }

    private void AnchorPlayer(FileObject target) 
    {
        anchorCo = StartCoroutine(PlayerAnchorAnimation(target.playerAnchor.position, target.playerAnchor.rotation, 1.2f, 0.2f, player,false,false,false,null));
        OnStageFile?.Invoke(target.groundPositionInBliss);
    }

    IEnumerator PlayerAnchorAnimation(
        Vector3 targetPos, Quaternion targetRot, float speed, float dampSpeed,
        FirstPersonController player,
        bool zeroXZEuler,bool restorePlayerPos,bool cameraCenter,
        Action next) 
    {
        isAnchoring = true;
        float percent = 0;
        float progress = 0;
        Vector3 initialPos = player.transform.position;
        Vector3 camInitialLocalPos = player.playerCamera.transform.localPosition;
        Vector3 camInitialPos = player.playerCamera.transform.position;
        Quaternion initialCamRot = player.playerCamera.transform.localRotation;
        Quaternion initialPlayerRot = player.transform.localRotation;
        playerPositionBeforeAnchorAnimation = initialPos;

        player.playerCanMove = false;
        player.GetComponent<Rigidbody>().isKinematic = true;

        player.FreezeCamera();

        float distanceToTarget = Vector3.Distance(initialPos, targetPos);
        while (percent < 1 || (player.transform.position - targetPos).magnitude >= 0.02f) 
        {
            if (percent < 1) 
            {
               progress = AnchorAnimationCurve.Evaluate(percent);
            }
            Vector3 playerTargetPosition  = Vector3.Lerp(initialPos, targetPos, progress);
            player.transform.position = Vector3.SmoothDamp(player.transform.position, playerTargetPosition, ref playerPosRef, dampSpeed);
            Quaternion playerTargerRotation = Quaternion.Slerp(initialPlayerRot, targetRot, percent);
            player.transform.localRotation = Utility.SmoothDampQuaternion(player.transform.localRotation, playerTargerRotation, ref playerRotRef, dampSpeed, 100f, Time.deltaTime);
            Quaternion cameraTargetRotation = Quaternion.Slerp(initialCamRot, Quaternion.identity, percent);
            player.playerCamera.transform.localRotation = Utility.SmoothDampQuaternion(player.playerCamera.transform.localRotation, cameraTargetRotation, ref playerCamRotRef, dampSpeed, 100f, Time.deltaTime);
            if (cameraCenter) 
            {
                Vector3 cameraTargetPosition = Vector3.Lerp(camInitialPos, targetPos, progress);
                player.playerCamera.transform.position = Vector3.SmoothDamp(player.playerCamera.transform.position, cameraTargetPosition, ref playerCamPosRef, dampSpeed);
            }
            if (zeroXZEuler)
                player.transform.localEulerAngles = new Vector3(0, player.transform.localEulerAngles.y, 0);            
            percent += Time.deltaTime * speed;
            yield return null;
        }
        if (restorePlayerPos)
            player.transform.position = playerPositionBeforeAnchorAnimation;
        player.playerCamera.transform.localPosition = camInitialLocalPos;
        player.UnFreezeCamera(player.transform.localEulerAngles.y,0,zeroXZEuler);
        next?.Invoke();
  
    }
    private void DisablePlayerAnchor() 
    {
        player.playerCanMove = true;
        player.GetComponent<Rigidbody>().isKinematic = false;
        isAnchoring = false;
        if (anchorCo != null) 
        {
            StopCoroutine(anchorCo);
        }
       
        OnPlayerExitAnchor?.Invoke();
        StartCoroutine(ReturnPlayerToNormalXZRot());
    }

    IEnumerator ReturnPlayerToNormalXZRot() 
    {
        float percent = 0;
        Vector3 currentEuler = player.transform.eulerAngles;
        while (percent < 1) 
        {
            player.transform.rotation = Quaternion.Slerp(
                Quaternion.Euler(
                    currentEuler.x,
                    player.transform.eulerAngles.y,
                    currentEuler.z
                ),
                Quaternion.Euler(
                     0,
                    player.transform.eulerAngles.y,
                    0
                    ),
                percent

                );
                            
            percent += Time.deltaTime * 3f;
            yield return null;
        }
        player.zeroPlayerXZ = true;
    }

    void GetFileObject(FileObject file) 
    {
        currFile = file;
        if (currFile != prevFile && prevFile != null) 
        {
            OnSelectedFileChange?.Invoke(currFile,prevFile);
            
            //print("You have changed File");
        }
        prevFile = currFile;
        //print("You have got " + prevFile.name + "selected");
    }

    void SaveCurrentFile()
    {
        {
            if (!Array.Find(clippyFileLoaded,  x => x != null && x.name == prevFile.name + "(Clone)" ))
            {
               
                fileIndex = GetFirstNullIndexInList(clippyFileLoaded);
                if (fileIndex < clippyFileLoaded.Length) 
                {
                    FileObject f = Instantiate(currFile);
                    f.SwitchToClippyWorld();

                    f.transform.position = clippyFileLoadPosition[fileIndex].position;
                    f.transform.parent = clippyFileSystem.transform;
                    f.transform.forward = (clippyFileSystem.transform.position - f.transform.position).normalized;
                    f.transform.localScale *= 0.8f;
                    f.ResetIsAnchoredInClippy();
                    f.SetCloseButtonPosition(clippyFileSystem.transform);
                    
                    clippyFileLoaded[fileIndex] = f;
                    //print("You have Saved " + prevFile.name + "Congratulations!");

                }
            }
        }
    }

    void InitiateSaveAnimation(Vector3 targetPosition, Quaternion lookDirection) 
    {
        anchorCo = StartCoroutine(PlayerAnchorAnimation(currFile.transform.position + Vector3.up * 7,Quaternion.LookRotation(Vector3.down,Vector3.left),1.5f,0.2f,player,false,false,false,InitiateDiveAnimationFromFile));
    }

    void InitiateDiveAnimationFromFile() 
    {
        anchorCo = StartCoroutine(PlayerAnchorAnimation(currFile.transform.position - Vector3.up * 3, Quaternion.LookRotation(Vector3.down, Vector3.left), 1.5f,0.2f, player, false,false,false,SwitchSceneAndResetPlayer));
    }

    void InitiateDiveAnimation(Vector3 targetPosition, Quaternion lookDirection) 
    {
        anchorCo = StartCoroutine(PlayerAnchorAnimation(targetPosition, lookDirection, 0.6f, 0.02f, player, false, true, true,SwitchSceneAndResetPlayer));
    }
    void SwitchSceneAndResetPlayer() 
    {
        SwitchScene();
        DisablePlayerAnchor();
        
    }
    int GetFirstNullIndexInList<T>(T[] array)
    {
        foreach (T t in array) 
        {
            if (t == null)
                return Array.IndexOf(array, t);
        }
        return array.Count();
    }
    void RemoveFile(FileObject fileToRemove) 
    {
        for (int i = 0; i < clippyFileLoaded.Length; i++) {

            if (clippyFileLoaded[i] == fileToRemove) 
            {
                clippyFileLoaded[i] = null;
            }
        }
    }
    
    void Update()
    {
        SceneSwithcer();
    }

    void SwitchScene() 
    {
        if (!isInClippy)
        {
            isInClippy = true;
            previousBlissPosition = transform.position;
            transform.position = clippyLoadPoint.transform.position;
            localVolume.profile = clippyVolume;
            blizzWrapper.SetActive(false);
            clippyWrapper.SetActive(true);
            foreach (FileObject f in clippyFileLoaded)
            {
                if (f != null)
                    f.SetGroundPositioninClippy();
            }
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
    void SceneSwithcer() 
    {
        if (Input.GetKeyDown(KeyCode.F) && !isAnchoring)
        {
            SwitchScene();
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
