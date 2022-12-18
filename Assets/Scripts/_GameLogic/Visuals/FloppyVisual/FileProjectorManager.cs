using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class FileProjectorManager : MonoBehaviour
{
    [Header("NullState")]
    [ColorUsage(true, false)]
    public Color MatNullColor;
    public Color LightsNullColor;

    [Header("SavedState")]
    [ColorUsage(true, false)]
    public Color MatSavedColor;
    public Color LightsSavedColor;

    //Animation parameters
    string s_Saved = "FileSaved", s_Null = "FileNull";
    //Material parameters
    string s_ProjectorLights = "ProjectorLights";

    [Header("RingsGroup")]
    public Transform RingParent;
    public Transform RingBottomParent;
    [SerializeField]
    private List<Transform> AnimRings = new List<Transform>();
    private List<Transform> AnimBottomRings = new List<Transform>();
    [SerializeField]
    private List<Transform> AllRings = new List<Transform>();

    [Header("LightsGroup")]
    public Transform LightsParent;
    [SerializeField]
    private List<Transform> AnimLights = new List<Transform>();
    public float LerperVar = 0f;

    //Debugger boolean state
    [SerializeField]
    private bool FileSaveDebugger = false;
    [SerializeField]
    private float LerpMultiplier = 0.5f;

    void StartStateDeclaration()
    {
        FileSavedState(-2f, 0, false);

        foreach (Transform Child in RingParent)
        {
            if (Child.gameObject.GetComponent<MeshRenderer>().sharedMaterials[1].name.Equals(s_ProjectorLights.ToString()))
                AllRings.Add(Child);     
            if (Child.gameObject.GetComponent<Animator>() != null)
                AnimRings.Add(Child);
        }
        foreach(Transform Child in RingBottomParent)
        {
            if (Child.gameObject.GetComponent<Animator>() != null)
                AnimRings.Add(Child);
        }

        foreach(Transform Child in LightsParent)
        {
            if (Child.gameObject.GetComponent<Light>() != null)
                AnimLights.Add(Child);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartStateDeclaration();
    }

    // Update is called once per frame
    void Update()
    {
        if (FileSaveDebugger)
            FileSavedState(2f, 1f, true);
        else
            FileSavedState(-2f, 0, false);
    }

    public void FileSavedState(float animMultiplier, float TargetValue, bool animState)
    {
        LerperVar = Utility.LerpHelper(ref LerperVar, TargetValue, LerpMultiplier);
        foreach (Transform Child in AllRings)
        {
           
            Color c_MatSavedColor = Color.Lerp(MatNullColor, MatSavedColor, LerperVar);
            Child.GetComponent<MeshRenderer>().materials[1].SetColor("_EmissiveColor", c_MatSavedColor);
        }
        foreach(Transform Child in AnimLights)
        {
            Color c_LightColor = Color.Lerp(LightsNullColor, LightsSavedColor, LerperVar);
            Child.GetComponent<HDAdditionalLightData>().SetColor(c_LightColor, 5626);
            //print(c_LightColor);
        }
        foreach (Transform Child in AnimRings)
        {
            Animator a_Anim;
            a_Anim = Child.gameObject.GetComponent<Animator>();

            if (a_Anim.GetCurrentAnimatorStateInfo(0).IsTag("ProjectorRings")
                && a_Anim.GetFloat(s_Null.ToString()) == animMultiplier)
                return;

            a_Anim.SetBool(s_Saved.ToString(), animState);
            //a_Anim.SetFloat(s_Null.ToString(), animMultiplier);
        }
    }

}
