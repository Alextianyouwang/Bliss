using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class WordDocManager : FileObject
{
    string s_OpenFile = "OpenFile";
    string s_DissolveMat = "M_Dissolve_WordDoc";

    // I dont know why but if those list are private the dissolve logic of instantiated files in floppy world doesn't work.
    [HideInInspector]public List<Transform> animatorHolder = new List<Transform>();
    [HideInInspector] public List<Transform> dissolveMatHolder = new List<Transform>();
    [HideInInspector] public List<Transform> contentsHolder = new List<Transform>();

    [Header("MaterialAttributes")]
    public float minDissolve; public float maxDissolve;

    [Header("Please Put In ContentHolder")]
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
    }
    public void FileClickControl(bool animState, float targetValue)
    {
        foreach (Transform c in animatorHolder)
        {
            Animator anim = c.GetComponent<Animator>();
            anim.SetBool(s_OpenFile.ToString(), animState);
        }

        float dissolveDistance = Mathf.Lerp(minDissolve, maxDissolve, animationLerpValue);
        foreach (Transform c in dissolveMatHolder)
        {
            c.GetComponent<MeshRenderer>().material.SetFloat("_WaveDistance", dissolveDistance);
        }
        SetOpacity(animationLerpValue,contentsHolder[contentIndex]);
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
