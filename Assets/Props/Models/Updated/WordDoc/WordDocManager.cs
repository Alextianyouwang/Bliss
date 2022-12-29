using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class WordDocManager : MonoBehaviour, IClickable
{

    string s_OpenFile = "OpenFile";
    bool canDissolve = false;

    public List<Transform> animatorHolder = new List<Transform>();
    public List<Transform> dissolveMatHolder = new List<Transform>();

    [SerializeField]
    private bool FileDebugger = false, IndividualDebugger = false;

    [Header("MaterialAttributes")]
    public float minDissolve; public float maxDissolve;
    [SerializeField]
    private float lerperVar = 0f, lerpMultiplier = 2f, dissolveDistance;
    string s_DissolveMat = "M_Dissolve_WordDoc";

    [Header("MaterialAttributes")]
    public TextMeshProUGUI txt;
    float txtLerper = 0;
    public Image img;

    [Header("ContentHolder - TMP")]
    public Transform TMPParent;
    public List<Transform> contentsHolderT = new List<Transform>();
    [Header("ContentHolder - IMAGES")]
    public Transform IMGParent;
    public List<Transform> contentsHolderI = new List<Transform>();

    public int ContentsIndex;
    public bool UsingImages = false;

    void Initialization()
    {
        ContentInitialization(ContentsIndex, UsingImages);

        foreach (Transform Child in this.gameObject.transform)
        {
            if (Child.GetComponent<Animator>() != null)
                animatorHolder.Add(Child);
            if (Child.gameObject.GetComponent<MeshRenderer>()
                .sharedMaterial.name.Equals(s_DissolveMat.ToString()))
                dissolveMatHolder.Add(Child);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialization();
        txt = GetComponent<TextMeshProUGUI>();
        img = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IndividualDebugger)
            Debugger();
    }

    public void FileClickControl(bool animState, float targetValue)
    {
        foreach (Transform Child in animatorHolder)
        {
            Animator anim = Child.GetComponent<Animator>();
            anim.SetBool(s_OpenFile.ToString(), animState);
            if (animState)
            {
                if (anim.GetCurrentAnimatorStateInfo(0).IsTag("FileAnimation") &&
                    anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !anim.IsInTransition(0))
                    canDissolve = true;
                else
                    canDissolve = false;
            }
        }
        foreach(Transform Child in dissolveMatHolder)
        {
            if (animState)
            {
                lerperVar = canDissolve ? Utility.LerpHelper(ref lerperVar, targetValue, lerpMultiplier) : lerperVar;
            }
            else
            {
                lerperVar = Utility.LerpHelper(ref lerperVar, targetValue, lerpMultiplier * 3);
                txtLerper = Utility.LerpHelper(ref txtLerper, targetValue, lerpMultiplier * 15);
            }
            dissolveDistance = Mathf.Lerp(minDissolve, maxDissolve, lerperVar); 
            if(dissolveDistance >= maxDissolve && animState)
                txtLerper = canDissolve ? Utility.LerpHelper(ref txtLerper, targetValue, 0.3f) : lerperVar;

            Child.GetComponent<MeshRenderer>().material.SetFloat("_WaveDistance", dissolveDistance);
        }

        ContentSelection(ContentsIndex, UsingImages);
    }

    void ContentInitialization(int contentIndex, bool IMGYes)
    {
        int listIndex = contentIndex - 1;

        foreach(Transform Contents in TMPParent)
        {
            contentsHolderT.Add(Contents);
        }
        foreach (Transform Contents in IMGParent)
        {
            contentsHolderI.Add(Contents);
        }

        if (IMGYes)
        {
            foreach(var Child in contentsHolderT)
            {
                Child.gameObject.SetActive(false);
            }
            foreach(var Child in contentsHolderI)
            {
                if (contentsHolderI.IndexOf(Child) != listIndex)
                    Child.gameObject.SetActive(false);
            }
        }
        else
        {
            foreach (var Child in contentsHolderI)
            {
                Child.gameObject.SetActive(false);
            }
            foreach (var Child in contentsHolderT)
            {
                if (contentsHolderT.IndexOf(Child) != listIndex)
                    Child.gameObject.SetActive(false);
            }
        }
    }

    void ContentSelection(int contentIndex, bool IMGYes)
    {
        int listIndex = contentIndex - 1;

        if (IMGYes)
        {
            Color imgColor = contentsHolderI[listIndex].gameObject.GetComponent<Image>().color;
            imgColor.a = txtLerper;
            contentsHolderI[listIndex].gameObject.GetComponent<Image>().color = imgColor;
        }
        else
        {
            Color txtColor = contentsHolderT[listIndex].gameObject.GetComponent<TextMeshProUGUI>().faceColor;
            txtColor.a = txtLerper;
            contentsHolderT[listIndex].gameObject.GetComponent<TextMeshProUGUI>().faceColor = txtColor;
        }
    }
    void Debugger()
    {
        //Debugging section. Use Interface in build
        if (FileDebugger)
            FileClickControl(true, 1f);
        else
            FileClickControl(false, 0);
    }
}
