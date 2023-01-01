using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class WordDocManager : FileObject
{
    readonly string 
        s_OpenFile = "OpenFile", 
        s_DissolveMat = "M_Dissolve_WordDoc";

    [HideInInspector] public List<Transform> animatorHolder = new List<Transform>();
    [HideInInspector] public List<Transform> dissolveMatHolder = new List<Transform>();
    [HideInInspector] public List<Transform> contentsHolder = new List<Transform>();
    public float minDissolve, maxDissolve;
    [SerializeField] private Transform contentParent;
    public int contentIndex;
    void Initialization()
    {
        foreach (Transform c in contentParent)
        {
            contentsHolder.Add(c);
            SetOpacity(0, c);
        }
        foreach (Transform c in this.gameObject.transform)
        {
            if (c.GetComponent<Animator>() != null)
                animatorHolder.Add(c);

            if (c.gameObject.GetComponent<MeshRenderer>())
            if (c.gameObject.GetComponent<MeshRenderer>()
                .sharedMaterial.name.Equals(s_DissolveMat.ToString()))
                dissolveMatHolder.Add(c);
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
