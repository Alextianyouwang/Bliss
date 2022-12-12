using System;
using System.Collections;
using UnityEngine;

public class TileMatrixManager : MonoBehaviour
{
    // Changing: The value does not has a default, will be assigned later
    // Varying: The value will be set to a default at first, it will move between other values and the defalut
    // Default: The value will not change

    [SerializeField] private GameObject tile;
    [SerializeField] private GameObject saveButton;
    [SerializeField] private GameObject deleteButton;
    [SerializeField] private int defaultTileDimension = 7;
    [SerializeField] private float defaultRadius = 15;
    [SerializeField] private LayerMask groundMask;

    private TileMatrixFunctions t;

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

    private enum TileStates { NormalFollow, Staging, Landing, PrepareLanding }
    private TileStates state;

    public static Action<Vector3, Quaternion> OnInitiateDivingFromMatrix;
    public static Action<Vector3, Quaternion> OnInitiateSoaringFromMatrix;


    private void OnEnable()
    {
        FirstPersonController.OnPitchChange += ChangeRadius;
        PlayerAnchorAnimation.OnStageFile += StartStagingFile;
        PlayerAnchorAnimation.OnPlayerExitAnchor += EndStagingFile;
        PlayerAnchorAnimation.OnPlayerTeleportAnimationFinished += EndAnimation;

        FirstPersonController.OnIncreaseDownAnimationTime += ReceiveDownAnimationGlobalPositionOffset;
        FirstPersonController.OnIncreaseUpAnimationTime += ReceiveUpAnimationGlobalPositionOffset;
        FirstPersonController.OnExitThreshold += ExitThreshold;
        FirstPersonController.OnStartDiving += StartDivingAnimation;
        FirstPersonController.OnStartSoaring += StartSoaringAnimation;

        PlayerAnchorAnimation.OnDiving += DivingAnimation;
        PlayerAnchorAnimation.OnSoring += SoaringAnimation;

        PlayerAnchorAnimation.OnStartDiving += StartDivingAnimation;

    }
    private void OnDisable()
    {
        FirstPersonController.OnPitchChange -= ChangeRadius;
        PlayerAnchorAnimation.OnStageFile -= StartStagingFile;
        PlayerAnchorAnimation.OnPlayerExitAnchor -= EndStagingFile;
        PlayerAnchorAnimation.OnPlayerTeleportAnimationFinished -= EndAnimation;

        FirstPersonController.OnIncreaseDownAnimationTime -= ReceiveDownAnimationGlobalPositionOffset;
        FirstPersonController.OnIncreaseUpAnimationTime -= ReceiveUpAnimationGlobalPositionOffset;
        FirstPersonController.OnExitThreshold -= ExitThreshold;
        FirstPersonController.OnStartDiving -= StartDivingAnimation;
        FirstPersonController.OnStartSoaring -= StartSoaringAnimation;


        PlayerAnchorAnimation.OnDiving -= DivingAnimation;
        PlayerAnchorAnimation.OnSoring -= SoaringAnimation;

        PlayerAnchorAnimation.OnStartDiving -= StartDivingAnimation;
    }

    private void Awake()
    {
        t = new TileMatrixFunctions(transform, tile, saveButton, deleteButton, defaultTileDimension);
    }
    void Start()
    {
        t.Initialize();
        varyingDampSpeed = defaultDampSpeed;
        state = TileStates.NormalFollow;
    }
    void Update()
    {
        Vector3 playerGroundPosition = FirstPersonController.playerGroundPosition;
        switch (state)
        {
            case TileStates.NormalFollow:
                t.UpdateTileSetActive(playerGroundPosition, changingRadius);
                t.UpdateTileOrderedCoordinate(playerGroundPosition);
                t.ResetWindowsTilePrefab();
                t.UpdateTileDampSpeedTogether(varyingDampSpeed);
                t.UpdateWindowTile(playerGroundPosition);
                t.UpdateTilesStatusPerFrame(0, defaultRadius, changingRadius / 2 + changingHighRiseMultiplierBoost, changingMatrixYOffset, defaultNoiseWeight, playerGroundPosition);
                break;
            case TileStates.Staging:
                break;
            case TileStates.Landing:
                t.UpdateTileSetActive(playerGroundPosition, 6.5f);
                t.UpdateTileOrderedCoordinate(playerGroundPosition);
                t.ResetWindowsTilePrefab();
                t.UpdateTielDampSpeedForLanding();
                t.UpdateWindowTile(playerGroundPosition);

                t.UpdateTilesStatusPerFrame(0, 6.5f, changingHighRiseMultiplierBoost, changingMatrixYOffset, 0, playerGroundPosition);
                break;

            case TileStates.PrepareLanding:
                t.UpdateTileSetActive(playerGroundPosition, 6.5f);
                t.UpdateTileOrderedCoordinate(playerGroundPosition);
                t.ResetWindowsTilePrefab();
                t.UpdateTileDampSpeedTogether(varyingDampSpeed);
                t.UpdateWindowTile(playerGroundPosition);
                t.UpdateTilesStatusPerFrame(0, 6.5f, changingRadius / 2 + changingHighRiseMultiplierBoost, changingMatrixYOffset, 0, playerGroundPosition);
                break;
        }
    }

    #region EventSubscribtion
    void ChangeRadius(float pitch)
    {
        float percentage = !SceneSwitcher.isInClippy ? Mathf.InverseLerp(30, 75, pitch) : Mathf.InverseLerp(-30, -75, pitch);
        changingRadius = Mathf.Lerp(0, defaultRadius, percentage);
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
    void ChangeFormation(float globalOffset, float curvatureOffset)
    {
        changingMatrixYOffset = -globalOffset;
        changingHighRiseMultiplierBoost = curvatureOffset;
    }

    void SetWindowsIndependance(bool flag)
    {
        t.activateWindowsIndependance = flag;
    }

    public void StartStagingFile(Vector3 target)
    {
        //if (!isInClippy) 
        {
            if (fileStagingCo != null)
                StopCoroutine(fileStagingCo);
            fileStagingCo = StartCoroutine(FileStagingAnimation(target));
        }

    }
    public void EndStagingFile()
    {
        if (fileStagingCo != null)
            StopCoroutine(fileStagingCo);
        fileStagingCo = null;
        state = TileStates.NormalFollow;
    }
    public void SetDeleteButtonReference(FileObject f) 
    {
        //print(f.name);
        foreach (TileBase t in t.tileOrderedDict.Values) 
        {
           // t.SetFileReference(f);
        }

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

    void StartSoaringAnimation(FirstPersonController player)
    {
        float playerPhysicalYPosEndPoint = 180f;
        float topOfFormationVShape = 100f;

        //if (state == TileStates.Staging)
            //return;

        // Prepare landing, it will enlarge the formation Ring.
        state = TileStates.PrepareLanding;

        // Send position and rotation to playercontroller.
        Vector3 divePosition = Vector3.zero;
        for (int i = 0; i < t.windowTiles.Length; i++)
        {
            divePosition += t.windowTiles[i].formationFinalPosition;
        }
        divePosition /= t.windowTiles.Length;
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
    void StartDivingAnimation(FirstPersonController player)
    {
        float playerPhysicalYPosEndPoint = -78f;
        float formationLowestPoint = 27f;

        //if (state == TileStates.Staging)
            //return;

        // Prepare landing, it will enlarge the formation Ring.
        state = TileStates.PrepareLanding;

        // Send position and rotation to playercontroller.
        Vector3 divePosition = Vector3.zero;
        for (int i = 0; i < t.windowTiles.Length; i++)
        {
            divePosition += t.windowTiles[i].formationFinalPosition;
        }
        divePosition /= t.windowTiles.Length;
        divePosition.y += playerPhysicalYPosEndPoint;

        Quaternion finalRot = Quaternion.LookRotation(CheckPlayerProximateDirection(player.transform), Vector3.up);
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
    void EndAnimation()
    {

        isInDiveFormation = false;
        varyingDampSpeed = defaultDampSpeed;
        SetWindowsIndependance(false);
        t.changingWindowsYPos = 0;
        hasTriggeredLandingGathering = false;
        hasWindowsDetached = false;
        state = TileStates.NormalFollow;
        EndStagingFile();
    }

    #endregion
    IEnumerator FileStagingAnimation(Vector3 target)
    {
        state = TileStates.Staging;
        float percent = 0;
        float initialRaidius = changingRadius;
        float targetRadius = 8;
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
            t.UpdateTilesStatusPerFrame(0, newRadius, 0.3f, changingMatrixYOffset, defaultNoiseWeight, path);
            yield return null;
        }
        float innerRadius = targetRadius * 0.5f;
        float highRiseMultipler = 1f;
        while (fileStagingCo != null)
        {
            t.UpdateTileOrderedCoordinate(path);
            t.ResetWindowsTilePrefab();

            t.UpdateTileDampSpeedTogether(varyingDampSpeed);
            t.UpdateWindowTile(path);
            t.UpdateWindowsTilePrefab(SceneSwitcher.isInClippy);
            t.UpdateTilesStatusPerFrame(innerRadius, targetRadius, highRiseMultipler, changingMatrixYOffset, 0.5f, path);
            yield return null;
        }
        state = TileStates.NormalFollow;

    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, defaultRadius);
        if (t != null)
            foreach (TileBase t in t.tileOrderedDict.Values)
            {
                Gizmos.DrawSphere(t.formationFinalPosition, 0.5f);
            }
    }
}
