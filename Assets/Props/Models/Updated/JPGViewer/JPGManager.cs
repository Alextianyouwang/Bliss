using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JPGManager : FileObject, IClickable
{
    public bool FileDebugger = false, IndividualDebugger = false;

    string s_OpenFile = "OpenFile";

    public List<Transform> animatorHolder = new List<Transform>();
    public List<Transform> matHolder = new List<Transform>();

    bool canFade = false;

    [Header("MaterialAttributes")]
    string s_MatrixFadeMat = "M_MatrixFade"; string s_FragmentFadeMat = "M_FragmentFade";
    public float minFade, minFadeMatrix; public float maxFade;
    [SerializeField]
    private float lerpMultiplier = 2f, fadeDistance, fadeDistanceMatrix;

    void Initialization()
    {
        foreach (Transform Child in this.gameObject.transform)
        {
            if (Child.GetComponent<Animator>() != null)
                animatorHolder.Add(Child);


            if (Child.gameObject.GetComponent<MeshRenderer>())

                if (Child.gameObject.GetComponent<MeshRenderer>()
               .sharedMaterial.name.Equals(s_MatrixFadeMat.ToString()) ||
               Child.gameObject.GetComponent<MeshRenderer>()
               .sharedMaterial.name.Equals(s_FragmentFadeMat.ToString()))
                if(Child.gameObject.activeInHierarchy)
                    matHolder.Add(Child);
        }
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        Initialization();
    }

     void OnEnable()
    {
        OnFileAnimation = FileClickControl;
    }
    // Update is called once per frame
    void Update()
    {
        //if(IndividualDebugger)
            //Debugger();
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
                    canFade = true;
                else
                    canFade = false;
            }
        }
        foreach (Transform Child in matHolder)
        {
            if (animState)
            {
                //lerperVar = canFade ? Utility.LerpHelper(ref lerperVar, targetValue, lerpMultiplier) : lerperVar;
                animationLerpValue = canFade ? targetValue: animationLerpValue;
            }
            else
            {
                //lerperVar = Utility.LerpHelper(ref lerperVar, targetValue, lerpMultiplier);
                animationLerpValue = targetValue;
            }
            fadeDistance = Mathf.Lerp(minFade, maxFade, animationLerpValue);
            fadeDistanceMatrix = Mathf.Lerp(minFadeMatrix, maxFade, animationLerpValue);
            //Child.GetComponent<MeshRenderer>().material.SetFloat("_WaveDistance", fadeDistance);
        }

        matHolder[0].GetComponent<MeshRenderer>().material.SetFloat("_WaveDistance", fadeDistanceMatrix);
        matHolder[1].GetComponent<MeshRenderer>().material.SetFloat("_WaveDistance", fadeDistance);
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
