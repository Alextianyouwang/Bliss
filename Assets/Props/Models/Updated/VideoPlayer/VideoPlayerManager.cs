using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoPlayerManager : MonoBehaviour, IClickable
{

    public bool FileDebugger = false, IndividualDebugger = false;

    string s_OpenFile = "OpenFile", s_Display = "VideoPlayer_Display";

    [SerializeField]
    private List<Transform> animatorHolder = new List<Transform>();
    GameObject display;
    float scaleRef = 0;

    public AnimationCurve displayIncre;

    public float lerpMultiplier = 2f;

    void Initialization()
    { 
        foreach (Transform Child in this.gameObject.transform)
        {
            if (Child.GetComponent<Animator>() != null)
                animatorHolder.Add(Child);
        }

        display = GameObject.Find(s_Display.ToString());
        display.transform.localPosition = Vector3.zero;
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialization();
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
        }      

        if (animState)
        {
            scaleRef = Utility.LerpHelper(ref scaleRef, targetValue, lerpMultiplier);
            scaleRef = scaleRef >= targetValue ? targetValue : scaleRef;
        }
        else
        {
            scaleRef = Utility.LerpHelper(ref scaleRef, targetValue, lerpMultiplier * 5);
            scaleRef = scaleRef <= targetValue ? targetValue : scaleRef;
        }

        float scaleIncre = displayIncre.Evaluate(scaleRef);

        display.transform.localScale = new Vector3(scaleIncre, scaleIncre, scaleIncre);
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
