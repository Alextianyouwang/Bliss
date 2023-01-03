using System;
using UnityEngine;

// This class takes care of the procedural animation of player & camera movements in the Main Bliss Scene.
// All movements of player that is not conducted by the FPS controller is classified as an Anchoring Animation.
public class AM_BlissMain : AnchorAnimation
{
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

    // A general animation curver controlling all Anchoring animation, will introduce more in the future.

    public AnimationCurve slowFastCurve, fastSlowCurve;


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

    }
    // Unlock player from a Anchoring situation.
    void InitiateDisableAnchorAnimation()
    {
        playerAnimationCTS?.Cancel();
        playerZeroXZRotationCTS?.Cancel();
        ReturnPlayerToNormalXZRot();

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