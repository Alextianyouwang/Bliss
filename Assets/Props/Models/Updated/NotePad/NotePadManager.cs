using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class NotePadManager : FileObject
{
    readonly string s_OpenFile = "OpenFile";

    [HideInInspector]public List<Transform> animatorHolder = new List<Transform>();
    public Transform TMPParent;
    [HideInInspector]public List<Transform> contentsHolderT = new List<Transform>();
    public int contentsIndex;

    void Initialization()
    {
        ContentInitialization(contentsIndex);

        foreach (Transform Child in transform)
        {
            if (Child.GetComponent<Animator>() != null)
                animatorHolder.Add(Child);
        }
    }

    void ContentInitialization(int contentIndex)
    {
        int listIndex = contentIndex;

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
