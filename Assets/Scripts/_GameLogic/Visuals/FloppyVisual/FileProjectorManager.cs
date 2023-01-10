using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class FileProjectorManager : MonoBehaviour
{
    public FileLightData nullStateLightData;
    private FileLightData activatedStateLightData;
    public void SetFileLightData(FileLightData data)
    {
        activatedStateLightData = data;
    }

    //Animation parameters
    readonly string
        s_Saved = "FileSaved",
        s_Null = "FileNull";
    //Material parameters
    readonly string s_ProjectorLights = "ProjectorLights";
    //Parent names
    readonly string
        s_RingParent = "RimTopGroup",
        s_RingBase = "BaseGroup",
        s_Lights = "Lights";


    private Transform
        ringParent,
        ringBaseParent,
        lightsParent;
    private List<Animator> animRings = new List<Animator>();
    private List<Material> ringMats = new List<Material>();
    private List<HDAdditionalLightData> animLights = new List<HDAdditionalLightData>();

    public Vector3 GetFileLoadPosition()
    {
        return transform.Find("FileLoadingPoint").position;
    }
    public FileObject contianedFile { get; private set; }
    public void SetContainedFile(FileObject f)
    {
        contianedFile = f;
    }
    public bool isOccupied { get; private set; }
    public static bool isPerformingFileDisplayAnimation;


    private void OnEnable()
    {
        SceneSwitcher.OnFloppyToggle += TurnOnWhenPlayerEnterFloppy;
    }
    private void OnDisable()
    {
        SceneSwitcher.OnFloppyToggle -= TurnOnWhenPlayerEnterFloppy;

    }

    void Initialization()
    {
        if (nullStateLightData == null)
            Debug.LogWarning("Please Assign Light Data for " + name);
        foreach (Transform c in transform)
        {
            if (c.name == s_RingParent)
                ringParent = c;
            if (c.name == s_RingBase)
                ringBaseParent = c;
            if (c.name == s_Lights)
                lightsParent = c;
        }
        foreach (Transform Child in ringParent)
        {
            if (Child.gameObject.GetComponent<MeshRenderer>().sharedMaterials[1].name.Equals(s_ProjectorLights.ToString()))
                ringMats.Add(Child.GetComponent<MeshRenderer>().materials[1]);
            if (Child.gameObject.GetComponent<Animator>() != null)
                animRings.Add(Child.GetComponent<Animator>());
        }
        foreach (Transform Child in ringBaseParent)
        {
            if (Child.gameObject.GetComponent<Animator>() != null)
                animRings.Add(Child.GetComponent<Animator>());
        }

        foreach (Transform Child in lightsParent)
        {
            if (Child.gameObject.GetComponent<Light>() != null)
                animLights.Add(Child.GetComponent<HDAdditionalLightData>());
        }
        InitializeColor();
    }

    void InitializeColor()
    {
        foreach (Material mat in ringMats)
            mat.SetColor("_EmissiveColor", nullStateLightData.matColor);
        foreach (HDAdditionalLightData light in animLights)
            light.GetComponent<HDAdditionalLightData>().SetColor(nullStateLightData.lightColor);
    }


    void Awake()
    {
        Initialization();
    }
    public void TurnOff()
    {
        isOccupied = false;
        StartCoroutine( ProjectorAnimation(0.4f, -3f, false));
    }

    void TurnOnWhenPlayerEnterFloppy(bool inFloppy) 
    {
        // Animator will reset is value back to 0 upon disable, need to set it back to one if it is previously activated
        if (isOccupied)
            SetAnimationState(3f, true);
        // For the most recent saved file, the lerp animation will refresh each time when player is teleported into floppy.
        if (contianedFile != null && contianedFile == SceneSwitcher.sd.mostRecentSavedFile && inFloppy) 
        {
            isOccupied = true;
            StartCoroutine( ProjectorAnimation(0.4f, 3f, true));
        }
    }
    IEnumerator ProjectorAnimation(float speed, float animMultiplier, bool open)
    {
        float percent = 0;
        float initialValue = open ? 0 : 1;
        float targetValue = open ? 1 : 0;
        SetAnimationState(animMultiplier, open);
        isPerformingFileDisplayAnimation = true;
        while (percent < 1f)
        {

            percent += Time.deltaTime * speed;
            SetMatAndLightLerpValue(Mathf.Lerp(initialValue, targetValue, percent));
            yield return null;
        }
        isPerformingFileDisplayAnimation = false;

    }

    void SetAnimationState(float animMultiplier, bool animState) 
    {
        foreach (Animator anim in animRings)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsTag("ProjectorRings")
                && anim.GetFloat(s_Null.ToString()) == animMultiplier)
                continue;
            anim.SetBool(s_Saved.ToString(), animState);
        }
    }
    void SetMatAndLightLerpValue(float lerpValue)
    {
        if (!nullStateLightData || !activatedStateLightData)
            return;
        foreach (Material mat in ringMats)
        {
            Color c_MatSavedColor = Color.Lerp(nullStateLightData.matColor, activatedStateLightData.matColor, lerpValue);
            mat.SetColor("_EmissiveColor", c_MatSavedColor);
        }
        foreach (HDAdditionalLightData light in animLights)
        {
            Color c_LightColor = Color.Lerp(nullStateLightData.lightColor, activatedStateLightData.lightColor, lerpValue);
            light.GetComponent<HDAdditionalLightData>().SetColor(c_LightColor);
        }
    }
}
