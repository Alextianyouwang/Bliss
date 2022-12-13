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
    public static bool isAnchoring = false;

    public static Action<Vector3> OnStageFile;
    public static Action OnPlayerTeleportAnimationFinished;
    public static Action OnPlayerExitAnchor;
    public static Action<float, float> OnDiving;
    public static Action<float, float> OnPrepareDiving;
    public static Action<float, float> OnSoring;
    public static Action OnRequestSceneSwitch;
    public static Action<FirstPersonController,bool> OnRequestDive;
    public static Action<bool> OnToggleStagingDelete;

    public FirstPersonController player;
    public AnimationCurve AnchorAnimationCurve;

    private CancellationTokenSource
    playerAnimationCTS,
    playerZeroXZCTS;
    private Vector3 playerPositionBeforeLastAnchor;
    public enum playerAnimationState { none, anchoring, resetting, mostlyDone }
    public playerAnimationState animationState;

    private void OnEnable()
    {
        SaveButton.OnInitiateSaveAnimation += InitiateSaveAnimation;

        FileObject.OnPlayerAnchored += InitiateAnchorPlayerAnimation;
        FileObject.OnPlayerReleased += InitiateDisableAnchorAnimation;
        DeleteButton.OnPlayerReleased += InitiateDeleteAnchorAnimation;

        TileMatrixManager.OnInitiateDivingFromMatrix += InitiateDiveAnimation;
        TileMatrixManager.OnInitiateSoaringFromMatrix += InitiateSoarAnimation;

    }
    private void OnDisable()
    {
        SaveButton.OnInitiateSaveAnimation -= InitiateSaveAnimation;

        FileObject.OnPlayerAnchored -= InitiateAnchorPlayerAnimation;
        FileObject.OnPlayerReleased -= InitiateDisableAnchorAnimation;
        DeleteButton.OnPlayerReleased -= InitiateDeleteAnchorAnimation;


        TileMatrixManager.OnInitiateDivingFromMatrix -= InitiateDiveAnimation;
        TileMatrixManager.OnInitiateSoaringFromMatrix -= InitiateSoarAnimation;


        isAnchoring = false;
    }

    async void PlayerAnchorTask(
        Vector3 targetPos,Vector3 curveControlPointOffset, Quaternion targetRot, float speed, float posDampSpeed, float rotDampSpeed,
        FirstPersonController player,
        bool zeroXZRot, bool restorePlayerPos, bool cameraCenter, bool onlyRotateCamera,
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
        Quaternion camInitialRot = player.playerCamera.transform.rotation;
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
                float interpolate = timeProgress <= 1 ? AnchorAnimationCurve.Evaluate(timeProgress) : 1;
                distanceProgress = (player.transform.position - targetPos).magnitude / distanceToTarget;

                Vector3 playerTargetPosition = Utility.QuadraticBezier(playerInitialLocalPos, (targetPos - playerInitialLocalPos).magnitude / 2 * (targetPos - playerInitialLocalPos).normalized + playerInitialLocalPos + curveControlPointOffset, targetPos, interpolate);
                player.transform.position = Vector3.SmoothDamp(player.transform.position, playerTargetPosition, ref playerPosRef, posDampSpeed, 1000f);
                if (!onlyRotateCamera)
                {
                    Quaternion playerTargerRotation = Quaternion.Slerp(playerInitialLocalRot, targetRot, timeProgress);
                    player.transform.localRotation = Utility.SmoothDampQuaternion(player.transform.localRotation, playerTargerRotation, ref playerRotRef, rotDampSpeed, 100f, Time.deltaTime);
                    Quaternion cameraTargetRotation = Quaternion.Slerp(camInitialLocalRot, Quaternion.identity, timeProgress);
                    player.playerCamera.transform.localRotation = Utility.SmoothDampQuaternion(player.playerCamera.transform.localRotation, cameraTargetRotation, ref playerCamRotRef, rotDampSpeed, 100f, Time.deltaTime);
                }
                else
                {
                    Quaternion playerTargerRotation = Quaternion.Slerp(playerInitialLocalRot, Quaternion.identity, timeProgress);
                    player.transform.localRotation = Utility.SmoothDampQuaternion(player.transform.localRotation, playerTargerRotation, ref playerRotRef, rotDampSpeed, 100f, Time.deltaTime);
                    Quaternion cameraTargetRotation = Quaternion.Slerp(camInitialRot, targetRot, timeProgress);
                    player.playerCamera.transform.localRotation = Utility.SmoothDampQuaternion(player.playerCamera.transform.localRotation, cameraTargetRotation, ref playerCamRotRef, rotDampSpeed, 100f, Time.deltaTime);
                }
                if (cameraCenter)
                {
                    Vector3 cameraTargetPosition = Vector3.Lerp(camInitialPos, targetPos, interpolate);
                    player.playerCamera.transform.position = Vector3.SmoothDamp(player.playerCamera.transform.position, cameraTargetPosition, ref playerCamPosRef, posDampSpeed);
                }

                timeProgress += Time.deltaTime * speed;
                during?.Invoke(timeProgress, distanceProgress);
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
        playerAnimationCTS?.Cancel();
        playerZeroXZCTS?.Cancel();
        PlayerAnchorTask(target.playerAnchor.position, Vector3.up * 4f, target.playerAnchor.rotation, 1.5f, 0.1f, 0.1f, player, false, false, false,false, null, null);
        OnStageFile?.Invoke(target.groundPosition);
    }

    void InitiateDisableAnchorAnimation()
    {

        playerAnimationCTS?.Cancel();
        playerZeroXZCTS?.Cancel();
        OnPlayerExitAnchor?.Invoke();
        ReturnPlayerToNormalXZRot();
        EndAnchor();
    }

    void InitiateDeleteAnchorAnimation() 
    {
        StartCoroutine(WaitMatrixDeleteAnimation());
    }
    IEnumerator WaitMatrixDeleteAnimation() 
    {
        OnToggleStagingDelete?.Invoke(true);
        yield return new WaitForSeconds(0.8f);
        OnToggleStagingDelete?.Invoke(false);
        yield return new WaitForSeconds(0.2f);
        InitiateDisableAnchorAnimation();
    }
   
    void EndAnchor()
    {
        player.playerCanMove = true;
        player.GetComponent<Rigidbody>().isKinematic = false;
        isAnchoring = false;
    }

    void InitiateSaveAnimation(Vector3 targetPosition, Quaternion lookDirection)
    {
        playerAnimationCTS?.Cancel();
        playerZeroXZCTS?.Cancel();
        PlayerAnchorTask(SceneSwitcher.sd.currFile.groundPosition + Vector3.up * 4f, Vector3.up * 25f, Quaternion.LookRotation(Vector3.down, transform.right), 0.7f, 0.01f, 0.2f, player, false, false, false,true, RequestDive_passive, DuringPrepareDiving);
       
    }
    void DuringPrepareDiving(float timePercent, float distancePercent) 
    {
        OnPrepareDiving?.Invoke(timePercent,distancePercent);
    }

    void RequestDive_passive()
    {
        OnRequestDive?.Invoke(GetComponent<FirstPersonController>(),false);
    }
    void InitiateDiveAnimation(Vector3 targetPosition, Quaternion lookDirection)
    {
        playerAnimationCTS?.Cancel();
        playerZeroXZCTS?.Cancel();
        PlayerAnchorTask(targetPosition, Vector3.zero, lookDirection, 0.3f, 0.02f, 0.7f, player, false, true, true,false, SwitchSceneAndResetPlayer, DuringDiving);
    }

    void InitiateSoarAnimation(Vector3 targetPosition, Quaternion lookDirection)
    {
        playerAnimationCTS?.Cancel();
        playerZeroXZCTS?.Cancel();
        PlayerAnchorTask(targetPosition, Vector3.zero, lookDirection, 0.25f, 0.02f, 0.99f, player, false, true, true,false, SwitchSceneAndResetPlayer, DuringSoring);
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
        OnRequestSceneSwitch?.Invoke();
        OnPlayerTeleportAnimationFinished?.Invoke();
        playerAnimationCTS?.Cancel();
        playerZeroXZCTS?.Cancel();
        ReturnPlayerToNormalXZRot();
        player.playerCanMove = true;
        player.GetComponent<Rigidbody>().isKinematic = false;
        isAnchoring = false;
    }
}