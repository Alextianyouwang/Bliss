using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FolderManager : FileObject
{
    readonly string s_OpenFile = "OpenFile";
    public List<Transform> animatorHolder = new List<Transform>();
    public List<Transform> prefabHolder = new List<Transform>();

     private BoxCollider clickCollider;

    [SerializeField]
    public AnimationCurve filePopCurve;
    
    Vector3[] prefabOriginalScale;
    void Initialization()
    {
        clickCollider = GetComponent<BoxCollider>();
        prefabOriginalScale = new Vector3[ prefabHolder.Count];
        foreach (Transform Child in transform)
        {
            if (Child.GetComponent<Animator>() != null)
                animatorHolder.Add(Child);
        }
        for (int i = 0; i < prefabHolder.Count; i++)
        {
            prefabOriginalScale[i] = prefabHolder[i].localScale;
            prefabHolder[i].localScale = Vector3.zero;
            if (prefabHolder[i].GetComponent<FileObject>()) 
            {
                prefabHolder[i].GetComponent<FileObject>().parentFolder = this;
            }
        }
    }

    void SetCollider_fromBase(bool value) 
    {
        clickCollider.enabled = !value;
    }

    protected override void Start()
    {
        base.Start();
        Initialization();
    }
    void OnEnable()
    {
        OnFileAnimation = FileClickControl;
        OnTestingFileAnimationPreRoutine = SettingAndTestingAnimatorTargetValue;
        OnFileActivatedLocal = SetCollider_fromBase;
    }


    private bool SettingAndTestingAnimatorTargetValue(bool animState)
    {
        return base.SettingAndTestingAnimatorTargetValue_base(animatorHolder.Select(x => x.GetComponent<Animator>()).ToArray(), s_OpenFile.ToString(), "FileAnimation", animState);
    }
    public void FileClickControl(bool animState)
   {
        for (int i = 0; i < prefabHolder.Count; i++)
        {
            prefabHolder[i].localScale = Vector3.Lerp(Vector3.zero,
            prefabOriginalScale[i], filePopCurve.Evaluate(animationLerpValue));
        }
   }
}
