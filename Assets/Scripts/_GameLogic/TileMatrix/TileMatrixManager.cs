using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class TileMatrixManager : MonoBehaviour
{
    // Changing: The value does not has a default, it start with 0 and will be assigned later depends on needs.
    // Varying: The value will be set to a default at first, it will move between other values and the defalut.
    // Default: The value will not change.

    [SerializeField] private GameObject tile;
    [SerializeField] private SaveButton saveButton;
    [SerializeField] private DeleteButton deleteButton;
    [SerializeField] private int defaultTileDimension = 7;
    [SerializeField] private float defaultRadius = 15;
    [SerializeField] private bool isEnabled = true;



    private TileDrawInstance t;
    private TileButtons b;

    private Coroutine fileStagingCo;

    // Test
    private GameObject follower;
    Vector3 velocity;
    Vector3 dampPosition;

    private float
        changingHighRiseMultiplierBoost,
        changingRadius,
        changingMatrixYOffset,
        varyingDampSpeed,
        defaultDampSpeed = 0.08f,
        defaultNoiseWeight = 0.3f;

    private bool
        isInDiveFormation = false,
        hasTriggeredLandingGathering = false,
        hasWindowsDetached = false;


    private enum TileStates { NormalFollow, Popup, Staging, Landing, PrepareLanding, Staging_Diving, Staging_Deleting }
    private TileStates state;

    public static Action<Vector3, Quaternion> OnInitiateDivingFromMatrix;
    public static Action<Vector3, Quaternion> OnInitiateSoaringFromMatrix;
    public static Action OnFinishingDeleteFileAnimation;



    private void OnEnable()
    {
        FirstPersonController.OnPitchChange += ChangeRadius;
        FirstPersonController.OnIncreaseDownAnimationTime += ReceiveDownAnimationGlobalPositionOffset;
        FirstPersonController.OnIncreaseUpAnimationTime += ReceiveUpAnimationGlobalPositionOffset;
        FirstPersonController.OnExitThreshold += ExitThreshold;
        FirstPersonController.OnStartDiving += StartDivingAnimation;
        FirstPersonController.OnStartSoaring += StartSoaringAnimation;

        AM_BlissMain.OnPlayerTeleportAnimationFinished += ResetToDefault;
        AM_BlissMain.OnDiving += DivingAnimation;
        AM_BlissMain.OnSoring += SoaringAnimation;
        AM_BlissMain.OnRequestDive += StartDivingAnimation;
        AM_BlissMain.OnRequestDive += SwitchToStageDiving_fromPlayerAnchroAnimation;
        AM_BlissMain.OnPrepareDiving += ReceiveDownAnimationGlobalPositionOffset_fromPlayerAnchorAnimation;

        FileManager.OnTriggerSaveMatrix += StartStagingFile;
        FileManager.OnFileChange += EndStagingFile_fromFileManager;
        FileObject.OnPlayerReleased += EndStagingFile;

        SaveButton.OnRetreatSaveButton += InitiateRetreatAndResetWindowsAnimation;
        DeleteButton.OnPlayerReleased += InitiateDeleteAnchorAnimation;

        GemCollectionPlat.OnFileUnlockMatrixPopup += InitiatePopupAnimation;

        PostAndScenery.OnGettingUndergroundTileRadius += GetUnderGroundTilesProxiRadius;
    }
    private void OnDisable()
    {
        FirstPersonController.OnPitchChange -= ChangeRadius;
        FirstPersonController.OnIncreaseDownAnimationTime -= ReceiveDownAnimationGlobalPositionOffset;
        FirstPersonController.OnIncreaseUpAnimationTime -= ReceiveUpAnimationGlobalPositionOffset;
        FirstPersonController.OnExitThreshold -= ExitThreshold;
        FirstPersonController.OnStartDiving -= StartDivingAnimation;
        FirstPersonController.OnStartSoaring -= StartSoaringAnimation;

        AM_BlissMain.OnPlayerTeleportAnimationFinished -= ResetToDefault;
        AM_BlissMain.OnDiving -= DivingAnimation;
        AM_BlissMain.OnSoring -= SoaringAnimation;
        AM_BlissMain.OnRequestDive -= StartDivingAnimation;
        AM_BlissMain.OnRequestDive -= SwitchToStageDiving_fromPlayerAnchroAnimation;
        AM_BlissMain.OnPrepareDiving -= ReceiveDownAnimationGlobalPositionOffset_fromPlayerAnchorAnimation;

        FileManager.OnTriggerSaveMatrix -= StartStagingFile;
        FileManager.OnFileChange -= EndStagingFile_fromFileManager;
        FileObject.OnPlayerReleased -= EndStagingFile;

        SaveButton.OnRetreatSaveButton -= InitiateRetreatAndResetWindowsAnimation;
        DeleteButton.OnPlayerReleased -= InitiateDeleteAnchorAnimation;

        GemCollectionPlat.OnFileUnlockMatrixPopup -= InitiatePopupAnimation;

        PostAndScenery.OnGettingUndergroundTileRadius -= GetUnderGroundTilesProxiRadius;

    }

    private void Awake()
    {
        t = new TileDrawInstance(tile, defaultTileDimension);
        b = new TileButtons(t, saveButton, deleteButton);
    }
    void Start()
    {
        follower = GameObject.Find("GemPlatform");
        if (!follower)
            Debug.LogError("GemPlatform not found in scene");
        if (follower)
            dampPosition = follower.transform.position;

        t.Initialize();
        varyingDampSpeed = defaultDampSpeed;
        state = TileStates.NormalFollow;
        b.UpdateButtonPosition( TileButtons.ButtonTile.DisplayState.off);
    }


    /*
        The main logic of this procedural Tile Animation system is driven by State and Step.
        There are two main states. The tile main code is running in either Update or in the FileStagingAnimation Coroutine, never in both.
        In Update, the Matrix always follow player position, and there are three substates: NormalFollow, Landing, and PrepareLanding.
        In the Coroutine, the Matrix will follow an animated center, and there are other three substates: Staging, Staging_Diving, Staging_Delete. More states could be added if needed.
        The difference between states are achieved by using modular steps to accumulate the result.
        For example a state could be consists of:

            UpdateTileSetActive()           ********* Only check the activation and deactivation of a tile, base on a position and a radius. Runs only when condition met.
            UpdateTileOrderedCoordinate()   ********* Assign each tile to an ordered Vector2 ID, the center is (0,0). Runs Every Frame.
            ResetWindowsTilePrefab()        ********* Reset all the tile prefabs to the default tile Prefab. Runs only when condition met.
            UpdateTileDampSpeed()           ********* Flexible block that can tweak the smoothdamp speed of tiles procedually. Runs Every Frame. 
            UpdateWindowTile()              ********* Identify the center 2*2 tiles as window tile. Runs Every Frame.
            UpdateWindowTilePrefab()        ********* Swap the prefab of the window tile to Save button or Delete button. Runs only when condition met.
            UpdateTileStatusPerFrame()      ********* Update the transform position of every single tile. Runs Every Frame.
        
        Depends on need, the pipeline could be customised to achieve certain effects with a minimal allocation of resources.
        Here are some ways to customise animation:

        1).The code blocks mentioned above will be constantly refreshed accross frames. Changing their parameters will cause the Matrix animated.
        For example, tweak

            changingHighRiseMultiplierBoost, 
            changingMatrixYOffset,
            changingRadius

        2).Since each states can behave drastically different, one way to create animations is to change the states in another Coroutine.state = TileStates.Staging_Deleting:

            state = TileStates.Staging_Deleting;
            yield return new WaitForSeconds(0.8f);
            state = TileStates.Staging;
            yield return new WaitForSeconds(0.2f);
          

        3). Lastly, some global variables in the TileMatrixFunctions could be tweaked as well to achieve some unique effects.
        For now, it can furthurely animate window tiles. Those parameters are:
            
            activateWindowsIndependance;
            changingWindowsYPos;
     */

    void Update()
    {
        if (!isEnabled)
            return;
        Vector3 playerGroundPosition = FirstPersonController.playerGroundPosition;
        b.UpdateButtonPosition(TileButtons.ButtonTile.DisplayState.off);

        switch (state)
        {
            case TileStates.NormalFollow:
                t.varyingNoiseTime = Time.time/3f;
                t.UpdateTileSetActive(playerGroundPosition, changingRadius);
                t.UpdateTileOrderedCoordinate(playerGroundPosition);
                t.UpdateTileDampSpeedTogether(varyingDampSpeed);
                t.UpdateWindowTile(playerGroundPosition);
                t.UpdateTilesStatusPerFrame(0, defaultRadius, changingRadius / 2 + changingHighRiseMultiplierBoost, changingMatrixYOffset +0.4f, defaultNoiseWeight, playerGroundPosition);
                t.DrawTileInstanceCurrentFrame(false);
                t.UpdateClosestTileToScreenCenter();
                if (t.closestTileToCenter != null && follower && !SceneSwitcher.isInFloppy) 
                {
                    dampPosition = Vector3.SmoothDamp(dampPosition, t.closestTileToCenter.smoothedFinalXYZPosition + Vector3.up * 0.8f, ref velocity, 0.1f);
                }
                if(follower)
                    follower.transform.position = dampPosition;


                break;

            case TileStates.Popup:
                t.varyingNoiseTime = Time.time / 3f;
                Vector3 adjustedPosition = Vector3.ProjectOnPlane(InteractionManager.camRay.direction, Vector3.up).normalized * 2.5f + playerGroundPosition;
                t.UpdateTileSetActive(adjustedPosition, 3f);
                t.UpdateTileOrderedCoordinate(adjustedPosition);
                t.UpdateTileDampSpeedTogether(0.15f);
                t.UpdateWindowTile(adjustedPosition);
                t.UpdateTilesStatusPerFrame(0, 3f, 0.8f, 0.3f, defaultNoiseWeight, adjustedPosition);
                t.DrawTileInstanceCurrentFrame(false);
                t.UpdateClosestTileToScreenCenter();
                if (t.closestTileToCenter != null && follower && !SceneSwitcher.isInFloppy)
                {
                    dampPosition = Vector3.SmoothDamp(dampPosition, t.closestTileToCenter.smoothedFinalXYZPosition + Vector3.up * 0.8f, ref velocity, 0.1f);
                }
                if (follower)
                    follower.transform.position = dampPosition;
                break;
            case TileStates.Landing:
                t.UpdateTileSetActive(playerGroundPosition, 6f);
                t.UpdateTileOrderedCoordinate(playerGroundPosition);
                t.UpdateTileDampSpeedForLanding();
                t.UpdateWindowTile(playerGroundPosition);
                t.UpdateTilesStatusPerFrame(0, 7f, changingHighRiseMultiplierBoost, changingMatrixYOffset, 0, playerGroundPosition);
                t.DrawTileInstanceCurrentFrame(false);

                break;

            case TileStates.PrepareLanding:
                t.UpdateTileSetActive(playerGroundPosition, 7f);
                t.UpdateTileOrderedCoordinate(playerGroundPosition);
                t.UpdateTileDampSpeedTogether(varyingDampSpeed);
                t.UpdateWindowTile(playerGroundPosition);
                t.UpdateTilesStatusPerFrame(2f, 7f, changingRadius / 2 + changingHighRiseMultiplierBoost, changingMatrixYOffset, 0, playerGroundPosition);
                t.DrawTileInstanceCurrentFrame(false);

                break;
        }
    }
    IEnumerator FileStagingAnimation(Vector3 target)
    {
        state = TileStates.Staging;
        float percent = 0;
        float initialRaidius = 2f;
        float targetRadius = SceneSwitcher.isInFloppy ? 5.5f : 8f;
        Vector3 currentPosition = FirstPersonController.playerGroundPosition;
        Vector3 path = Vector3.zero;
        while (percent < 1)
        {
            percent += Time.deltaTime;
            path = Vector3.Lerp(currentPosition, target, percent);
            float newRadius = Mathf.Lerp(initialRaidius, targetRadius, percent);
            t.UpdateTileSetActive(path, newRadius);
            t.UpdateTileOrderedCoordinate(path);
            t.UpdateTileDampSpeedTogether(varyingDampSpeed);
            t.UpdateWindowTile(path);
            t.UpdateTilesStatusPerFrame(0, newRadius, 0.0f, 0.5f, defaultNoiseWeight, path);
            if ((!FileManager.isFileFull || SceneSwitcher.isInFloppy) || t.activateWindowsIndependance)
                b.UpdateButtonPosition(SceneSwitcher.isInFloppy ? TileButtons.ButtonTile.DisplayState.delete : TileButtons.ButtonTile.DisplayState.save);
            t.DrawTileInstanceCurrentFrame(SceneSwitcher.isInFloppy ? true : !(SceneSwitcher.sd.currFile.isSaved || FileManager.isFileFull));

            yield return null;
        }
        float innerRadius = targetRadius * 0.5f;
        while (fileStagingCo != null)
        {
            switch (state)
            {
                case TileStates.Staging:
                    t.varyingNoiseTime = Time.time / 3;
                    t.UpdateTileOrderedCoordinate(path);
                    t.UpdateTileDampSpeedTogether(varyingDampSpeed);
                    t.UpdateWindowTile(path);
                    t.UpdateTilesStatusPerFrame(innerRadius, targetRadius, changingHighRiseMultiplierBoost, changingMatrixYOffset + 1f, 0.5f, path);
                    if ((!FileManager.isFileFull || SceneSwitcher.isInFloppy) || t.activateWindowsIndependance)
                        b.UpdateButtonPosition(SceneSwitcher.isInFloppy ? TileButtons.ButtonTile.DisplayState.delete : TileButtons.ButtonTile.DisplayState.save);
                    t.DrawTileInstanceCurrentFrame(SceneSwitcher.isInFloppy ? true :!(SceneSwitcher.sd.currFile.isSaved || FileManager.isFileFull));

                    break;
                case TileStates.Staging_Diving:
                    Vector3 playerGroundPosition = FirstPersonController.playerGroundPosition;

                    t.UpdateTileSetActive(playerGroundPosition, 10f);
                    t.UpdateTileOrderedCoordinate(playerGroundPosition);
                    t.UpdateTileDampSpeedForLanding();
                    t.UpdateWindowTile(playerGroundPosition);
                    t.UpdateTilesStatusPerFrame(0, 5f, changingHighRiseMultiplierBoost, changingMatrixYOffset, 0, playerGroundPosition);
                    b.UpdateButtonPosition(TileButtons.ButtonTile.DisplayState.off);

                    t.DrawTileInstanceCurrentFrame(false);

                    break;
                case TileStates.Staging_Deleting:
                    t.varyingNoiseTime = Time.time / 2f;
                    t.UpdateTileOrderedCoordinate(path);
                    t.UpdateTileDampSpeedTogether(0.2f);
                    t.UpdateWindowTile(path);
                    t.UpdateTilesStatusPerFrame(0, 4f, 2f, 5f, 12f, path);
                    b.UpdateButtonPosition(SceneSwitcher.isInFloppy ? TileButtons.ButtonTile.DisplayState.delete : TileButtons.ButtonTile.DisplayState.save);

                    t.DrawTileInstanceCurrentFrame(true);

                    break;

            }

            yield return null;
        }
        state = TileStates.NormalFollow;
    }

    IEnumerator WindowsClickedAnimation(bool buttonStateAfterAnimation)
    {
        t.activateWindowsIndependance = true;
        t.displayAndUpdateButton = true;
       float percent = 0;
        Vector3 initialAveragePosition = GetWindowsAveragePosition(false);
        float waveScale = 2.5f;
        b.ToggleSaveHasBeenClicked(true);
        while (percent < 1)
        {
            float interpolate = Mathf.Sin(percent * Mathf.PI / 2 - Mathf.PI / 8) * waveScale;
            t.changingWindowsYPos = interpolate + initialAveragePosition.y;

            percent += Time.deltaTime * 2f;
            yield return null;
        }
        t.displayAndUpdateButton = buttonStateAfterAnimation;
        b.ToggleSaveHasBeenClicked(false);
        t.changingWindowsYPos = initialAveragePosition.y;
        yield return new WaitForSeconds(0.4f);
        t.activateWindowsIndependance = false;

    }
    IEnumerator MatrixDeleteAnimation()
    {
        state = TileStates.Staging_Deleting;
        yield return new WaitForSeconds(0.8f);
        state = TileStates.Staging;
        yield return new WaitForSeconds(0.2f);
        state = TileStates.NormalFollow;

        OnFinishingDeleteFileAnimation?.Invoke();
    }

    IEnumerator MatrixPopupAnimation()
    {
        state = TileStates.Popup;
        yield return new WaitForSeconds(0.8f);
        state = TileStates.NormalFollow;
    }


    Vector3 CheckPlayerProximateDirection(Transform t)
    {
        float yRot = t.eulerAngles.y;
        Vector3 finalEuler = Vector3.zero;
        if (yRot >= -45f && yRot < 45f)
        {
            finalEuler = Vector3.forward;
        }
        else if (yRot >= 45f && yRot < 135f)
        {
            finalEuler = Vector3.right;
        }
        else if (yRot >= 135f && yRot < 225f)
        {
            finalEuler = Vector3.back;
        }
        else if (yRot >= 225f && yRot < 315f)
        {
            finalEuler = Vector3.left;
        }
        return finalEuler;
    }

    Vector3 GetWindowsAveragePosition(bool formation)
    {
        Vector3 divePosition = Vector3.zero;
        for (int i = 0; i < t.windowTiles.Length; i++)
        {
            if (t.windowTiles[i] == null)
                continue;
            if (formation)
                divePosition += t.windowTiles[i].initialXZPosition;
            else
                divePosition += t.windowTiles[i].smoothedFinalXYZPosition;

        }
        divePosition /= t.windowTiles.Length;
        return divePosition;
    }

    public float GetUnderGroundTilesProxiRadius()
    {
        float radius = 0;
        TileDrawInstance.TileData[] array = new TileDrawInstance.TileData[t.tileOrderedDict.Count];
        t.tileOrderedDict.Values.CopyTo(array, 0);

        foreach (TileDrawInstance.TileData t in array)
        {
            radius += Vector2.Distance(new Vector2(t.smoothedFinalXYZPosition.x, t.smoothedFinalXYZPosition.z),
        new Vector2(FirstPersonController.playerGroundPosition.x, FirstPersonController.playerGroundPosition.z));
        }
        if (array.Length >= 10)
            radius /= array.Length;
        else
            radius = 0;

        return radius -0.2f;
    }


    void ChangeFormation(float globalOffset, float curvatureOffset)
    {
        changingMatrixYOffset = -globalOffset;
        changingHighRiseMultiplierBoost = curvatureOffset;
    }

    #region EventSubscribtion
    void ChangeRadius(float pitch)
    {
        float percentage = !SceneSwitcher.isInFloppy ? Mathf.InverseLerp(45, 80, pitch) : Mathf.InverseLerp(-45, -80, pitch);
        changingRadius = percentage == 0? 0f : Mathf.Lerp(1.5f, defaultRadius, percentage);
    }

    void ReceiveDownAnimationGlobalPositionOffset_fromPlayerAnchorAnimation(float time, float distance)
    {
        changingMatrixYOffset = -time * 8 + 8f;
        changingHighRiseMultiplierBoost = time * 16;
        varyingDampSpeed = Mathf.Lerp(defaultDampSpeed, .01f, time);
    }
    void ReceiveDownAnimationGlobalPositionOffset(float y)
    {
        state = TileStates.PrepareLanding;

        changingMatrixYOffset = -y * 5;
        changingHighRiseMultiplierBoost = y * 18;
        varyingDampSpeed = Mathf.Lerp(defaultDampSpeed, .5f, y);
    }

    void ReceiveUpAnimationGlobalPositionOffset(float y)
    {
        state = TileStates.PrepareLanding;

        t.activateWindowsIndependance = true;
        t.changingWindowsYPos = transform.position.y - 1f;
        changingHighRiseMultiplierBoost = y * 13;

        changingMatrixYOffset = -y * 1;
        varyingDampSpeed = Mathf.Lerp(defaultDampSpeed, .6f, y);
    }

    void ExitThreshold(float number)
    {
        //if (!isInDiveFormation)
        {
            changingMatrixYOffset = 0;
            changingHighRiseMultiplierBoost = 0;
            varyingDampSpeed = defaultDampSpeed;
        }
        t.activateWindowsIndependance = false;

        state = TileStates.NormalFollow;
    }

    void SetWindowsIndependance(bool flag)
    {
        t.activateWindowsIndependance = flag;
    }

    public void StartStagingFile(FileObject f)
    {
        if (fileStagingCo != null)
            StopCoroutine(fileStagingCo);
        fileStagingCo = StartCoroutine(FileStagingAnimation(f.groundPosition));
    }

    void EndStagingFile_fromFileManager(FileObject a,FileObject b) 
    {
        EndStagingFile();
    }
    public void EndStagingFile()
    {
        if (fileStagingCo != null)
            StopCoroutine(fileStagingCo);
        fileStagingCo = null;
        state = TileStates.NormalFollow;
    }


    void StartSoaringAnimation(FirstPersonController player)
    {
        float playerPhysicalYPosEndPoint = 180f;
        float topOfFormationVShape = 100f;
        // Prepare landing, it will enlarge the formation Ring.
        state = TileStates.PrepareLanding;

        // Send position and rotation to playercontroller.
        Vector3 divePosition = isEnabled ? GetWindowsAveragePosition(true) : FirstPersonController.playerGroundPosition;
        divePosition.y += playerPhysicalYPosEndPoint;

        Quaternion finalRot = Quaternion.LookRotation(CheckPlayerProximateDirection(player.transform), Vector3.up);
        // Activate volume and scenery, set plane target position to form a "V" shape.
        // The dive animation is officially started.
        OnInitiateSoaringFromMatrix?.Invoke(divePosition, finalRot);
        isInDiveFormation = true;
        ChangeFormation(30, topOfFormationVShape);
    }
    void SoaringAnimation(float timePercent, float distancePercent)
    {
        float planeCatchUpYPos = -205f;

        // Player Went Through the "V" shaped formation matrix
        // Matrix will teleported to the very bottom and catch player
        // The state is set to "Landing", the outside ring of the formation will quickly move up.
        if (distancePercent < 0.60f && !hasTriggeredLandingGathering)
        {
            state = TileStates.Landing;
            hasTriggeredLandingGathering = true;
            t.TeleportMatrixAlongY(transform.position.y - 1000f);
            t.activateWindowsIndependance = false;
        }
        // The whole plane will follow player, creating a catch effect.
        if (distancePercent < 0.5f)
        {
            ChangeFormation(Mathf.Lerp(planeCatchUpYPos, -100, distancePercent), 0);
        }
    }
    void StartDivingAnimation(FirstPersonController player, bool faceSide)
    {
        float playerPhysicalYPosEndPoint = -78f;
        float formationLowestPoint = 20f;
        //if (state == TileStates.Staging)
        //return;

        // Prepare landing, it will enlarge the formation Ring.
        state = TileStates.PrepareLanding;

        // Send position and rotation to playercontroller.
        Vector3 divePosition = isEnabled ? GetWindowsAveragePosition(true) : FirstPersonController.playerGroundPosition;
        divePosition.y += playerPhysicalYPosEndPoint;

        Quaternion finalRot = faceSide ?
            Quaternion.LookRotation(CheckPlayerProximateDirection(player.transform), Vector3.up) :
            Quaternion.LookRotation(Vector3.down, CheckPlayerProximateDirection(player.transform));
        // Activate volume and scenery, set plane target position to form a "V" shape.
        // The dive animation is officially started.

        OnInitiateDivingFromMatrix?.Invoke(divePosition, finalRot);
        isInDiveFormation = true;
        ChangeFormation(formationLowestPoint, formationLowestPoint + 8f);

    }
    void DivingAnimation(float timePercent, float distancePercent)
    {
        float planeCatchUpYPos = 80f;
        // The window will Detach, move out the way for player to travel through.
        if (distancePercent < 0.9f && !hasWindowsDetached)
        {
            hasWindowsDetached = true;
            t.activateWindowsIndependance = true;
            t.changingWindowsYPos = transform.position.y - 300f;
        }
        // Player Went Through the "V" shaped formation matrix
        // Matrix will teleported to the very bottom and catch player
        // The state is set to "Landing", the outside ring of the formation will quickly move up.
        if (distancePercent < 0.6f && !hasTriggeredLandingGathering)
        {
            state = TileStates.Landing;
            hasTriggeredLandingGathering = true;
            t.TeleportMatrixAlongY(transform.position.y - 1000f);
            t.activateWindowsIndependance = false;
        }
        // The whole plane will follow player, creating a catch effect.
        if (distancePercent < 0.40f)
        {
            ChangeFormation(Mathf.Lerp(planeCatchUpYPos, 20, distancePercent), 0);
        }
    }
    void InitiateRetreatAndResetWindowsAnimation(bool buttonStateAfterAnimation)
    {
        StartCoroutine(WindowsClickedAnimation(buttonStateAfterAnimation));
    }

    void InitiateDeleteAnchorAnimation()
    {
        StartCoroutine(MatrixDeleteAnimation());
    }

    void InitiatePopupAnimation() 
    {
        StartCoroutine(MatrixPopupAnimation());
    }

    public void SwitchToStageDiving_fromPlayerAnchroAnimation(FirstPersonController f, bool b)
    {
        state = TileStates.Staging_Diving;
    }

    void ResetToDefault()
    {
        // Reset Windows Tile Behavior 
        SetWindowsIndependance(false);
        t.changingWindowsYPos = 0;
        // Exit staging mode and reset state
        EndStagingFile();

        //Reset Flags
        hasTriggeredLandingGathering = false;
        hasWindowsDetached = false;

        // Reset States
        isInDiveFormation = false;
        t.displayAndUpdateButton = true;

        // Reset Values
        varyingDampSpeed = defaultDampSpeed;
        changingMatrixYOffset = 0;
        changingHighRiseMultiplierBoost = 0;
    }

    #endregion


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, defaultRadius);
        if (t!= null && t.tileOrderedDict != null)
        foreach (TileDrawInstance.TileData t in t.tileOrderedDict.Values) 
        {
            Gizmos.DrawSphere(t.screenPos,0.1f);
        }
    }
}
