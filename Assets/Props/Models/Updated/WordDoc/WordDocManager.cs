using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class WordDocManager : FileObject
{
    [Space(20)]

    readonly string
        s_OpenFile = "OpenFile",
        s_DissolveMat = "M_Dissolve_WordDoc",
        s_ContentParent = "Content";
    public List<Transform> animatorHolder { get; private set; } = new List<Transform>();
    // Cannot use Get and Set if pass to imposter using Instantiate
    [HideInInspector]public List<Transform> dissolveMatHolder = new List<Transform>();
    public List<Transform> contentsHolder { get; private set; } = new List<Transform>();

    private Transform contentParent;

    public float minDissolve, maxDissolve;
    public int contentIndex;
    void Initialization()
    {
        foreach (Transform c in transform)
        {
            if (c.GetComponent<Animator>() != null)
                animatorHolder.Add(c);

            if (c.gameObject.GetComponent<MeshRenderer>())
                if (c.gameObject.GetComponent<MeshRenderer>()
                    .sharedMaterial.name.Equals(s_DissolveMat.ToString()))
                    dissolveMatHolder.Add(c);
            if (c.name == "Canvas")
                contentParent = c.Find(s_ContentParent);
        }
        foreach (Transform c in contentParent)
        {
            contentsHolder.Add(c);
            SetOpacity(0, c);
        }
       
    }
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        Initialization();
    }

    void OnEnable()
    {
        OnFileAnimation = FileClickControl;
        OnTestingFileAnimationPreRoutine = SettingAndTestingAnimatorTargetValue;
    }
    private bool SettingAndTestingAnimatorTargetValue(bool animState) 
    {
        return base.SettingAndTestingAnimatorTargetValue_base(animatorHolder.Select(x => x.GetComponent<Animator>()).ToArray(), s_OpenFile.ToString(), "FileAnimation", animState);
    }
    public void FileClickControl(bool animState)
    {
        float dissolveDistance = Mathf.Lerp(minDissolve, maxDissolve, animationLerpValue);
        foreach (Transform c in dissolveMatHolder)
        {
            c.GetComponent<MeshRenderer>().material.SetFloat("_WaveDistance", dissolveDistance);
        }
        SetOpacity(animationLerpValue, contentsHolder[contentIndex - 1]);
    }
    void SetOpacity(float value, Transform c) 
    {
        Image i = c.gameObject.GetComponent<Image>();
        TextMeshProUGUI t = c.gameObject.GetComponent<TextMeshProUGUI>();

        if (i != null) 
        {
            Color imgColor = i.color;
            imgColor.a = value;
            i.color = imgColor;
        }
        if (t != null)
        {
            Color txtColor = t.faceColor;
            txtColor.a = value;
            t.faceColor = txtColor;
        }
    }
}
