using System.Collections;   
using System.Collections.Generic;
using UnityEngine;

public class FolderManager : FileObject, IClickable
{

    public bool FileDebugger = false, IndividualDebugger = false;

    string s_OpenFile = "OpenFile";
    bool canPop = false;
    public List<Transform> animatorHolder = new List<Transform>();

    public List<Transform> prefabHolder = new List<Transform>();


    [SerializeField]
    private AnimationCurve filePopCurve;
    [SerializeField]
    private float lerpMultiplier = 2f;
/*    [SerializeField]
    private float  scaleRef = 0;*/

    Vector3[] prefabOriginalScale;
    void Initialization()
    {
        prefabOriginalScale = new Vector3[ prefabHolder.Count];
        foreach (Transform Child in this.gameObject.transform)
        {
            if (Child.GetComponent<Animator>() != null)
                animatorHolder.Add(Child);
        }
        for (int i = 0; i < prefabHolder.Count; i++)
        {
            prefabOriginalScale[i] = prefabHolder[i].localScale;

            prefabHolder[i].localScale = Vector3.zero;

        }
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
            if (animState)
            {
                if (anim.GetCurrentAnimatorStateInfo(0).IsTag("FileAnimation") &&
                    anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !anim.IsInTransition(0))
                    canPop = true;
                else
                    canPop = false;
            }
            else
            {
                canPop = false;
            }
        }

        //targetValue = targetValue == 0 ? 0 : prefabOriginalScale;
        for (int i = 0; i < prefabHolder.Count; i++)

        {
            /*if (canPop)
              {
                //scaleRef = Utility.LerpHelper(ref scaleRef, targetValue, lerpMultiplier);
               //scaleRef = scaleRef >= targetValue ? targetValue : scaleRef;
              }
              if (!animState)
              {
               //scaleRef = Utility.LerpHelper(ref scaleRef, targetValue, lerpMultiplier);
               //scaleRef = scaleRef <= targetValue ? targetValue : scaleRef;
              }*/

            //To ensure the contents will only fade after all animation has finished playing when clicked OPEN, not CLOSED.
            if (!animState)
                if (!animState)
                prefabHolder[i].localScale = Vector3.Lerp(Vector3.zero, 
                    prefabOriginalScale[i], filePopCurve.Evaluate(animationLerpValue));
            else
                if(canPop)
                {
                    prefabHolder[i].localScale = Vector3.Lerp(Vector3.zero,
                    prefabOriginalScale[i], filePopCurve.Evaluate(animationLerpValue)); //Need different animLerpValue.
                }
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
