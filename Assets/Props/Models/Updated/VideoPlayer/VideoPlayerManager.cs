using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoPlayerManager : FileObject, IClickable
{

    public bool FileDebugger = false, IndividualDebugger = false;

    string s_OpenFile = "OpenFile", s_Display = "VideoPlayer_Display";

    Vector3 displayOriginalScale;

    [SerializeField]
    private List<Transform> animatorHolder = new List<Transform>();
    GameObject display;

    public AnimationCurve displayIncre;

    public float lerpMultiplier = 2f;

    void Initialization()
    { 
        foreach (Transform Child in this.gameObject.transform)
        {
            if (Child.GetComponent<Animator>() != null)
                animatorHolder.Add(Child);
        }

        display = transform.Find(s_Display.ToString()).gameObject;
        displayOriginalScale = display.transform.localScale;
        display.transform.localPosition = Vector3.zero;
        display.transform.localScale = Vector3.zero;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        OnFileAnimation = FileClickControl;
        Initialization();
    }

    // Update is called once per frame
    void Update()
    {
        //if (IndividualDebugger)
            //Debugger();
    }

    public void FileClickControl(bool animState, float targetValue)
    {
        foreach (Transform Child in animatorHolder)
        {
            Animator anim = Child.GetComponent<Animator>();
            anim.SetBool(s_OpenFile.ToString(), animState);
        }      

        if (animState)
        {
            //scaleRef = Utility.LerpHelper(ref scaleRef, targetValue, lerpMultiplier);
            //scaleRef = targetValue;
            //scaleRef = scaleRef >= targetValue ? targetValue : scaleRef;
        }
        else
        {
            //scaleRef = Utility.LerpHelper(ref scaleRef, targetValue, lerpMultiplier * 5);
            //scaleRef = targetValue;
            //scaleRef = scaleRef <= targetValue ? targetValue : scaleRef;
        }
  
        display.transform.localScale = Vector3.Lerp(Vector3.zero,displayOriginalScale, displayIncre.Evaluate(animationLerpValue));
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
