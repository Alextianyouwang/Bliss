using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class NotePadManager : FileObject
{
    [Space(20)]

    readonly string 
        s_OpenFile = "OpenFile", 
        s_TMPParent = "TMPContents";

    public List<Transform> animatorHolder { get; private set; } = new List<Transform>();
    public List<Transform> contentsHolderT { get; private set; } = new List<Transform>();

    private Transform TMPParent;
    public int contentIndex;

    void Initialization()
    {
        foreach (Transform c in transform)
        {
            if (c.GetComponent<Animator>() != null)
                animatorHolder.Add(c);
            if (c.name == "Canvas")
                TMPParent = c.Find(s_TMPParent);
        }
        ContentInitialization(contentIndex);
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
    protected override void Start()
    {
        base.Start();
        Initialization();
    }

    void OnEnable()
    {
        OnFileAnimation = FileClickControl;
        OnTestingFileAnimationPreRoutine = (bool b) => true;
    }
    public void FileClickControl(bool animState)
    {
        base.SettingAnimatorTargetValue_base(animatorHolder.Select(x => x.GetComponent<Animator>()).ToArray(), s_OpenFile.ToString(), animState);
    }
}
