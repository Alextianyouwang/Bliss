using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

// This class takes care of the procedural animation of player & camera movements.
// All movements of player that is not conducted by the FPS controller is classified as an Anchoring Animation.
public class PlayerAnchorAnimation : MonoBehaviour
{
    public static bool isAnchoring = false, isInTeleporting = false;

    // Invoked when Player starts to perform an anchoring animation to a File.
    public static Action<Vector3> OnPlayerStartAnchor;
    // Invoked when Player are set free from an anchoring animation.
    public static Action OnPlayerExitAnchor;
    // Invoked when Player just finished its teleporting anchoring animation.
    public static Action OnPlayerTeleportAnimationFinished;

    // Invoked while Player is in Leaping into file, Diving to Floppy and Soaring to Bliss anchoring animation respectively.
    public static Action<float, float> OnPrepareDiving;
    public static Action<float, float> OnDiving;
    public static Action<float, float> OnSoring;

    // Invoke to request the TileMatrixManager class to perform the dive animation.
    public static Action<FirstPersonController,bool> OnRequestDive;
    // Invoke to request SceneSwith. Same as pressing F.
    public static Action OnRequestSceneSwitch;

    [SerializeField]private FirstPersonController player;
    // A general animation curver controlling all Anchoring animation, will introduce more in the future.
    public AnimationCurve slowFastCurve,fastSlowCurve;

    // Each type of Anchoring Animation has its own cancellation token. Call the cancel methord on these to stop an animation.
    private CancellationTokenSource
    playerAnimationCTS,
    playerZeroXZRotationCTS;
    private Vector3 playerPositionBeforeLastAnchor;
    public enum playerAnimationState { none, anchoring, resetting }
    [HideInInspector] public playerAnimationState animationState;

    private void OnEnable()
    {
        SaveButton.OnInitiateSaveAnimation += InitiateSaveAnimation;

        FileObject.OnPlayerAnchored += InitiateAnchorPlayerAnimation;
        FileObject.OnPlayerReleased += InitiateDisableAnchorAnimation;

        TileMatrixManager.OnInitiateDivingFromMatrix += InitiateDiveAnimation;
        TileMatrixManager.OnInitiateSoaringFromMatrix += InitiateSoarAnimation;
        TileMatrixManager.OnFinishingDeleteFileAnimation += InitiateDisableAnchorAnimation;
    }
    private void OnDisable()
    {
        SaveButton.OnInitiateSaveAnimation -= InitiateSaveAnimation;

        FileObject.OnPlayerAnchored -= InitiateAnchorPlayerAnimation;
        FileObject.OnPlayerReleased -= InitiateDisableAnchorAnimation;

        TileMatrixManager.OnInitiateDivingFromMatrix -= InitiateDiveAnimation;
        TileMatrixManager.OnInitiateSoaringFromMatrix -= InitiateSoarAnimation;
        TileMatrixManager.OnFinishingDeleteFileAnimation -= InitiateDisableAnchorAnimation;

        isAnchoring = false;
    }

    async void PlayerAnchorTask(
        Vector3 targetPos,Vector3 curveControlPointOffset, Quaternion targetRot, float speed, float posDampSpeed, float rotDampSpeed,
        FirstPersonController player,
        AnimationCurve curve,
        bool zeroXZRot, bool restorePlayerPos, bool cameraCenter, bool onlyRotateCamera, bool teleporting,
        Action next, Action<float, float> during)
    {
        animationState = playerAnimationState.anchoring;
        Vector3 playerPosRef = Vector3.zero, playerCamRotRef = Vector3.zero, playerRotRef = Vector3.zero, playerCamPosRef = Vector3.zero;
        playerAnimationCTS = new CancellationTokenSource();
        CancellationToken ct = playerAnimationCTS.Token;

        float timeProgress = 0;
        float distanceProgress = 0;

        Vector3 camInitialLocalPos = player.playerCamera.transform.localPosition;
        Vector3 camInitialPos = player.playerCamera.transform.position;
        Quaternion camInitialLocalRot = player.playerCamera.transform.localRotation;
        Quaternion camInitialRot = player.playerCamera.transform.rotation;
        Vector3 playerInitialLocalPos = player.transform.position;
        Quaternion playerInitialLocalRot = player.transform.localRotation;

        playerPositionBeforeLastAnchor = playerInitialLocalPos;
        player.playerCanMove = false;
        player.GetComponent<Rigidbody>().isKinematic = true;
        player.FreezeCamera();
        float distanceToTarget = Vector3.Distance(playerInitialLocalPos, targetPos);

        isAnchoring = true;
        isInTeleporting = teleporting;
        try
        {
            while (timeProgress < 1 || (player.transform.position - targetPos).magnitude >= 0.02f)
            {
                float interpolate = timeProgress <= 1 ? curve.Evaluate(timeProgress) : 1;
                distanceProgress = (player.transform.position - targetPos).magnitude / distanceToTarget;
                timeProgress += Time.deltaTime * speed;
                during?.Invoke(timeProgress, distanceProgress);

                Vector3 playerTargetPosition = Utility.QuadraticBezier(playerInitialLocalPos, (targetPos + playerInitialLocalPos) / 2 + curveControlPointOffset, targetPos, interpolate);
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

                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            if (restorePlayerPos)
                player.transform.position = playerPositionBeforeLastAnchor;
            if (cameraCenter)
                player.playerCamera.transform.localPosition = camInitialLocalPos;
            // make sure the camera is remain Locked if the anchoring animation is cancelled. Since the next animation will be injected the next frame, this practice will prevent glitch.
            if (!ct.IsCancellationRequested)
                player.UnFreezeCamera(player.transform.localEulerAngles.y, 0, zeroXZRot);
            animationState = playerAnimationState.none;
            isInTeleporting = false;
            next?.Invoke();
        }
    }
    async void ReturnPlayerToNormalXZRot()
    {
        playerZeroXZRotationCTS = new CancellationTokenSource();
        CancellationToken ct = playerZeroXZRotationCTS.Token;
        float percent = 0;
        Vector3 currentEuler = player.transform.eulerAngles;
        Vector3 rotRef = Vector3.zero;
        animationState = playerAnimationState.resetting;
        float pitch = player.playerCamera.transform.localEulerAngles.x;
        // this will ensure the camera is Unlocked and its eular angle value will be continuous to that stored in Player to give a smooth unlock.
        // the pitch of camera get from Unity is in range of "-270 to -359" and "0 to 90". So need a bit of conversion.
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
                    ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            animationState = playerAnimationState.none;
            player.zeroPlayerXZ = true;
        }
    }
    void KillAnchor()
    {
        player.playerCanMove = true;
        player.GetComponent<Rigidbody>().isKinematic = false;
        isAnchoring = false;
    }
    // Lock player to a certain File Anchor.
    void InitiateAnchorPlayerAnimation(FileObject target)
    {
        playerAnimationCTS?.Cancel();
        playerZeroXZRotationCTS?.Cancel();
        PlayerAnchorTask(target.playerAnchor.position, Vector3.up * 4f, target.playerAnchor.rotation, 1.5f, 0.1f, 0.1f, player,slowFastCurve, false, false, false,false, false, null, null);

        OnPlayerStartAnchor?.Invoke(target.groundPosition);
    }
    // Unlock player from a Anchoring situation.
    void InitiateDisableAnchorAnimation()
    {
        playerAnimationCTS?.Cancel();
        playerZeroXZRotationCTS?.Cancel();
        ReturnPlayerToNormalXZRot();

        OnPlayerExitAnchor?.Invoke();
        KillAnchor();
    }
    // Player will leap into the ground position of the currently selected file. It will followed by the dive anchoring animation.
    void InitiateSaveAnimation()
    {
        playerAnimationCTS?.Cancel();
        playerZeroXZRotationCTS?.Cancel();
        PlayerAnchorTask(SceneSwitcher.sd.currFile.groundPosition + Vector3.up * 4f, Vector3.up * 25f, Quaternion.LookRotation(Vector3.down, transform.right), 0.6f, 0.01f, 0.2f, player, fastSlowCurve, false, false, false,true,true, RequestDive_passive, DuringPrepareDiving);
    }
    // Dive animation, will follow by scene swith and reset.
    void InitiateDiveAnimation(Vector3 targetPosition, Quaternion lookDirection)
    {
        playerAnimationCTS?.Cancel();
        playerZeroXZRotationCTS?.Cancel();
        PlayerAnchorTask(targetPosition, Vector3.zero, lookDirection, 0.3f, 0.02f, 0.7f, player, slowFastCurve, false, true, true, false,true, SwitchSceneAndResetPlayer, DuringDiving);
    }
    // Soar animation, will follow by scene swith and reset.
    void InitiateSoarAnimation(Vector3 targetPosition, Quaternion lookDirection)
    {
        playerAnimationCTS?.Cancel();
        playerZeroXZRotationCTS?.Cancel();
        PlayerAnchorTask(targetPosition, Vector3.zero, lookDirection, 0.25f, 0.02f, 0.99f, player, slowFastCurve, false, true, true, false,true, SwitchSceneAndResetPlayer, DuringSoring);
    }
    // Notify the TileMatrixManager to perform the animation simutaneously with the Player.
    void RequestDive_passive()
    {
        OnRequestDive?.Invoke(GetComponent<FirstPersonController>(), false);
    }
    void DuringPrepareDiving(float timePercent, float distancePercent) 
    {
        OnPrepareDiving?.Invoke(timePercent,distancePercent);
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
        InitiateDisableAnchorAnimation();
    }
}