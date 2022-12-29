using System.Collections;   
using System.Collections.Generic;
using UnityEngine;

public class FolderManager : MonoBehaviour, IClickable
{

    public bool FileDebugger = false, IndividualDebugger = false;

    string s_OpenFile = "OpenFile";
    bool canPop = false;
    public List<Transform> animatorHolder = new List<Transform>();

    public List<Transform> prefabHolder = new List<Transform>();

    [SerializeField]
    private float lerperVar = 1f, lerpMultiplier = 2f;
    [SerializeField]
    private float prefabOriginalScale, scaleRef = 0;


    void Initialization()
    {
        foreach (Transform Child in this.gameObject.transform)
        {
            if (Child.GetComponent<Animator>() != null)
                animatorHolder.Add(Child);
        }
        foreach (Transform Child in prefabHolder)
        {
            Child.localScale = Vector3.zero;
        }
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

        targetValue = targetValue == 0 ? 0 : prefabOriginalScale;
        foreach (Transform Child in prefabHolder)
        {
            if(canPop)
            {
                scaleRef = Utility.LerpHelper(ref scaleRef, targetValue, lerpMultiplier);
                scaleRef = scaleRef >= targetValue ? targetValue : scaleRef;
            }
            if (!animState)
            {
                scaleRef = Utility.LerpHelper(ref scaleRef, targetValue, lerpMultiplier);
                scaleRef = scaleRef <= targetValue ? targetValue : scaleRef;
            }

            Child.localScale = new Vector3(scaleRef, scaleRef, scaleRef);
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
