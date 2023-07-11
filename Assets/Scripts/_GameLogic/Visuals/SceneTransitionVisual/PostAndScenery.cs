using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

public class PostAndScenery : MonoBehaviour
{

    #region SceneTransitionSceneriesAndAnimations
    public Camera cam;
    public Volume localVolume;
    public VolumeProfile blissProfile, clippyProfile;
    //public CustomPassVolume pass;
    public Material grassMat;
    [SerializeField] private GameObject diveVolume, diveScenes;

    private float originalFOV, targetFOV, camFOVSpeedRef;
    private float originalChromaticBliss, targetChromaticBliss, chromSpeedRefBliss;
    private float originalVignetteBliss, targetVignetteBliss, vignetteSpeedRefBliss;
    private float originalChromaticClippy, targetChromaticClippy, chromSpeedRefClippy;
    private float originalVignetteClippy, targetVignetteClippy, vignetteSpeedRefClippy;
    float preTeleportFOVMultiplier = 1.5f;
    float stageModeChromatic = 1f, stageModeVignette = 0.44f;

    Coroutine cutoutShrinkCo;
    ChromaticAberration caBliss, caClippy;
    Vignette vBliss, vClippy;
    ColorAdjustments colorAdjDiveSoar;

    private GameObject
        diveVolume_instance,
        diveScenes_instance;
    private AmbientOcclusion ao;


    public static Func<bool> OnTestingWindowsAboveGround;
    public static Func<float> OnGettingUndergroundTileRadius;
    #endregion

    public Image fadeScreen;

    private void OnEnable()
    {
        GrassCutout(0, Vector3.zero);

        FirstPersonController.OnEnterThreshold += EnlargeFOV;
        FirstPersonController.OnExitThreshold += ShrinkFOV;

        TileMatrixManager.OnInitiateDivingFromMatrix += EnableDiveVolumeAndScene;
        TileMatrixManager.OnInitiateSoaringFromMatrix += EnableSoarVolumeAndScene;
        AM_BlissMain.OnPlayerTeleportAnimationFinished += DisableVolumenAndScene;
        AM_BlissMain.OnDiving += AdjustAOInDiveScene;

        SceneDataMaster.OnFloppyToggle += ToggleClippyVolume;
        SceneDataMaster.OnFloppyToggle += FadeFromBlackToClear;
        AM_BlissMain.OnRequestDive += EnlargeFOV_fromPlayerAnchorAnimation;
        AM_BlissMain.OnPlayerTeleportAnimationFinished += ShrinkFOV_fromPlayerAnchorAnimation;
        AM_BlissMain.OnDiving += FadeInWhileDiving;

        FileObject.OnPlayerAnchored += ShrinkFOV_fromFileObject;

    }
    private void OnDisable()
    {
        FirstPersonController.OnEnterThreshold -= EnlargeFOV;
        FirstPersonController.OnExitThreshold -= ShrinkFOV;

        TileMatrixManager.OnInitiateDivingFromMatrix -= EnableDiveVolumeAndScene;
        TileMatrixManager.OnInitiateSoaringFromMatrix -= EnableSoarVolumeAndScene;

        AM_BlissMain.OnPlayerTeleportAnimationFinished -= DisableVolumenAndScene;
        AM_BlissMain.OnDiving -= AdjustAOInDiveScene;

        SceneDataMaster.OnFloppyToggle -= ToggleClippyVolume;
        SceneDataMaster.OnFloppyToggle -= FadeFromBlackToClear;


        AM_BlissMain.OnRequestDive -= EnlargeFOV_fromPlayerAnchorAnimation;
        AM_BlissMain.OnPlayerTeleportAnimationFinished -= ShrinkFOV_fromPlayerAnchorAnimation;
        AM_BlissMain.OnDiving -= FadeInWhileDiving;


        FileObject.OnPlayerAnchored -= ShrinkFOV_fromFileObject;


        vBliss.intensity.value = 0;
        caBliss.intensity.value = 0;
        vClippy.intensity.value = 0;
        caClippy.intensity.value = 0;
        colorAdjDiveSoar.postExposure.value = -2.5f;
        GrassCutout(0, Vector3.zero);
    }
    private void Update()
    {
        UpdatePostprocessingValue();
    }
    private void Start()
    {
        targetFOV = 60f;
        InitializePostprocessings();
        InitializeFadeScreenColor();
        
    }

    void InitializeFadeScreenColor() 
    {
        fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, 0);

    }
    void FadeInWhileDiving(float timePercent, float distancePercent) 
    {
        float initialDistance = 0.25f;
        if (distancePercent < initialDistance) 
        {
            float interpolator = (initialDistance - distancePercent) / initialDistance;
            fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, interpolator);
        }
        
    }
    void FadeFromBlackToClear(bool isInFloppy) 
    {
        if (isInFloppy)
            StartCoroutine(EnterFloppyVisualAnimation(false, 0.8f));
    }
    IEnumerator EnterFloppyVisualAnimation(bool fadeToBlack, float speed) 
    {
        float percent = 0;
        float initialValue = fadeToBlack ? 0 : 1.2f;
        float targetValue = fadeToBlack ? 1.2f : 0;

        while (percent < 1f)
        {
            percent += Time.deltaTime * speed;
            fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, Mathf.Lerp(initialValue,targetValue,percent));
            targetFOV = originalFOV * Mathf.Lerp(0.6f, 1f, percent);

            yield return null;
        }
        targetFOV = originalFOV;

    }
    void ToggleClippyVolume(bool isInClippy)
    {
        if (isInClippy)
        {
            localVolume.profile = clippyProfile;
        }

        else
        {
            localVolume.profile = blissProfile;
        }
    }

    void GrassCutout(float radius, Vector3 position)
    {
        grassMat.SetVector("_CutoutPosition", new Vector4(position.x, position.y, position.z, 0));
        grassMat.SetFloat("_CutoutRadius", radius);
    }
    #region SceneTransitionSceneriesAndAnimations
    void InitializePostprocessings()
    {
        originalFOV = cam.fieldOfView;

        blissProfile.TryGet(out caBliss);
        originalChromaticBliss = caBliss.intensity.value;
        targetChromaticBliss = originalChromaticBliss;

        blissProfile.TryGet(out vBliss);
        originalVignetteBliss = vBliss.intensity.value;
        targetVignetteBliss = originalVignetteBliss;

        clippyProfile.TryGet(out caClippy);
        originalChromaticClippy = caClippy.intensity.value;
        targetChromaticClippy = originalChromaticClippy;

        clippyProfile.TryGet(out vClippy);
        originalVignetteClippy = vClippy.intensity.value;
        targetVignetteClippy = originalVignetteClippy;

        diveVolume_instance = Instantiate(diveVolume);
        diveVolume_instance.GetComponent<Volume>().profile.TryGet(out ao);
        diveVolume_instance.SetActive(false);
        diveScenes_instance = Instantiate(diveScenes);
        diveScenes_instance.SetActive(false);

        diveVolume_instance.GetComponent<Volume>().profile.TryGet(out colorAdjDiveSoar);


        //always = pass.customPasses[0];
        //lessEqual = pass.customPasses[1];
    }

    void AdjustAOInDiveScene(float timeValue, float distValue)
    {
        if (ao != null)
            ao.intensity.value = (1 - distValue) * 8;
    }
    void ZeroAOInDiveScene()
    {
        if (ao != null)
            ao.intensity.value = 0;
    }
    void EnableDiveVolumeAndScene(Vector3 divePosition, Quaternion diveRot)
    {
        diveVolume_instance.SetActive(true);
        diveVolume_instance.transform.position = divePosition;
        diveVolume_instance.transform.eulerAngles = new Vector3(0, 0, 0);

        diveScenes_instance.SetActive(true);
        diveScenes_instance.transform.position = divePosition;
        diveScenes_instance.transform.eulerAngles = new Vector3(0, 0, 0);
        colorAdjDiveSoar.postExposure.value = -2f;


        ZeroAOInDiveScene();
    }

    void EnableSoarVolumeAndScene(Vector3 divePosition, Quaternion diveRot)
    {
        diveVolume_instance.SetActive(true);
        diveVolume_instance.transform.position = divePosition;
        diveVolume_instance.transform.eulerAngles = new Vector3(180, 0, 0);
        diveScenes_instance.SetActive(true);
        diveScenes_instance.transform.position = divePosition;
        diveScenes_instance.transform.eulerAngles = new Vector3(180, 0, 0);
        colorAdjDiveSoar.postExposure.value = -7f;


        ZeroAOInDiveScene();
    }

    void DisableVolumenAndScene()
    {
        diveVolume_instance.SetActive(false);
        diveScenes_instance.SetActive(false);
    }

    void EnlargeFOV_fromPlayerAnchorAnimation(FirstPersonController p, bool b)
    {
        EnlargeFOV(0);
    }
    void EnlargeFOV(float f)
    {

        targetFOV = originalFOV * preTeleportFOVMultiplier;
        targetChromaticBliss = stageModeChromatic;
        targetVignetteBliss = stageModeVignette;
        targetChromaticClippy = stageModeChromatic;
        targetVignetteClippy = stageModeVignette;

        if (cutoutShrinkCo != null)
            StopCoroutine(cutoutShrinkCo);
        GrassCutout(20f, FirstPersonController.playerGroundPosition);

    }

    void ShrinkFOV_fromPlayerAnchorAnimation()
    {
        ShrinkFOV(0);
    }
    void ShrinkFOV_fromFileObject(FileObject f)
    {
        targetFOV = originalFOV;
        targetChromaticBliss = originalVignetteBliss;
        targetVignetteBliss = originalVignetteBliss;
        targetChromaticClippy = originalChromaticClippy;
        targetVignetteClippy = originalVignetteClippy;

        GrassCutout(0, FirstPersonController.playerGroundPosition);

    }
    void ShrinkFOV(float f)
    {

        targetFOV = originalFOV;
        targetChromaticBliss = originalVignetteBliss;
        targetVignetteBliss = originalVignetteBliss;
        targetChromaticClippy = originalChromaticClippy;
        targetVignetteClippy = originalVignetteClippy;



        cutoutShrinkCo = StartCoroutine(CutoutShrinkAccordingToTile());

    }

    IEnumerator CutoutShrinkAccordingToTile()
    {

        float percentage = 0;
        while (percentage < 1)
        {
            percentage += Time.deltaTime *2f;
            
            float liveRadius = OnGettingUndergroundTileRadius();
            liveRadius = liveRadius > 2 ? liveRadius : 0;
            GrassCutout(liveRadius, FirstPersonController.playerGroundPosition);

            yield return null;
        }
        GrassCutout(0, FirstPersonController.playerGroundPosition);
    }

    void UpdatePostprocessingValue()
    {
        cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetFOV, ref camFOVSpeedRef, 0.15f);
        if (caBliss != null)
        {
            caBliss.intensity.value = Mathf.SmoothDamp(caBliss.intensity.value, targetChromaticBliss, ref chromSpeedRefBliss, 0.3f);

        }
        if (vBliss != null)
        {
            vBliss.intensity.value = Mathf.SmoothDamp(vBliss.intensity.value, targetVignetteBliss, ref vignetteSpeedRefBliss, 0.3f);
        }
        if (caClippy != null)
        {
            caClippy.intensity.value = Mathf.SmoothDamp(caClippy.intensity.value, targetChromaticClippy, ref chromSpeedRefClippy, 0.3f);

        }
        if (vClippy != null)
        {
            vClippy.intensity.value = Mathf.SmoothDamp(vClippy.intensity.value, targetVignetteClippy, ref vignetteSpeedRefClippy, 0.3f);
        }
    }
    #endregion



}
