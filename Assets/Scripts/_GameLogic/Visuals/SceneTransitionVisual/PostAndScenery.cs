using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;

public class PostAndScenery : MonoBehaviour
{
    #region SceneTransitionSceneriesAndAnimations
    public Camera cam;
    public Volume localVolume;
    public VolumeProfile blissProfile,clippyProfile;
    public CustomPassVolume pass;
    [SerializeField] private GameObject diveVolume, diveScenes;

    private float originalFOV,targetFOV, camFOVSpeedRef;
    private float originalChromaticBliss, targetChromaticBliss, chromSpeedRefBliss;
    private float originalVignetteBliss, targetVignetteBliss, vignetteSpeedRefBliss;  
    private float originalChromaticClippy, targetChromaticClippy, chromSpeedRefClippy;
    private float originalVignetteClippy, targetVignetteClippy, vignetteSpeedRefClippy;
    float preTeleportFOVMultiplier = 1.5f;
    float stageModeChromatic = 1f ,stageModeVignette = 0.44f;

    Coroutine waitEmergeCo;
    ChromaticAberration caBliss,caClippy;
    Vignette vBliss,vClippy;
    CustomPass always,lessEqual;
    ColorAdjustments colorAdjDiveSoar;

    private GameObject
        diveVolume_instance,
        diveScenes_instance;
    private AmbientOcclusion ao;
    #endregion

    private void OnEnable()
    {
        FirstPersonController.OnEnterThreshold += EnlargeFOV;
        FirstPersonController.OnExitThreshold += ShrinkFOV;

        TileMatrixManager.OnInitiateDivingFromMatrix += EnableDiveVolumeAndScene;
        TileMatrixManager.OnInitiateSoaringFromMatrix += EnableSoarVolumeAndScene;
        PlayerAnchorAnimation.OnPlayerTeleportAnimationFinished += DisableVolumenAndScene;
        PlayerAnchorAnimation.OnDiving += AdjustAOInDiveScene;

        SceneSwitcher.OnClippyToggle += ToggleClippyVolume;
        PlayerAnchorAnimation.OnRequestDive += EnlargeFOV_fromPlayerAnchorAnimation;
        PlayerAnchorAnimation.OnPlayerTeleportAnimationFinished += ShrinkFOV_fromPlayerAnchorAnimation;
    }
    private void OnDisable()
    {
        FirstPersonController.OnEnterThreshold -= EnlargeFOV;
        FirstPersonController.OnExitThreshold -= ShrinkFOV;

        TileMatrixManager.OnInitiateDivingFromMatrix -= EnableDiveVolumeAndScene;
        TileMatrixManager.OnInitiateSoaringFromMatrix -= EnableSoarVolumeAndScene;

        PlayerAnchorAnimation.OnPlayerTeleportAnimationFinished -= DisableVolumenAndScene;
        PlayerAnchorAnimation.OnDiving -= AdjustAOInDiveScene;

        SceneSwitcher.OnClippyToggle -= ToggleClippyVolume;

        PlayerAnchorAnimation.OnRequestDive -= EnlargeFOV_fromPlayerAnchorAnimation;

        PlayerAnchorAnimation.OnPlayerTeleportAnimationFinished -= ShrinkFOV_fromPlayerAnchorAnimation;



        vBliss.intensity.value = 0;
        caBliss.intensity.value = 0;
        vClippy.intensity.value = 0;
        caClippy.intensity.value = 0;
        colorAdjDiveSoar.postExposure.value = -2.5f;
    }
    private void Update()
    {
        UpdatePostprocessingValue();
    }
    private void Start()
    {
        InitializePostprocessings();
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
    #region SceneTransitionSceneriesAndAnimations
    void InitializePostprocessings() 
    {
        originalFOV = cam.fieldOfView;
        targetFOV = originalFOV;

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


        always = pass.customPasses[0];
        lessEqual = pass.customPasses[1];
    }

    void AdjustAOInDiveScene(float timeValue,float distValue) 
    {
        if (ao != null)
            ao.intensity.value = (1- distValue) * 8;
    }
    void ZeroAOInDiveScene() 
    {
        if (ao != null)
            ao.intensity.value = 0;
    }
    void EnableDiveVolumeAndScene(Vector3 divePosition,Quaternion diveRot) 
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

    void EnlargeFOV_fromPlayerAnchorAnimation(FirstPersonController p,bool b) 
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

        always.enabled = true;
        lessEqual.enabled = true;

        if (waitEmergeCo != null)
            StopCoroutine(waitEmergeCo);
    }

    void ShrinkFOV_fromPlayerAnchorAnimation() 
    {
        ShrinkFOV(0);
    }
    void ShrinkFOV(float f) 
    {
        targetFOV = originalFOV;
        targetChromaticBliss = originalVignetteBliss;
        targetVignetteBliss = originalVignetteBliss;
        targetChromaticClippy = originalChromaticClippy;
        targetVignetteClippy =originalVignetteClippy;

        waitEmergeCo = StartCoroutine(WaitUntilAllBlocksEmerge());
    }

    IEnumerator WaitUntilAllBlocksEmerge() 
    {
        yield return new WaitForSeconds(0.5f);
        always.enabled = false;
        lessEqual.enabled = false;
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
