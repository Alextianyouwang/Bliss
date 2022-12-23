using System;
using System.Collections;
using UnityEngine;
public class TileMatrixManager : MonoBehaviour
{
    // Changing: The value does not has a default, will be assigned later.
    // Varying: The value will be set to a default at first, it will move between other values and the defalut.
    // Default: The value will not change.

    [SerializeField] private GameObject tile;
    [SerializeField] private SaveButton saveButton;
    [SerializeField] private DeleteButton deleteButton;
    [SerializeField] private int defaultTileDimension = 7;
    [SerializeField] private float defaultRadius = 15;
    [SerializeField] private bool isEnabled = true;

    //private TileMatrixFunctions t;
    private TileDrawInstance t;

    private Coroutine fileStagingCo;

    private float
        changingHighRiseMultiplierBoost,
        changingRadius,
        changingMatrixYOffset,
        varyingDampSpeed,
        defaultDampSpeed = 0.12f,
        defaultNoiseWeight = 0.5f;

    private bool
        isInDiveFormation = false,
        hasTriggeredLandingGathering = false,
        hasWindowsDetached = false;

    private enum TileStates { NormalFollow, Staging, Landing, PrepareLanding, Staging_Diving, Staging_Deleting }
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

        PlayerAnchorAnimation.OnPlayerStartAnchor += StartStagingFile;
        PlayerAnchorAnimation.OnPlayerExitAnchor += EndStagingFile;
        PlayerAnchorAnimation.OnPlayerTeleportAnimationFinished += ResetToDefault;
        PlayerAnchorAnimation.OnDiving += DivingAnimation;
        PlayerAnchorAnimation.OnSoring += SoaringAnimation;
        PlayerAnchorAnimation.OnRequestDive += StartDivingAnimation;
        PlayerAnchorAnimation.OnRequestDive += SwitchToStageDiving_fromPlayerAnchroAnimation;
        PlayerAnchorAnimation.OnPrepareDiving += ReceiveDownAnimationGlobalPositionOffset_fromPlayerAnchorAnimation;

        SaveButton.OnRetreatSaveButton += InitiateRetreatAndResetWindowsAnimation;
        DeleteButton.OnPlayerReleased += InitiateDeleteAnchorAnimation;
    }
    private void OnDisable()
    {
        FirstPersonController.OnPitchChange -= ChangeRadius;
        FirstPersonController.OnIncreaseDownAnimationTime -= ReceiveDownAnimationGlobalPositionOffset;
        FirstPersonController.OnIncreaseUpAnimationTime -= ReceiveUpAnimationGlobalPositionOffset;
        FirstPersonController.OnExitThreshold -= ExitThreshold;
        FirstPersonController.OnStartDiving -= StartDivingAnimation;
        FirstPersonController.OnStartSoaring -= StartSoaringAnimation;


        PlayerAnchorAnimation.OnPlayerStartAnchor -= StartStagingFile;
        PlayerAnchorAnimation.OnPlayerExitAnchor -= EndStagingFile;
        PlayerAnchorAnimation.OnPlayerTeleportAnimationFinished -= ResetToDefault;
        PlayerAnchorAnimation.OnDiving -= DivingAnimation;
        PlayerAnchorAnimation.OnSoring -= SoaringAnimation;
        PlayerAnchorAnimation.OnRequestDive -= StartDivingAnimation;
        PlayerAnchorAnimation.OnRequestDive -= SwitchToStageDiving_fromPlayerAnchroAnimation;
        PlayerAnchorAnimation.OnPrepareDiving -= ReceiveDownAnimationGlobalPositionOffset_fromPlayerAnchorAnimation;

        SaveButton.OnRetreatSaveButton -= InitiateRetreatAndResetWindowsAnimation;
        DeleteButton.OnPlayerReleased -= InitiateDeleteAnchorAnimation;
    }

    private void Awake()
    {
        //t = new TileMatrixFunctions(tile, saveButton, deleteButton, defaultTileDimension);
        t = new TileDrawInstance(tile, saveButton,deleteButton, defaultTileDimension);
    }
    void Start()
    {
        t.Initialize();
        varyingDampSpeed = defaultDampSpeed;
        state = TileStates.NormalFollow;
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
        Vector3 playerGroundPosition = FirstPersonController.playerGroundPosition;
        if (!isEnabled)
            return;
        t.UpdateButtonPosition(TileDrawInstance.ButtonTile.DisplayState.off);

        switch (state)
        {
            case TileStates.NormalFollow:
                t.UpdateTileSetActive(playerGroundPosition, changingRadius);
                t.UpdateTileOrderedCoordinate(playerGroundPosition);
                t.ResetWindowsTilePrefab();
                t.UpdateTileDampSpeedTogether(varyingDampSpeed);
                t.UpdateWindowTile(playerGroundPosition);
                t.UpdateTilesStatusPerFrame(0, defaultRadius, changingRadius / 2 + changingHighRiseMultiplierBoost, changingMatrixYOffset, defaultNoiseWeight, playerGroundPosition);
                t.DrawTileInstanceCurrentFrame(false);

                break;
            case TileStates.Landing:
                t.UpdateTileSetActive(playerGroundPosition, 7f);
                t.UpdateTileOrderedCoordinate(playerGroundPosition);
                t.ResetWindowsTilePrefab();
                t.UpdateTileDampSpeedForLanding();
                t.UpdateWindowTile(playerGroundPosition);
                t.UpdateTilesStatusPerFrame(0, 7f, changingHighRiseMultiplierBoost, changingMatrixYOffset, 0, playerGroundPosition);
                t.DrawTileInstanceCurrentFrame(false);

                break;

            case TileStates.PrepareLanding:
                t.UpdateTileSetActive(playerGroundPosition, 7f);
                t.UpdateTileOrderedCoordinate(playerGroundPosition);
                t.ResetWindowsTilePrefab();
                t.UpdateTileDampSpeedTogether(varyingDampSpeed);
                t.UpdateWindowTile(playerGroundPosition);
                t.UpdateTilesStatusPerFrame(0, 7f, changingRadius / 2 + changingHighRiseMultiplierBoost, changingMatrixYOffset, 0, playerGroundPosition);
                t.DrawTileInstanceCurrentFrame(false);

                break;
        }
    }
    IEnumerator FileStagingAnimation(Vector3 target)
    {
        state = TileStates.Staging;
        float percent = 0;
        float initialRaidius = changingRadius;
        float targetRadius = SceneSwitcher.isInClippy ? 4.5f : 8;
        Vector3 currentPosition = FirstPersonController.playerGroundPosition;
        Vector3 path = Vector3.zero;
        while (percent < 1)
        {
            percent += Time.deltaTime;
            path = Vector3.Lerp(currentPosition, target, percent);
            float newRadius = Mathf.Lerp(initialRaidius, targetRadius, percent);
            t.UpdateTileSetActive(path, newRadius);
            t.UpdateTileOrderedCoordinate(path);
            t.ResetWindowsTilePrefab();
            t.UpdateTileDampSpeedTogether(varyingDampSpeed);
            t.UpdateWindowTile(path);
            t.UpdateWindowsTilePrefab(SceneSwitcher.isInClippy);
            t.UpdateTilesStatusPerFrame(0, newRadius, 0.0f, 0.5f, defaultNoiseWeight, path);
            t.UpdateButtonPosition(SceneSwitcher.isInClippy ? TileDrawInstance.ButtonTile.DisplayState.delete : TileDrawInstance.ButtonTile.DisplayState.save);
            //t.UpdateButtonPosition(TileDrawInstance.ButtonTile.DisplayState.off);
            t.DrawTileInstanceCurrentFrame(true);

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
                    t.ResetWindowsTilePrefab();

                    t.UpdateTileDampSpeedTogether(varyingDampSpeed);
                    t.UpdateWindowTile(path);
                    t.UpdateWindowsTilePrefab(SceneSwitcher.isInClippy);
                    t.UpdateTilesStatusPerFrame(innerRadius, targetRadius, changingHighRiseMultiplierBoost, changingMatrixYOffset + 1f, 0.5f, path);
                    t.UpdateButtonPosition(SceneSwitcher.isInClippy ? TileDrawInstance.ButtonTile.DisplayState.delete : TileDrawInstance.ButtonTile.DisplayState.save);

                    t.DrawTileInstanceCurrentFrame(true);

                    break;
                case TileStates.Staging_Diving:
                    Vector3 playerGroundPosition = FirstPersonController.playerGroundPosition;

                    t.UpdateTileSetActive(playerGroundPosition, 10f);
                    t.UpdateTileOrderedCoordinate(playerGroundPosition);
                    t.ResetWindowsTilePrefab();
                    t.UpdateTileDampSpeedForLanding();
                    t.UpdateWindowTile(playerGroundPosition);

                    t.UpdateTilesStatusPerFrame(0, 5f, changingHighRiseMultiplierBoost, changingMatrixYOffset, 0, playerGroundPosition);
                    t.UpdateButtonPosition(TileDrawInstance.ButtonTile.DisplayState.off);

                    t.DrawTileInstanceCurrentFrame(false);

                    break;
                case TileStates.Staging_Deleting:
                    t.varyingNoiseTime = Time.time / 2f;
                    t.UpdateTileOrderedCoordinate(path);
                    t.ResetWindowsTilePrefab();

                    t.UpdateTileDampSpeedTogether(0.2f);
                    t.UpdateWindowTile(path);
                    t.UpdateTilesStatusPerFrame(0, 4f, 2f, 5f, 12f, path);
                    t.UpdateButtonPosition(SceneSwitcher.isInClippy ? TileDrawInstance.ButtonTile.DisplayState.delete : TileDrawInstance.ButtonTile.DisplayState.save);

                    t.DrawTileInstanceCurrentFrame(true);

                    break;

            }

            yield return null;
        }
        state = TileStates.NormalFollow;
    }

    IEnumerator WindowsClickedAnimation()
    {
        t.activateWindowsIndependance = true;
        float percent = 0;
        Vector3 initialAveragePosition = GetWindowsAveragePosition(false);
        float waveScale = 4f;
        t.ToggleSaveHasBeenClicked(true);
        while (percent < 1)
        {
            float interpolate =  Mathf.Sin(percent * Mathf.PI / 2 - Mathf.PI / 4) * waveScale ;
            t.changingWindowsYPos = interpolate + initialAveragePosition.y;

            percent += Time.deltaTime * 2f;
            yield return null;
        }
        t.allowWindowsSetPrefabToButtons = false;
        t.ToggleSaveHasBeenClicked(false);
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
        OnFinishingDeleteFileAnimation?.Invoke();
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


    void ChangeFormation(float globalOffset, float curvatureOffset)
    {
        changingMatrixYOffset = -globalOffset;
        changingHighRiseMultiplierBoost = curvatureOffset;
    }

    #region EventSubscribtion
    void ChangeRadius(float pitch)
    {
        float percentage = !SceneSwitcher.isInClippy ? Mathf.InverseLerp(30, 75, pitch) : Mathf.InverseLerp(-30, -75, pitch);
        changingRadius = Mathf.Lerp(0, defaultRadius, percentage);
    }

    void ReceiveDownAnimationGlobalPositionOffset_fromPlayerAnchorAnimation(float time, float distance)
    {
        changingMatrixYOffset = -time * 8 + 8f;
        changingHighRiseMultiplierBoost = time * 8;
        varyingDampSpeed = Mathf.Lerp(defaultDampSpeed, .01f, time);
    }
    void ReceiveDownAnimationGlobalPositionOffset(float y)
    {
        state = TileStates.PrepareLanding;

        changingMatrixYOffset = -y * 4;
        changingHighRiseMultiplierBoost = y * 4;
        varyingDampSpeed = Mathf.Lerp(defaultDampSpeed, .7f, y);
    }

    void ReceiveUpAnimationGlobalPositionOffset(float y)
    {
        state = TileStates.PrepareLanding;

        t.activateWindowsIndependance = true;
        t.changingWindowsYPos = transform.position.y - 1f;
        changingHighRiseMultiplierBoost = y * 10;

        changingMatrixYOffset = -y * 1;
        varyingDampSpeed = Mathf.Lerp(defaultDampSpeed, .6f, y);
    }

    void ExitThreshold(float number)
    {
        if (!isInDiveFormation)
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

    public void StartStagingFile(Vector3 target)
    {
        if (fileStagingCo != null)
            StopCoroutine(fileStagingCo);
        fileStagingCo = StartCoroutine(FileStagingAnimation(target));
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

        //if (state == TileStates.Staging)
        //return;

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
        float formationLowestPoint = 27f;
        //if (state == TileStates.Staging)
        //return;

        // Prepare landing, it will enlarge the formation Ring.
        state = TileStates.PrepareLanding;

        // Send position and rotation to playercontroller.
        Vector3 divePosition = isEnabled? GetWindowsAveragePosition(true) : FirstPersonController.playerGroundPosition;
        divePosition.y += playerPhysicalYPosEndPoint;

        Quaternion finalRot = faceSide ?
            Quaternion.LookRotation(CheckPlayerProximateDirection(player.transform), Vector3.up) :
            Quaternion.LookRotation(Vector3.down, CheckPlayerProximateDirection(player.transform));
        // Activate volume and scenery, set plane target position to form a "V" shape.
        // The dive animation is officially started.

        OnInitiateDivingFromMatrix?.Invoke(divePosition, finalRot);
        isInDiveFormation = true;
        ChangeFormation(formationLowestPoint, formationLowestPoint);

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
        if (distancePercent < 0.60f && !hasTriggeredLandingGathering)
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
    void InitiateRetreatAndResetWindowsAnimation()
    {
        StartCoroutine(WindowsClickedAnimation());
    }

    void InitiateDeleteAnchorAnimation()
    {
        StartCoroutine(MatrixDeleteAnimation());
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
        t.allowWindowsSetPrefabToButtons = true;

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
        //if (t != null)
          /*  foreach (TileData t in t.tileOrderedDict.Values)
            {
                Gizmos.DrawSphere(t., 0.5f);
            }*/
    }
}
