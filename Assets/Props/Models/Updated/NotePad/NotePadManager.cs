using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotePadManager : FileObject, IClickable
{
    public bool FileDebugger = false, IndividualDebugger = false;

    string s_OpenFile = "OpenFile";

    [SerializeField]
    private List<Transform> animatorHolder = new List<Transform>();

    [Header("ContentHolder - TMP")]
    public Transform TMPParent;
    public List<Transform> contentsHolderT = new List<Transform>();

    public int ContentsIndex;

    void Initialization()
    {
        ContentInitialization(ContentsIndex);

        foreach (Transform Child in this.gameObject.transform)
        {
            if (Child.GetComponent<Animator>() != null)
                animatorHolder.Add(Child);
        }
    }

    void ContentInitialization(int contentIndex)
    {
        int listIndex = contentIndex - 1;

        foreach (Transform Contents in TMPParent)
        {
            contentsHolderT.Add(Contents);
        }

        foreach (var Child in contentsHolderT)
        {
            if (contentsHolderT.IndexOf(Child) != listIndex)
                Child.gameObject.SetActive(false);
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
