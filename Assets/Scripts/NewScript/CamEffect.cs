using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class CamEffect : MonoBehaviour
{
    public Camera cam;
    public Volume volume;
    public VolumeProfile profile;
    public CustomPassVolume pass;

    private float originalFOV,currentFOV,targetFOV, camFOVSpeedRef;
    private float originalChromatic, targetChromatic, chromSpeedRef;
    private float originalVignette, targetVignette, vignetteSpeedRef;
    private float originalExposure, targetExposure, exposureSpeedRef;
    float preTeleportFOVMultiplier = 1.5f;
    float stageModeChromatic = 1f ,stageModeVignette = 0.6f;

    Coroutine waitEmergeCo;

    ChromaticAberration ca;
    Vignette v;
    Exposure e;
    CustomPass always,lessEqual;
    private void OnEnable()
    {
        FirstPersonController.OnEnterThreshold += EnlargeFOV;
        FirstPersonController.OnExitThreshold += ShrinkFOV;
        FirstPersonController.OnTeleporting += DiveIn; 
    }
    private void OnDisable()
    {
        FirstPersonController.OnEnterThreshold -= EnlargeFOV;
        FirstPersonController.OnExitThreshold -= ShrinkFOV;
        FirstPersonController.OnTeleporting -= DiveIn;

        v.intensity.value = 0;
        ca.intensity.value = 0;


    }
    private void Start()
    {
        originalFOV = cam.fieldOfView;
        targetFOV = originalFOV;

        profile.TryGet(out ca);
        originalChromatic = ca.intensity.value;
        targetChromatic = originalChromatic;

        profile.TryGet(out v);
        originalVignette = v.intensity.value;
        targetVignette = originalVignette;


        always = pass.customPasses[0];
        lessEqual = pass.customPasses[1];
    }

    void EnlargeFOV(float f) 
    {
        targetFOV = originalFOV * preTeleportFOVMultiplier;
        targetChromatic = stageModeChromatic;
        targetVignette = stageModeVignette;

        always.enabled = true;
        lessEqual.enabled = true;

        if (waitEmergeCo != null)
            StopCoroutine(waitEmergeCo);
        
    }

    void ShrinkFOV(float f) 
    {
        targetFOV = originalFOV;
        targetChromatic = originalVignette;
        targetVignette = originalVignette;

        waitEmergeCo = StartCoroutine(WaitUntilAllBlocksEmerge());

    }

    void DiveIn(FirstPersonController f)
    {
        //targetChromatic =1;
        //targetVignette =1;
    }
    //PlaceHolder
    IEnumerator WaitUntilAllBlocksEmerge() 
    {
        yield return new WaitForSeconds(0.5f);
        always.enabled = false;
        lessEqual.enabled = false;
    }
    private void Update()
    {
        cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetFOV, ref camFOVSpeedRef, 0.15f);
        if (ca != null) 
        {
            ca.intensity.value = Mathf.SmoothDamp(ca.intensity.value, targetChromatic, ref chromSpeedRef, 0.3f);

        }
        if (v != null) 
        {
            v.intensity.value = Mathf.SmoothDamp(v.intensity.value, targetVignette, ref vignetteSpeedRef, 0.3f);
        }
    }
}
