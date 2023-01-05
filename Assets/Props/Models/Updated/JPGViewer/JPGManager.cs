using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class JPGManager : FileObject
{
    readonly string
        s_OpenFile = "OpenFile",
        s_MatrixFadeMat = "M_MatrixFade",
        s_FragmentFadeMat = "M_FragmentFade",
        s_JPGParent = "Contents";

    public List<Transform> animatorHolder { get; private set; } = new List<Transform>();
    // Cannot use Get and Set if pass to imposter using Instantiate
    [HideInInspector]public List<Transform> matHolder= new List<Transform>();
    public List<Transform> contentsHolder { get; private set; } = new List<Transform>();

    private Transform JPGParent;

    public float minFade, minFadeMatrix, maxFade;
    public int contentIndex;

    void Initialization()
    {
        foreach (Transform c in transform)
        {
            if (c.GetComponent<Animator>() != null)
                animatorHolder.Add(c);

            if (c.gameObject.GetComponent<MeshRenderer>())
                if (c.gameObject.GetComponent<MeshRenderer>()
               .sharedMaterial.name.Equals(s_MatrixFadeMat.ToString()) ||
               c.gameObject.GetComponent<MeshRenderer>()
               .sharedMaterial.name.Equals(s_FragmentFadeMat.ToString()))
                    if (c.gameObject.activeInHierarchy)
                        matHolder.Add(c);
            if (c.name == "Canvas")
                JPGParent = c.Find(s_JPGParent);
        }
        foreach (Transform c in JPGParent)
        {
            contentsHolder.Add(c);
            SetOpacity(0, c);
        }
      
    }

    protected override void Start()
    {
        base.Start();
        Initialization();
    }

     void OnEnable()
    {
        OnFileAnimation = FileClickControl;
        OnTestingFileAnimationPreRoutine = (bool b) => true;
    }

    private bool SettingAndTestingAnimatorTargetValue(bool animState)
    {
        return base.SettingAndTestingAnimatorTargetValue_base(animatorHolder.Select(x => x.GetComponent<Animator>()).ToArray(), s_OpenFile.ToString(), "FileAnimation", animState);
    }
    public void FileClickControl(bool animState)
    {
        SettingAndTestingAnimatorTargetValue(animState);
        float fadeDistance = Mathf.Lerp(minFade, maxFade, animationLerpValue);
        float fadeDistanceMatrix = Mathf.Lerp(minFadeMatrix, maxFade, animationLerpValue);
        matHolder[0].GetComponent<MeshRenderer>().material.SetFloat("_WaveDistance", fadeDistanceMatrix);
        matHolder[1].GetComponent<MeshRenderer>().material.SetFloat("_WaveDistance", fadeDistance);
        SetOpacity(animationLerpValue, contentsHolder[contentIndex - 1]);
    }
    void SetOpacity(float value, Transform c)
    {
        Image i = c.gameObject.GetComponent<Image>();

        if (i != null)
        {
            Color imgColor = i.color;
            imgColor.a = value;
            i.color = imgColor;
        }
    }

}
