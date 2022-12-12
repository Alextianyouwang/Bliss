using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;


public class PlayerAnchorAnimation : MonoBehaviour
{
    // ClippyFileSyatem
   /* private Vector3 previousBlissPosition;
    private GameObject blizzWrapper, clippyWrapper, clippyLoadPoint;

    private FileObject prevFile, currFile;
    private ClippyFileSystem clippyFileSystem;
    private List<Transform> clippyFileLoadPosition;
    private FileObject[] clippyFileLoaded;
    private int fileIndex = 0;

    public static bool isInClippy = false;

    public static Action<bool> OnClippyToggle;

    public static Action<FileObject, FileObject> OnSelectedFileChange;*/

    public static bool isAnchoring = false;

    public static Action<Vector3> OnStageFile;
    public static Action OnPlayerTeleportAnimationFinished;
    public static Action OnPlayerExitAnchor;
    public static Action<float, float> OnDiving;
    public static Action<float, float> OnSoring;
    public static Action<FirstPersonController> OnStartDiving;
    public static Action OnRequestSceneSwitch;

    public FirstPersonController player;
    public AnimationCurve AnchorAnimationCurve;

    private CancellationTokenSource
    playerAnimationCTS,
    playerZeroXZCTS;
    private Vector3 playerPositionBeforeLastAnchor;
    float pitchDifference;
    public enum playerAnimationState { none, anchoring, resetting, mostlyDone }
    public playerAnimationState animationState;

    private void OnEnable()
    {
       /* FileObject.OnFlieCollected += GetFileObject;
        SaveButton.OnSaveCurrentFile += SaveCurrentFile;
        DeleteButton.OnDeleteObject += DeleteCurrentFile;*/

        SaveButton.OnInitiateSaveAnimation += InitiateSaveAnimation;

        FileObject.OnPlayerAnchored += InitiateAnchorPlayerAnimation;
        FileObject.OnPlayerReleased += InitiateDisableAnchorAnimation;
        DeleteButton.OnPlayerReleased += InitiateDisableAnchorAnimation;

        TileMatrixManager.OnInitiateDivingFromMatrix += InitiateDiveAnimation;
        TileMatrixManager.OnInitiateSoaringFromMatrix += InitiateSoarAnimation;

    }
    private void OnDisable()
    {
    /*    FileObject.OnFlieCollected -= GetFileObject;
        SaveButton.OnSaveCurrentFile -= SaveCurrentFile;
        DeleteButton.OnDeleteObject -= DeleteCurrentFile;
*/
        SaveButton.OnInitiateSaveAnimation -= InitiateSaveAnimation;

        FileObject.OnPlayerAnchored -= InitiateAnchorPlayerAnimation;
        FileObject.OnPlayerReleased -= InitiateDisableAnchorAnimation;
        DeleteButton.OnPlayerReleased -= InitiateDisableAnchorAnimation;


        TileMatrixManager.OnInitiateDivingFromMatrix -= InitiateDiveAnimation;
        TileMatrixManager.OnInitiateSoaringFromMatrix -= InitiateSoarAnimation;


        isAnchoring = false;


    }

    async void PlayerAnchorTask(
        Vector3 targetPos, Quaternion targetRot, float speed, float posDampSpeed, float rotDampSpeed,
        FirstPersonController player,
        bool zeroXZRot, bool restorePlayerPos, bool cameraCenter,
        Action next, Action<float, float> during)
    {
        animationState = playerAnimationState.anchoring;
        Vector3 playerPosRef = Vector3.zero, playerCamRotRef = Vector3.zero, playerRotRef = Vector3.zero, playerCamPosRef = Vector3.zero;
        playerAnimationCTS = new CancellationTokenSource();
        CancellationToken ct = playerAnimationCTS.Token;

        float timeProgress = 0;
        float distanceProgress = 0;
        Vector3 camInitialLocalPos = player.playerCamera.transform.localPosition;
        Vector3 playerInitialLocalPos = player.transform.position;
        Vector3 camInitialPos = player.playerCamera.transform.position;
        Quaternion camInitialLocalRot = player.playerCamera.transform.localRotation;
        Quaternion playerInitialLocalRot = player.transform.localRotation;
        float camInitialPitch = player.playerCamera.transform.localEulerAngles.x;
        playerPositionBeforeLastAnchor = playerInitialLocalPos;
        player.playerCanMove = false;
        player.GetComponent<Rigidbody>().isKinematic = true;
        player.FreezeCamera();
        float distanceToTarget = Vector3.Distance(playerInitialLocalPos, targetPos);

        isAnchoring = true;
        try
        {
            while (timeProgress < 1 || (player.transform.position - targetPos).magnitude >= 0.02f)
            {
                if (timeProgress < 1)
                    distanceProgress = AnchorAnimationCurve.Evaluate(timeProgress);

                    //animationState = playerAnimationState.mostlyDone;
                Vector3 playerTargetPosition = Vector3.Lerp(playerInitialLocalPos, targetPos, distanceProgress);
                player.transform.position = Vector3.SmoothDamp(player.transform.position, playerTargetPosition, ref playerPosRef, posDampSpeed, 100f);
                Quaternion playerTargerRotation = Quaternion.Slerp(playerInitialLocalRot, targetRot, timeProgress);
                player.transform.localRotation = Utility.SmoothDampQuaternion(player.transform.localRotation, playerTargerRotation, ref playerRotRef, rotDampSpeed, 100f, Time.deltaTime);
                Quaternion cameraTargetRotation = Quaternion.Slerp(camInitialLocalRot, Quaternion.identity, timeProgress);
                player.playerCamera.transform.localRotation = Utility.SmoothDampQuaternion(player.playerCamera.transform.localRotation, cameraTargetRotation, ref playerCamRotRef, rotDampSpeed, 100f, Time.deltaTime);
                pitchDifference = player.playerCamera.transform.localEulerAngles.x;
                if (cameraCenter)
                {
                    Vector3 cameraTargetPosition = Vector3.Lerp(camInitialPos, targetPos, distanceProgress);
                    player.playerCamera.transform.position = Vector3.SmoothDamp(player.playerCamera.transform.position, cameraTargetPosition, ref playerCamPosRef, posDampSpeed);
                }

                timeProgress += Time.deltaTime * speed;
                during?.Invoke(timeProgress, (player.transform.position - targetPos).magnitude / distanceToTarget);
                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }
        }
        catch (OperationCanceledException)
        {
            
        }
        finally
        {
            if (restorePlayerPos)
                player.transform.position = playerPositionBeforeLastAnchor;
            if (cameraCenter)
                player.playerCamera.transform.localPosition = camInitialLocalPos;
            if (!ct.IsCancellationRequested)
                player.UnFreezeCamera(player.transform.localEulerAngles.y, 0, zeroXZRot);
            animationState = playerAnimationState.none;

            next?.Invoke();
        }
    }
    async void ReturnPlayerToNormalXZRot()
    {
        playerZeroXZCTS = new CancellationTokenSource();
        CancellationToken ct = playerZeroXZCTS.Token;
        float percent = 0;
        Vector3 currentEuler = player.transform.eulerAngles;
        Vector3 rotRef = Vector3.zero;
        animationState = playerAnimationState.resetting;
        float pitch = player.playerCamera.transform.localEulerAngles.x;
        player.UnFreezeCamera(player.transform.localEulerAngles.y, pitch > 180 ? pitch - 360 : pitch, false);

        try
        {
            while (percent < 1 || Mathf.Abs(player.transform.eulerAngles.x) + Mathf.Abs(player.transform.eulerAngles.z) > 0.01f)
            {
                percent += Time.deltaTime * 3;
                Quaternion target = Quaternion.Slerp(
                    Quaternion.Euler(currentEuler.x, player.transform.eulerAngles.y, currentEuler.z),
                    Quaternion.Euler(0, player.transform.eulerAngles.y, 0),
                    percent
                    );
                player.transform.rotation = Utility.SmoothDampQuaternion(player.transform.rotation, target, ref rotRef, 0.1f, 100, Time.deltaTime);


                if (ct.IsCancellationRequested)
                {
                    ct.ThrowIfCancellationRequested();
                }
                await Task.Yield();
            }
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            animationState = playerAnimationState.none;

            player.zeroPlayerXZ = true;
        }
    }



    void InitiateAnchorPlayerAnimation(FileObject target)
    {
        //if (animationState == playerAnimationState.none)
        {
            playerAnimationCTS?.Cancel();
            playerZeroXZCTS?.Cancel();
            PlayerAnchorTask(target.playerAnchor.position, target.playerAnchor.rotation, 1.5f, 0.1f, 0.1f, player, false, false, false, null, null);
            OnStageFile?.Invoke(target.groundPosition);
        }

    }

    void InitiateDisableAnchorAnimation()
    {
        //if (animationState == playerAnimationState.none)
        {
            playerAnimationCTS?.Cancel();
            playerZeroXZCTS?.Cancel();
            OnPlayerExitAnchor?.Invoke();
            ReturnPlayerToNormalXZRot();
            EndAnchor();
        }
    }

    void EndAnchor()
    {
        player.playerCanMove = true;
        player.GetComponent<Rigidbody>().isKinematic = false;
        isAnchoring = false;
    }

    void InitiateSaveAnimation(Vector3 targetPosition, Quaternion lookDirection)
    {
        PlayerAnchorTask(FirstPersonController.playerGroundPosition + Vector3.up * 12, Quaternion.LookRotation(Vector3.down, Vector3.left), 1.5f, 0.2f, 0.2f, player, false, false, false,InitiateDiveAnimationFromFile, null);
    }

    void InitiateDiveAnimationFromFile()
    {
        PlayerAnchorTask(FirstPersonController.playerGroundPosition + Vector3.up *3f, Quaternion.LookRotation(Vector3.down, Vector3.left), 2f, 0.01f, 0.01f, player, false, false, false,SwitchSceneAndResetPlayer, null);
    }
    void InitiateDiveAnimation(Vector3 targetPosition, Quaternion lookDirection)
    {
        PlayerAnchorTask(targetPosition, lookDirection, 0.3f, 0.02f, 0.7f, player, false, true, true, SwitchSceneAndResetPlayer, DuringDiving);
    }

    void InitiateSoarAnimation(Vector3 targetPosition, Quaternion lookDirection)
    {
        PlayerAnchorTask(targetPosition, lookDirection, 0.25f, 0.02f, 0.99f, player, false, true, true, SwitchSceneAndResetPlayer, DuringSoring);
    }

    void DuringDiving(float timePercent, float distancePercent)
    {
        OnDiving?.Invoke(timePercent, distancePercent);
    }
    void DuringSoring(float timePercent, float distancePercent)
    {
        OnSoring?.Invoke(timePercent, distancePercent);
    }

    void SwitchSceneAndResetPlayer()
    {
        //SwitchScene();
        OnRequestSceneSwitch?.Invoke();
        OnPlayerTeleportAnimationFinished?.Invoke();
        playerAnimationCTS?.Cancel();
        ReturnPlayerToNormalXZRot();
        player.playerCanMove = true;
        player.GetComponent<Rigidbody>().isKinematic = false;
        isAnchoring = false;
    }

   /* #region FileSavingLogic
    void SaveCurrentFile()
    {
        if (!Array.Find(clippyFileLoaded, x => x != null && x.name == prevFile.name + "(Clone)"))
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
                f.isAnchored = false;
                clippyFileLoaded[fileIndex] = f;
            }
        }
    }
    void DeleteCurrentFile()
    {
        RemoveFile(currFile);
        Destroy(currFile.gameObject);
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
        for (int i = 0; i < clippyFileLoaded.Length; i++)
        {

            if (clippyFileLoaded[i] == fileToRemove)
            {
                clippyFileLoaded[i] = null;
            }
        }
    }
    void GetFileObject(FileObject file)
    {
        currFile = file;
        if (currFile != prevFile && prevFile != null)
        {
            OnSelectedFileChange?.Invoke(currFile, prevFile);
        }
        prevFile = currFile;
    }
    #endregion

    #region SceneSwitchingLogic
    void Start()
    {
        player = GetComponent<FirstPersonController>();
        StartCoroutine(WaitUntilSceneLoad());
    }
    void Update()
    {
        SceneSwithcer();
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
    void SwitchScene()
    {

        if (!isInClippy)
        {
            isInClippy = true;
            previousBlissPosition = transform.position;
            transform.position = clippyLoadPoint.transform.position;

            blizzWrapper.SetActive(false);
            clippyWrapper.SetActive(true);
   
        }

        else
        {
            isInClippy = false;
            clippyLoadPoint.transform.position = transform.position;

            transform.position = previousBlissPosition;
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
    #endregion*/
}
